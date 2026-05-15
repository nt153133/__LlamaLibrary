using System;
using System.Linq;
using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers.NPC
{
    /// <summary>
    /// Represents an in-game NPC (Non-Player Character) with its game identifier, world location,
    /// and optional quest-based unlock requirement.
    /// Used by LlamaLibrary to describe vendors, quest givers, inn keepers, retainer vocates,
    /// and other interactable characters in FFXIV.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Npc : IEquatable<Npc>
    {
        /// <summary>
        /// Gets or sets the game-internal NPC identifier (ENpcResident ID).
        /// Values above 1,000,000 are standard NPCs; values above 2,000,000 are event objects.
        /// </summary>
        [JsonProperty]
        public uint NpcId { get; set; }

        /// <summary>
        /// Gets or sets the world location (zone + coordinates) where this NPC is found.
        /// </summary>
        [JsonProperty]
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the ID of the FFXIV quest that must be completed before this NPC becomes accessible.
        /// A value of <c>0</c> indicates no quest is required.
        /// </summary>
        [JsonProperty]
        public int QuestRequiredId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this NPC has a quest unlock requirement.
        /// </summary>
        /// <value><see langword="true"/> if <see cref="QuestRequiredId"/> is non-zero; otherwise <see langword="false"/>.</value>
        public bool IsQuestRequired => QuestRequiredId != 0;

        /// <summary>
        /// Gets a value indicating whether the unlock condition for this NPC has been satisfied.
        /// Returns <see langword="true"/> if no quest is required or the required quest is already completed.
        /// </summary>
        /// <value><see langword="true"/> if the NPC is accessible; otherwise <see langword="false"/>.</value>
        public bool IsQuestCompleted => !IsQuestRequired || ConditionParser.IsQuestCompleted(QuestRequiredId);

        /// <summary>
        /// Gets a value indicating whether the player can travel to this NPC's location by any means:
        /// direct aetheryte teleportation, housing travel, or already being in the same zone.
        /// </summary>
        /// <value><see langword="true"/> if the NPC is reachable; otherwise <see langword="false"/>.</value>
        public bool CanGetTo => Location.CanTeleportTo || Location.IsHousingLocation || WorldManager.ZoneId == Location.ZoneId;

        /// <summary>
        /// Gets the gil cost to teleport to this NPC's location. Delegates to <see cref="Location.TeleportCost"/>.
        /// Returns <see cref="int.MaxValue"/> if the location is unreachable, or <c>0</c> if already in the zone.
        /// </summary>
        /// <value>The teleport gil cost, or <see cref="int.MaxValue"/> if unreachable.</value>
        public int TeleportCost => Location.TeleportCost;

        /// <summary>
        /// Gets a value indicating whether this NPC is in the same aetheryte network region as the player's current zone.
        /// </summary>
        /// <value><see langword="true"/> if the NPC's aetheryte is accessible from the current zone; otherwise <see langword="false"/>.</value>
        public bool IsInCurrentArea => Location.IsInCurrentArea;

        /// <summary>
        /// Gets a value indicating whether the player is currently in the same zone as this NPC.
        /// </summary>
        /// <value><see langword="true"/> if <c>WorldManager.ZoneId</c> matches the NPC's zone; otherwise <see langword="false"/>.</value>
        public bool IsInCurrentZone => Location.IsInCurrentZone;

        /// <summary>
        /// Gets the localized display name of this NPC, resolved via <see cref="NpcHelper.GetNpcName"/>.
        /// Includes the NPC's title in parentheses if one exists (e.g. <c>"Baderon (Proprietor)"</c>).
        /// </summary>
        /// <value>The NPC's display name, or an empty string if it cannot be resolved.</value>
        public string Name => NpcHelper.GetNpcName(NpcId);

        /// <summary>
        /// Gets the live <see cref="ff14bot.Objects.GameObject"/> instance for this NPC from the current game world,
        /// selecting the nearest targetable object with a matching <see cref="NpcId"/>.
        /// Returns <see langword="null"/> if the NPC is not currently loaded in the scene.
        /// </summary>
        /// <value>The closest targetable game object with this NPC's ID, or <see langword="null"/>.</value>
        public GameObject? GameObject => GameObjectManager.GameObjects.Where(r => r.IsTargetable && r.NpcId == NpcId).OrderBy(r => r.Distance()).FirstOrDefault();

        /// <summary>
        /// Gets a value indicating whether this NPC is located in an FFXIV residential housing district.
        /// </summary>
        /// <value><see langword="true"/> if the NPC's zone is a housing district; otherwise <see langword="false"/>.</value>
        public bool IsHousingZoneNpc => Location.IsHousingLocation;

        /// <summary>
        /// Initializes a new <see cref="Npc"/> with the specified NPC ID and a pre-built location.
        /// </summary>
        /// <param name="npcId">The game-internal ENpcResident ID for this NPC.</param>
        /// <param name="location">The <see cref="Location"/> where this NPC is found.</param>
        public Npc(uint npcId, Location location)
        {
            NpcId = npcId;
            Location = location;
        }

        /// <summary>
        /// Initializes a new <see cref="Npc"/> with the specified NPC ID, zone, and coordinates.
        /// A <see cref="Location"/> is constructed internally from <paramref name="zoneId"/> and <paramref name="location"/>.
        /// </summary>
        /// <param name="npcId">The game-internal ENpcResident ID for this NPC.</param>
        /// <param name="zoneId">The territory/zone ID where this NPC resides.</param>
        /// <param name="location">The world-space 3D coordinates of the NPC within the zone.</param>
        public Npc(uint npcId, ushort zoneId, Vector3 location)
        {
            NpcId = npcId;
            Location = new Location(zoneId, location);
        }

        /// <summary>
        /// Initializes a new <see cref="Npc"/> with the specified NPC ID, zone, coordinates, and quest requirement.
        /// </summary>
        /// <param name="npcId">The game-internal ENpcResident ID for this NPC.</param>
        /// <param name="zoneId">The territory/zone ID where this NPC resides.</param>
        /// <param name="location">The world-space 3D coordinates of the NPC within the zone.</param>
        /// <param name="questRequiredId">The quest ID that must be completed to access this NPC, or <c>0</c> if none.</param>
        public Npc(uint npcId, ushort zoneId, Vector3 location, int questRequiredId)
        {
            NpcId = npcId;
            Location = new Location(zoneId, location);
            QuestRequiredId = questRequiredId;
        }

        /// <summary>
        /// Initializes a new <see cref="Npc"/> by snapshotting the NPC ID and current zone/position
        /// of a live <see cref="ff14bot.Objects.GameObject"/> in the game world.
        /// </summary>
        /// <param name="gameObject">The live in-game object to create the NPC record from.</param>
        public Npc(GameObject gameObject)
        {
            NpcId = gameObject.NpcId;
            Location = new Location(WorldManager.ZoneId, gameObject.Location);
        }

        /// <summary>
        /// Generates a C# constructor call string for this <see cref="Npc"/> that can be copy-pasted
        /// into source code.
        /// </summary>
        /// <returns>A string in the form <c>new Npc(id, zoneId, new Vector3(x, y, z), 0),</c>.</returns>
        public string ObjectCreationString()
        {
            return $"new Npc({NpcId}, {Location.ZoneId}, new Vector3({Location.Coordinates.X}f, {Location.Coordinates.Y}f, {Location.Coordinates.Z}f),0),";
        }

        /// <summary>
        /// Returns a human-readable representation of this NPC as <c>"{Name} - {ZoneName}"</c>.
        /// </summary>
        /// <returns>The NPC's display name combined with its zone name.</returns>
        public override string ToString()
        {
            return $"{Name} - {Location.ZoneName}";
        }

        /// <summary>
        /// Determines whether this NPC equals another <see cref="Npc"/> by comparing
        /// <see cref="NpcId"/> and <see cref="Location"/>.
        /// </summary>
        /// <param name="other">The other NPC to compare against.</param>
        /// <returns><see langword="true"/> if both NPC ID and location match; otherwise <see langword="false"/>.</returns>
        public bool Equals(Npc? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return NpcId == other.NpcId && Equals(Location, other.Location);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Npc)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)NpcId * 397) ^ (Location.GetHashCode());
            }
        }
    }
}