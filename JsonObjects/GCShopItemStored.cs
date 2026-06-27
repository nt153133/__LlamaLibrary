using System;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.JsonObjects
{
    /// <summary>
    /// Represents an item available for purchase from a Grand Company shop, typically loaded from embedded resources.
    /// </summary>
    public class GCShopItemStored : IEquatable<GCShopItemStored>
    {
        /// <summary>Gets or sets the numeric identifier of the item.</summary>
        public uint ItemId { get; set; }

        /// <summary>Gets or sets the cost of the item in Grand Company seals.</summary>
        public int Cost { get; set; }

        /// <summary>Gets or sets the Grand Company rank required to purchase this item.</summary>
        public uint RequiredRank { get; set; }

        /// <summary>Gets or sets the rank group (tier) this item belongs to within the GC shop.</summary>
        public byte GCRankGroup { get; set; }

        /// <summary>Gets or sets the category under which this item is listed in the shop interface.</summary>
        public GCShopCategory Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GCShopItemStored"/> class for JSON deserialization.
        /// </summary>
        public GCShopItemStored()
        {
        }

        /// <inheritdoc/>
        public bool Equals(GCShopItemStored? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ItemId == other.ItemId && Cost == other.Cost && RequiredRank == other.RequiredRank && GCRankGroup == other.GCRankGroup && Category == other.Category;
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

            return Equals((GCShopItemStored)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ItemId;
                hashCode = (hashCode * 397) ^ Cost;
                hashCode = (hashCode * 397) ^ (int)RequiredRank;
                hashCode = (hashCode * 397) ^ GCRankGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Category;
                return hashCode;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GCShopItemStored"/> class with specific data.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="cost">The seal cost.</param>
        /// <param name="requiredRank">The required GC rank ID.</param>
        /// <param name="gcRankGroup">The rank group tier.</param>
        /// <param name="category">The shop category.</param>
        public GCShopItemStored(uint itemId, int cost, uint requiredRank, byte gcRankGroup, GCShopCategory category)
        {
            ItemId = itemId;
            Cost = cost;
            RequiredRank = requiredRank;
            GCRankGroup = gcRankGroup;
            Category = category;
        }
    }
}
