using System;
using Clio.Utilities;

namespace LlamaLibrary.Helpers.HousingTravel
{
    public class HousingAetheryte : IEquatable<HousingAetheryte>
    {
        public uint Key;
        public uint NpcId;
        public string Name;
        public Vector3 Location;
        public bool Subdivision;

        public HousingAetheryte()
        {
        }

        public HousingAetheryte(uint key, uint npcId, string name, Vector3 location, bool subdivision)
        {
            Key = key;
            NpcId = npcId;
            Name = name;
            Location = location;
            Subdivision = subdivision;
        }

        public override string ToString()
        {
            return $"Key: {Key}, NpcId: {NpcId}, Name: {Name}, Location: {Location}, Subdivision: {Subdivision}";
        }

        public string CreationString()
        {
            return $"new HousingAetheryte({Key}, {NpcId}, \"{Name}\", new Vector3({Location.X}f, {Location.Y}f, {Location.Z}f), {Subdivision.ToString().ToLower()}),";
        }

        public bool Equals(HousingAetheryte other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key == other.Key && NpcId == other.NpcId && Location.Equals(other.Location);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HousingAetheryte)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Key;
                hashCode = (hashCode * 397) ^ (int)NpcId;
                hashCode = (hashCode * 397) ^ Location.GetHashCode();
                return hashCode;
            }
        }
    }
}