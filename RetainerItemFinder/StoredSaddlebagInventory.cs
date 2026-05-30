using System.Collections.Generic;

namespace LlamaLibrary.RetainerItemFinder
{
    /// <summary>
    /// Represents the player's Chocobo Saddlebag inventory data retrieved from game memory.
    /// Implements <see cref="IStoredInventory"/> to provide structured access to item counts and slot information.
    /// </summary>
    public class StoredSaddleBagInventory : IStoredInventory
    {
        /// <inheritdoc/>
        public Dictionary<uint, int> Inventory { get; } = new Dictionary<uint, int>();

        /// <inheritdoc/>
        public Dictionary<uint, int> SlotCount { get; } = new Dictionary<uint, int>();

        /// <inheritdoc/>
        public int FreeSlots { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredSaddleBagInventory"/> class using provided item IDs and quantities.
        /// Processes up to 70 slots of inventory data.
        /// </summary>
        /// <param name="itemIds">An array of 32-bit unsigned integers representing item IDs.</param>
        /// <param name="itemQuantities">An array of 16-bit unsigned integers representing item quantities.</param>
        public StoredSaddleBagInventory(uint[] itemIds, ushort[] itemQuantities)
        {
            // Assumes a non-expanded saddlebag (70 slots)
            for (var i = 0; i < 70; i++)
            {
                if (itemIds[i] == 0)
                {
                    FreeSlots++;
                    continue;
                }

                if (Inventory.ContainsKey(itemIds[i]))
                {
                    Inventory[itemIds[i]] += itemQuantities[i];
                    SlotCount[itemIds[i]]++;
                }
                else
                {
                    Inventory.Add(itemIds[i], itemQuantities[i]);
                    SlotCount.Add(itemIds[i], 1);
                }
            }
        }
    }
}
