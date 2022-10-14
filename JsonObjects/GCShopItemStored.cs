using System;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.JsonObjects
{
    public class GCShopItemStored : IEquatable<GCShopItemStored>
    {
        public uint ItemId { get; set; }
        public int Cost { get; set; }
        public uint RequiredRank { get; set; }
        public byte GCRankGroup { get; set; }
        public GCShopCategory Category { get; set; }


        public GCShopItemStored()
        {
        }

        public bool Equals(GCShopItemStored other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ItemId == other.ItemId && Cost == other.Cost && RequiredRank == other.RequiredRank && GCRankGroup == other.GCRankGroup && Category == other.Category;
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

            return Equals((GCShopItemStored)obj);
        }

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