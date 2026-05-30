using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;

namespace LlamaLibrary.RetainerItemFinder
{
    /// <summary>
    /// Represents a retainer's inventory data retrieved from game memory.
    /// Implements <see cref="IStoredInventory"/> to provide structured access to item counts and slot information.
    /// </summary>
    public class StoredRetainerInventory : IStoredInventory
    {
        /// <inheritdoc/>
        public Dictionary<uint, int> Inventory { get; } = new Dictionary<uint, int>();

        /// <inheritdoc/>
        public Dictionary<uint, int> SlotCount { get; } = new Dictionary<uint, int>();

        /// <inheritdoc/>
        public int FreeSlots { get; }

        /// <summary>
        /// Gets a list of item IDs currently equipped by the retainer.
        /// </summary>
        public List<uint> EquippedItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredRetainerInventory"/> class by reading data from the specified memory pointer.
        /// Parses equipped items (14 slots), regular inventory (175 slots), and crystal inventory (18 slots).
        /// </summary>
        /// <param name="pointer">The memory address where the retainer's inventory data starts.</param>
        public StoredRetainerInventory(IntPtr pointer)
        {
            var position = pointer;

            // Read equipped items (14 slots)
            EquippedItems = new List<uint>(Core.Memory.ReadArray<uint>(position, 14).Where(i => i != 0));
            position += 14 * sizeof(uint);

            // Read regular item IDs (175 slots)
            var itemIds = Core.Memory.ReadArray<uint>(position, 175);
            position += 175 * sizeof(uint);

            // Read item quantities (175 slots)
            var qtys = Core.Memory.ReadArray<ushort>(position, 175);
            position += 175 * sizeof(ushort);

            // Read crystal quantities (18 slots)
            var crystalQtys = Core.Memory.ReadArray<ushort>(position, 18);

            // Process crystals
            for (var i = 0; i < 18; i++)
            {
                if (crystalQtys[i] == 0)
                {
                    continue;
                }

                // Crystal item IDs start at 2 and go up to 19 (Fire Shard to Earth Cluster)
                Inventory.Add((uint)(i + 2), crystalQtys[i]);
                SlotCount.Add((uint)(i + 2), 1);
            }

            // Process regular items
            for (var i = 0; i < 175; i++)
            {
                if (itemIds[i] == 0)
                {
                    FreeSlots++;
                    continue;
                }

                if (Inventory.ContainsKey(itemIds[i]))
                {
                    Inventory[itemIds[i]] += qtys[i];
                    SlotCount[itemIds[i]]++;
                }
                else
                {
                    Inventory.Add(itemIds[i], qtys[i]);
                    SlotCount.Add(itemIds[i], 1);
                }
            }
        }
    }
}
