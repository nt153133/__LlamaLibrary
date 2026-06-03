namespace LlamaLibrary.Retainers
{
    /// <summary>
    /// Represents a single item within a retainer's inventory or market board listing.
    /// This is a simplified data container used for snapshots and state tracking.
    /// </summary>
    public class RetainerInventoryItem
    {
        /// <summary>
        /// Gets the unique identifier for the item, which includes high-quality (HQ) and collectable offsets.
        /// </summary>
        public uint TrueItemID;

        /// <summary>
        /// Gets the base item ID as defined in the game's data sheets, excluding HQ or collectable status.
        /// </summary>
        public uint RawItemID;

        /// <summary>
        /// Gets the number of items in this stack.
        /// </summary>
        public uint Count;

        /// <summary>
        /// Gets the zero-based index of the inventory slot occupied by this item.
        /// </summary>
        public int Slot;

        /// <summary>
        /// Gets a value indicating whether the item is of High Quality (HQ).
        /// Determined by comparing <see cref="TrueItemID"/> with <see cref="RawItemID"/>.
        /// </summary>
        public bool HQ => TrueItemID != RawItemID;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetainerInventoryItem"/> class.
        /// </summary>
        /// <param name="trueItemId">The true item ID (including HQ/collectable offsets).</param>
        /// <param name="rawItemId">The base item ID.</param>
        /// <param name="count">The stack size.</param>
        /// <param name="slot">The inventory slot index.</param>
        public RetainerInventoryItem(uint trueItemId, uint rawItemId, uint count, int slot)
        {
            TrueItemID = trueItemId;
            RawItemID = rawItemId;
            Count = count;
            Slot = slot;
        }

        /// <summary>
        /// Determines whether the specified <see cref="RetainerInventoryItem"/> is equal to the current instance.
        /// Equality is based on item ID, quantity, and slot index.
        /// </summary>
        /// <param name="other">The other item to compare.</param>
        /// <returns><see langword="true"/> if the items are considered equal; otherwise <see langword="false"/>.</returns>
        protected bool Equals(RetainerInventoryItem other)
        {
            return TrueItemID == other.TrueItemID && Count == other.Count && Slot == other.Slot;
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

            return Equals((RetainerInventoryItem)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)TrueItemID;
                hashCode = (hashCode * 397) ^ (int)Count;
                hashCode = (hashCode * 397) ^ Slot;
                return hashCode;
            }
        }
    }
}
