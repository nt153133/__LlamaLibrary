using System;
using System.Linq;
using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers.NPC
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Npc : IEquatable<Npc>
    {
        [JsonProperty]
        public uint NpcId { get; set; }

        [JsonProperty]
        public Location Location { get; set; }

        [JsonProperty]
        public int QuestRequiredId { get; set; } = 0;

        public bool IsQuestRequired => QuestRequiredId != 0;

        public bool IsQuestCompleted => !IsQuestRequired || ConditionParser.IsQuestCompleted(QuestRequiredId);
        public bool CanGetTo => Location.CanTeleportTo || Location.IsHousingLocation;
        public int TeleportCost => Location.TeleportCost;
        public bool IsInCurrentArea => WorldManager.AetheryteIdsForZone(WorldManager.ZoneId).Select(i => i.Item1).Contains(Location.ClosestAetherytePrimaryResult.Id);
        public bool IsInCurrentZone => WorldManager.ZoneId == Location.ZoneId;
        public string Name => NpcHelper.GetNpcName(NpcId);
        public GameObject GameObject => GameObjectManager.GameObjects.Where(r => r.IsTargetable && r.NpcId == NpcId).OrderBy(r => r.Distance()).FirstOrDefault();
        public bool IsHousingZoneNpc => Location.IsHousingLocation;

        public Npc(uint npcId, Location location)
        {
            NpcId = npcId;
            Location = location;
        }

        public Npc(uint npcId, ushort zoneId, Vector3 location)
        {
            NpcId = npcId;
            Location = new Location(zoneId, location);
        }

        public Npc(uint npcId, ushort zoneId, Vector3 location, int questRequiredId)
        {
            NpcId = npcId;
            Location = new Location(zoneId, location);
            QuestRequiredId = questRequiredId;
        }

        public Npc(GameObject gameObject)
        {
            NpcId = gameObject.NpcId;
            Location = new Location(WorldManager.ZoneId, gameObject.Location);
        }

        public string ObjectCreationString()
        {
            return $"new Npc({NpcId}, {Location.ZoneId}, new Vector3({Location.Coordinates.X}f, {Location.Coordinates.Y}f, {Location.Coordinates.Z}f),0),";
        }

        public override string ToString()
        {
            return $"{Name} - {Location.ZoneName}";
        }

        public bool Equals(Npc other)
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

        public override bool Equals(object obj)
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

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)NpcId * 397) ^ (Location != null ? Location.GetHashCode() : 0);
            }
        }
    }
}