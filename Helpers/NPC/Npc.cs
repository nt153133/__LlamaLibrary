using System;
using System.Linq;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;

namespace LlamaLibrary.Helpers.NPC
{
    public class Npc : IEquatable<Npc>
    {
        public uint NpcId { get; set; }
        public Location Location { get; set; }
        public bool CanGetTo => Location.CanTeleportTo;
        public int TeleportCost => Location.TeleportCost;
        public bool IsInCurrentArea => WorldManager.AetheryteIdsForZone(WorldManager.ZoneId).Select(i => i.Item1).Contains(Location.ClosestAetherytePrimaryResult.Id);
        public bool IsInCurrentZone => WorldManager.ZoneId == Location.ZoneId;
        public string Name => NpcHelper.GetNpcName(NpcId);
        public GameObject GameObject => GameObjectManager.GameObjects.Where(r => r.IsTargetable && r.NpcId == NpcId).OrderBy(r => r.Distance()).FirstOrDefault();

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

        public Npc(GameObject gameObject)
        {
            NpcId = gameObject.NpcId;
            Location = new Location(WorldManager.ZoneId, gameObject.Location);
        }

        public string ObjectCreationString()
        {
            return $"new Npc({NpcId}, {Location.ZoneId}, new Vector3({Location.Coordinates.X}f, {Location.Coordinates.Y}f, {Location.Coordinates.Z}f)),";
        }

        public override string ToString()
        {
            return $"{Name} - {Location.ZoneName}";
        }

        public bool Equals(Npc other)
        {
            if (ReferenceEquals(null, other))
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Npc) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) NpcId * 397) ^ (Location != null ? Location.GetHashCode() : 0);
            }
        }
    }
}