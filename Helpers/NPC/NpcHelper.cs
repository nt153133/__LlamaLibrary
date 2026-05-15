using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers.NPC
{
    /// <summary>
    /// Provides static utility methods for finding, sorting, and resolving display names for FFXIV NPCs.
    /// Handles both standard ENpcResident characters (IDs &gt; 1,000,000) and event objects (IDs &gt; 2,000,000),
    /// with name lookup results cached in memory to avoid repeated game function calls.
    /// </summary>
    public static class NpcHelper
    {
        private static Dictionary<uint, string>? EventObjectNames;
        private static Dictionary<uint, (string Name, string Plural, string Title)> _ENpcResident = new();

        

        /// <summary>
        /// From a collection of <see cref="Npc"/> instances, returns the single closest reachable NPC
        /// using this priority order: current zone &gt; current aetheryte area &gt; lowest teleport cost &gt; shortest 2D distance.
        /// If no NPC is in the current zone, distance is measured from each NPC's nearest aetheryte instead of the player.
        /// </summary>
        /// <param name="npcs">The candidate NPCs to evaluate.</param>
        /// <returns>The optimal reachable <see cref="Npc"/>, or <see langword="null"/> if none are reachable.</returns>
        public static Npc? GetClosestNpc(IEnumerable<Npc> npcs)
        {
            var enumerable = npcs.ToList();
            if (enumerable.Any(i => i.IsInCurrentZone))
            {
                var meLocation = Core.Me.Location;
                return enumerable.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(meLocation)).FirstOrDefault();
            }

            return enumerable.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(i.Location.ClosestAetheryteResult.Position)).FirstOrDefault();
        }

        /// <summary>
        /// Sorts a collection of <see cref="Npc"/> instances by reachability using the same priority order as
        /// <see cref="GetClosestNpc"/>: current zone &gt; current area &gt; teleport cost &gt; 2D distance.
        /// Only reachable NPCs (<see cref="Npc.CanGetTo"/> is <see langword="true"/>) are included in the result.
        /// </summary>
        /// <param name="npcs">The candidate NPCs to sort.</param>
        /// <returns>A list of reachable <see cref="Npc"/> instances ordered from nearest to farthest.</returns>
        public static List<Npc> OrderByDistance(IEnumerable<Npc> npcs)
        {
            var enumerable = npcs.ToList();
            if (enumerable.Any(i => i.IsInCurrentZone))
            {
                var meLocation = Core.Me.Location;
                return enumerable.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(meLocation)).ToList();
            }

            return enumerable.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(i.Location.ClosestAetheryteResult.Position)).ToList();
        }

        /// <summary>
        /// Resolves the localized display name for an NPC by its game ID.
        /// <list type="bullet">
        ///   <item><description>IDs &gt; 2,000,000 are event objects; the name is read from the local SQLite database via <see cref="GetEventObjectName"/>.</description></item>
        ///   <item><description>IDs &gt; 1,000,000 are standard ENpcResident NPCs; the name is resolved via <see cref="CallGetENpcResident"/>.</description></item>
        ///   <item><description>IDs ≤ 1,000,000 return an empty string (unused or internal entity range).</description></item>
        /// </list>
        /// </summary>
        /// <param name="npcId">The game-internal NPC or event object identifier.</param>
        /// <param name="includeTitle">
        /// When <see langword="true"/> (default), appends the NPC's title in parentheses if one exists,
        /// e.g. <c>"Baderon (Proprietor)"</c>.
        /// </param>
        /// <returns>The localized display name, or an empty string if the NPC cannot be resolved.</returns>
        public static string GetNpcName(uint npcId, bool includeTitle = true)
        {
            switch (npcId)
            {
                case > 2_000_000:
                    return GetEventObjectName(npcId);
                case <= 1_000_000:
                    return "";
                default:
                    (var name, _, var title) = CallGetENpcResident(npcId);
                    return (includeTitle && title != "") ? $"{name} ({title})" : name;
            }
        }

        /// <summary>
        /// Resolves the display name of an FFXIV event object by its ID, reading from the local SQLite
        /// database file <c>db.s3db</c> on first access. Results are cached for the lifetime of the session.
        /// If <paramref name="npcId"/> exceeds 2,000,000, the base offset is subtracted before the lookup.
        /// </summary>
        /// <param name="npcId">The event object identifier (raw value or with the 2,000,000 offset applied).</param>
        /// <returns>The localized name of the event object, or an empty string if not found.</returns>
        public static string GetEventObjectName(uint npcId)
        {
            if (EventObjectNames == null)
            {
                using var Database = new Database("db.s3db");
                var results = Database.AllAsDictionary<EventObjectResult>();
                EventObjectNames = new Dictionary<uint, string>(results.Count);
                foreach (var objectResult in results.Where(objectResult => objectResult.Value.CurrentLocaleName != ""))
                {
                    EventObjectNames.Add(objectResult.Key, objectResult.Value.CurrentLocaleName);
                }

                results.Clear();
            }

            if (npcId > 2_000_000)
            {
                npcId -= 2_000_000;
            }

            EventObjectNames.TryGetValue(npcId, out var value);
            return value ?? "";
        }

        /// <summary>
        /// Calls the injected FFXIV game function <c>GetENpcResident</c> to retrieve the name, plural form,
        /// and title for a standard NPC by its ENpcResident ID. Results are cached in memory after the first call.
        /// Returns a tuple of empty strings if the game returns a null pointer for the given ID.
        /// </summary>
        /// <param name="npcId">The ENpcResident ID of the NPC (expected to be &gt; 1,000,000).</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description><c>Name</c> — the NPC's primary display name.</description></item>
        ///   <item><description><c>Plural</c> — the plural form of the name (used in some UI contexts).</description></item>
        ///   <item><description><c>Title</c> — the NPC's title or role descriptor, e.g. <c>"Innkeeper"</c>.</description></item>
        /// </list>
        /// </returns>
        public static (string Name, string Plural, string Title) CallGetENpcResident(uint npcId)
        {
            if (_ENpcResident.TryGetValue(npcId, out var value))
            {
                return value;
            }

            var ptr = Core.Memory.CallInjectedWraper<IntPtr>(NpcHelperOffsets.GetENpcResident, npcId);

            if (ptr == IntPtr.Zero)
            {
                return ("", "", "");
            }

            var name = Core.Memory.ReadStringUTF8(ptr + 0x18);
            var plural = Core.Memory.ReadStringUTF8(ptr + 0x18 + name.Length + 1);
            var title = Core.Memory.ReadStringUTF8(ptr + 0x18 + name.Length + 1 + 1 + plural.Length);
            _ENpcResident.Add(npcId, (name, plural, title));
            return (name, plural, title);
        }
    }
}