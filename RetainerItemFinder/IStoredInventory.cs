using System.Collections.Generic;

namespace LlamaLibrary.RetainerItemFinder
{
    /// <summary>
    /// Defines a contract for a stored inventory, providing access to item counts and slot information.
    /// </summary>
    public interface IStoredInventory
    {
        /// <summary>
        /// Gets a dictionary mapping item IDs to their total quantities in the inventory.
        /// </summary>
        Dictionary<uint, int> Inventory { get; }

        /// <summary>
        /// Gets a dictionary mapping item IDs to the number of inventory slots they occupy.
        /// </summary>
        Dictionary<uint, int> SlotCount { get; }

        /// <summary>
        /// Gets the number of free (empty) slots available in the inventory.
        /// </summary>
        int FreeSlots { get; }
    }
}
