using System.Collections.Generic;
using System.Linq;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="Bag"/> and collections of bag identifiers.
    /// </summary>
    public static class BagExtensions
    {
        /// <summary>
        /// Retrieves the first available empty slot within the specified bag.
        /// </summary>
        /// <param name="bag">The bag to search.</param>
        /// <returns>The first empty <see cref="BagSlot"/>, or <see langword="null"/> if the bag is full.</returns>
        public static BagSlot? GetFirstFreeSlot(this Bag bag)
        {
            return bag.FreeSlots > 0 ? bag.First(i => !i.IsFilled) : null;
        }

        /// <summary>
        /// Searches through a sequence of bag IDs and returns the first empty slot found.
        /// </summary>
        /// <param name="bagIds">The sequence of inventory bag identifiers to search.</param>
        /// <returns>The first empty <see cref="BagSlot"/> found, or <see langword="null"/> if all bags are full.</returns>
        public static BagSlot? NextFreeBagSlot(this IEnumerable<InventoryBagId> bagIds)
        {
            return (from bagId in bagIds select InventoryManager.GetBagByInventoryBagId(bagId) into freeSlots where freeSlots.FreeSlots > 0 select freeSlots.First(i => !i.IsFilled)).FirstOrDefault();
        }

        /// <summary>
        /// Searches through a collection of bags and returns the first empty slot found.
        /// </summary>
        /// <param name="bags">The collection of bags to search.</param>
        /// <returns>The first empty <see cref="BagSlot"/> found, or <see langword="null"/> if all bags are full.</returns>
        public static BagSlot? NextFreeBagSlot(this IEnumerable<Bag> bags)
        {
            return (from bag in bags where bag.FreeSlots > 0 select bag.First(i => !i.IsFilled)).FirstOrDefault();
        }
    }
}