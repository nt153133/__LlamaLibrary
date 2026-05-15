using System;
using Clio.Utilities;

namespace LlamaLibrary.Helpers.HousingTravel
{
    /// <summary>
    /// Represents a housing-district aetheryte shard — an interactable NPC crystal that provides
    /// fast-travel between sections of an FFXIV residential district.
    /// </summary>
    /// <remarks>
    /// Each residential district (Mist, Lavender Beds, The Goblet, Shirogane, Empyreum) contains
    /// multiple aetheryte shards covering the main ward and its subdivision.
    /// </remarks>
    public class HousingAetheryte : IEquatable<HousingAetheryte>
    {
        /// <summary>The unique aetheryte key used by <c>AgentTelepotTown</c> to identify this shard.</summary>
        public uint Key;

        /// <summary>The in-game NPC object ID used by <c>GameObjectManager</c> to locate this aetheryte.</summary>
        public uint NpcId;

        /// <summary>The display name of this aetheryte shard as it appears in the teleport window.</summary>
        public string Name;

        /// <summary>The 3-D world-space position of this aetheryte shard.</summary>
        public Vector3 Location;

        /// <summary>
        /// Indicates whether this aetheryte belongs to the subdivision rather than the main ward.
        /// </summary>
        public bool Subdivision;

        /// <summary>Initialises an empty <see cref="HousingAetheryte"/>.</summary>
        public HousingAetheryte()
        {
        }

        /// <summary>
        /// Initialises a new <see cref="HousingAetheryte"/> with all identifying fields.
        /// </summary>
        /// <param name="key">The aetheryte key used by <c>AgentTelepotTown</c>.</param>
        /// <param name="npcId">The NPC object ID used by <c>GameObjectManager</c>.</param>
        /// <param name="name">The display name of the aetheryte shard.</param>
        /// <param name="location">The 3-D world-space position.</param>
        /// <param name="subdivision">
        /// <see langword="true"/> if this aetheryte is in the subdivision; otherwise <see langword="false"/>.
        /// </param>
        public HousingAetheryte(uint key, uint npcId, string name, Vector3 location, bool subdivision)
        {
            Key = key;
            NpcId = npcId;
            Name = name;
            Location = location;
            Subdivision = subdivision;
        }

        /// <summary>Returns a human-readable summary of this aetheryte.</summary>
        /// <returns>A string containing key, NPC ID, name, location, and subdivision flag.</returns>
        public override string ToString()
        {
            return $"Key: {Key}, NpcId: {NpcId}, Name: {Name}, Location: {Location}, Subdivision: {Subdivision}";
        }

        /// <summary>
        /// Generates a C# constructor call string that can be copy-pasted to recreate this instance.
        /// </summary>
        /// <returns>A C# source code snippet for constructing an equivalent <see cref="HousingAetheryte"/>.</returns>
        public string CreationString()
        {
            return $"new HousingAetheryte({Key}, {NpcId}, \"{Name}\", new Vector3({Location.X}f, {Location.Y}f, {Location.Z}f), {Subdivision.ToString().ToLower()}),";
        }

        /// <summary>
        /// Determines whether this <see cref="HousingAetheryte"/> is equal to another by key, NPC ID, and location.
        /// </summary>
        /// <param name="other">The other <see cref="HousingAetheryte"/> to compare against.</param>
        /// <returns>
        /// <see langword="true"/> if both aetherytes share the same <see cref="Key"/>,
        /// <see cref="NpcId"/>, and <see cref="Location"/>; otherwise <see langword="false"/>.
        /// </returns>
        public bool Equals(HousingAetheryte? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Key == other.Key && NpcId == other.NpcId && Location.Equals(other.Location);
        }

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