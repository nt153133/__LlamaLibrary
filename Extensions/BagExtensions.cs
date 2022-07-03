using System.Collections.Generic;
using System.Linq;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Extensions
{
    public static class BagExtensions
    {
        public static BagSlot GetFirstFreeSlot(this Bag bag)
        {
            return bag.FreeSlots > 0 ? bag.First(i => !i.IsFilled) : null;
        }

        public static BagSlot NextFreeBagSlot(this IEnumerable<InventoryBagId> bagIds)
        {
            return (from bagId in bagIds select InventoryManager.GetBagByInventoryBagId(bagId) into freeSlots where freeSlots.FreeSlots > 0 select freeSlots.First(i => !i.IsFilled)).FirstOrDefault();
        }

        public static BagSlot NextFreeBagSlot(this IEnumerable<Bag> bags)
        {
            return (from bag in bags where bag.FreeSlots > 0 select bag.First(i => !i.IsFilled)).FirstOrDefault();
        }
    }
}