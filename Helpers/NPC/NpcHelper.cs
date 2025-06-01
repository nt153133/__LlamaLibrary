using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers.NPC
{
    public static class NpcHelper
    {
        private static Dictionary<uint, string>? EventObjectNames;
        private static Dictionary<uint, (string Name, string Plural, string Title)> _ENpcResident = new();

        private static class Offsets
        {
            //7.1
            [Offset("Search E8 ? ? ? ? 0F B6 48 ? 85 C9 0F 84 ? ? ? ? TraceCall")]
            //[OffsetCN("Search E8 ? ? ? ? 48 89 45 AF 48 8B D8 TraceCall")]
            internal static IntPtr GetENpcResident;
        }

        public static Npc? GetClosestNpc(IEnumerable<Npc> npcs)
        {
            if (npcs.Any(i => i.IsInCurrentZone))
            {
                var meLocation = Core.Me.Location;
                return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(meLocation)).FirstOrDefault();
            }

            return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(i.Location.ClosestAetheryteResult.Position)).FirstOrDefault();
        }

        public static List<Npc> OrderByDistance(IEnumerable<Npc> npcs)
        {
            if (npcs.Any(i => i.IsInCurrentZone))
            {
                var meLocation = Core.Me.Location;
                return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(meLocation)).ToList();
            }

            return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(i.Location.ClosestAetheryteResult.Position)).ToList();
        }

        public static string GetNpcName(uint npcId, bool includeTitle = true)
        {
            if (npcId > 2_000_000)
            {
                return GetEventObjectName(npcId);
            }

            if (npcId <= 1_000_000)
            {
                return "";
            }

            (var name, _, var title) = CallGetENpcResident(npcId);
            return (includeTitle && title != "") ? $"{name} ({title})" : name;
        }

        public static string GetEventObjectName(uint npcId)
        {
            if (EventObjectNames == null)
            {
                using var Database = new Database("db.s3db");
                var results = Database.AllAsDictionary<EventObjectResult>();
                EventObjectNames = new Dictionary<uint, string>(results.Count);
                foreach (var objectResult in results)
                {
                    if (objectResult.Value.CurrentLocaleName != "")
                    {
                        EventObjectNames.Add(objectResult.Key, objectResult.Value.CurrentLocaleName);
                    }
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

        public static (string Name, string Plural, string Title) CallGetENpcResident(uint npcId)
        {
            if (_ENpcResident.TryGetValue(npcId, out var value))
            {
                return value;
            }

            var ptr = Core.Memory.CallInjectedWraper<IntPtr>(Offsets.GetENpcResident, npcId);

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