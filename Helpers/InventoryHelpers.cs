using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides extension methods and helpers for FFXIV inventory management,
    /// including FC chest transfer state detection, stack combining, and food/medicine filtering.
    /// </summary>
    public static class InventoryHelpers
    {
        private static readonly LLogger Log = new(nameof(InventoryHelpers), Colors.White);

        /// <summary>
        /// Gets a value indicating whether an FC chest item transfer is currently in progress.
        /// Read directly from <see cref="InventoryManager"/> memory without cache.
        /// </summary>
        public static bool IsFCItemBusy => Core.Memory.NoCacheRead<bool>(Offsets.g_InventoryManager + Offsets.InventoryManagerFCTransfering);

        /// <summary>
        /// Downgrades the first HQ stack of the given item to NQ, waits briefly, then moves all NQ stacks
        /// onto the first NQ slot to consolidate them.
        /// </summary>
        /// <param name="itemId">
        /// The raw (NQ) item ID whose HQ copies should be lowered to NQ and combined into one stack.
        /// </param>
        public static async Task LowerQualityAndCombine(int itemId)
        {
            var HQslots = InventoryManager.FilledSlots.Where(slot => slot.RawItemId == itemId && slot.IsHighQuality).ToList();

            if (HQslots.Count != 0)
            {
                HQslots.First().LowerQuality();
                await Coroutine.Sleep(1000);
            }

            var NQslots = InventoryManager.FilledSlots.Where(slot => slot.RawItemId == itemId && !slot.IsHighQuality).ToList();

            if (NQslots.Count > 1)
            {
                var firstSlot = NQslots.First();
                foreach (var slot in NQslots.Skip(1))
                {
                    slot.Move(firstSlot);
                    await Coroutine.Sleep(500);
                }
            }
        }

        private static bool IsFoodItem(this BagSlot slot) => slot.Item.EquipmentCatagory == ItemUiCategory.Meal;

        private static bool IsMedicineItem(this BagSlot slot) => slot.Item.EquipmentCatagory == ItemUiCategory.Medicine;

        /// <summary>
        /// Returns all filled bag slots from <paramref name="bags"/> whose item is in the <see cref="ItemUiCategory.Meal"/> category.
        /// </summary>
        /// <param name="bags">Source bag slots to filter.</param>
        /// <returns>Bag slots containing food items.</returns>
        public static IEnumerable<BagSlot> GetFoodItems(this IEnumerable<BagSlot> bags) =>
            bags.Where(s => s.IsFoodItem());

        /// <summary>
        /// Determines whether any slot in <paramref name="bags"/> contains the food item with the specified item ID.
        /// </summary>
        /// <param name="bags">Source bag slots to search.</param>
        /// <param name="id">Raw item ID to look for.</param>
        /// <returns><see langword="true"/> if at least one matching slot exists.</returns>
        public static bool ContainsFooditem(this IEnumerable<BagSlot> bags, uint id) =>
            bags.Select(s => s.RawItemId).Contains(id);

        /// <summary>
        /// Returns the first bag slot in <paramref name="bags"/> that contains the food item with the specified item ID.
        /// </summary>
        /// <param name="bags">Source bag slots to search.</param>
        /// <param name="id">Raw item ID to find.</param>
        /// <returns>The matching <see cref="BagSlot"/>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if no matching slot is found.</exception>
        public static BagSlot GetFoodItem(this IEnumerable<BagSlot> bags, uint id) =>
            bags.First(s => s.RawItemId == id);

        /// <summary>
        /// Returns all filled bag slots from <paramref name="bags"/> whose item is in the <see cref="ItemUiCategory.Medicine"/> category.
        /// </summary>
        /// <param name="bags">Source bag slots to filter.</param>
        /// <returns>Bag slots containing medicine items.</returns>
        public static IEnumerable<BagSlot> GetMedicineItems(this IEnumerable<BagSlot> bags) =>
            bags.Where(s => s.IsMedicineItem());

        /// <summary>
        /// Determines whether any slot in <paramref name="bags"/> contains the medicine item with the specified item ID.
        /// </summary>
        /// <param name="bags">Source bag slots to search.</param>
        /// <param name="id">Raw item ID to look for.</param>
        /// <returns><see langword="true"/> if at least one matching slot exists.</returns>
        public static bool ContainsMedicineitem(this IEnumerable<BagSlot> bags, uint id) =>
            bags.Select(s => s.RawItemId).Contains(id);

        /// <summary>
        /// Returns the first bag slot in <paramref name="bags"/> that contains the medicine item with the specified item ID.
        /// </summary>
        /// <param name="bags">Source bag slots to search.</param>
        /// <param name="id">Raw item ID to find.</param>
        /// <returns>The matching <see cref="BagSlot"/>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if no matching slot is found.</exception>
        public static BagSlot GetMedicineItem(this IEnumerable<BagSlot> bags, uint id) =>
            bags.First(s => s.RawItemId == id);

        /// <summary>
        /// Merges partial stacks of the same item ID (grouped by <see cref="BagSlot.TrueItemId"/>) within
        /// <paramref name="bagSlotsEnumerable"/> by moving items onto the fullest eligible slot.
        /// Collectable items (TrueItemId 500 000–999 999) are skipped.
        /// </summary>
        /// <param name="bagSlotsEnumerable">The bag slots to consolidate.</param>
        public static async Task CombineStacks(IEnumerable<BagSlot> bagSlotsEnumerable)
        {
            var bagSlots = bagSlotsEnumerable.ToArray();
            if (!bagSlots.Any())
            {
                return;
            }

            var groupedSlots = bagSlots
                .Where(x => x.IsValid && x.IsFilled && (x.Item?.StackSize ?? 0) > 1)
                .GroupBy(x => x.TrueItemId)
                .Where(x => x.Count(slot => slot.Count < slot.Item.StackSize) > 1);

            foreach (var slotGrouping in groupedSlots)
            {
                // Skip if item is a collectable.
                if (slotGrouping.Key is > 500_000 and < 1_000_000)
                {
                    continue;
                }

                var isHq = slotGrouping.Key > 1_000_000;
                var itemId = isHq ? slotGrouping.Key - 1_000_000 : slotGrouping.Key;
                var itemName = DataManager.GetItem(itemId)?.CurrentLocaleName ?? $"UNKNOWN(ID: {itemId})";
                Log.Information($"Combining stacks of {itemName}{(isHq ? " (HQ)" : string.Empty)}");

                var bagSlotArray = slotGrouping.OrderByDescending(x => x.Count).ToArray();
                var moveToIndex = Array.FindIndex(bagSlotArray, x => x.Count < x.Item.StackSize);
                if (moveToIndex < 0)
                {
                    continue;
                }

                for (var i = bagSlotArray.Length - 1; i > moveToIndex; i--)
                {
                    var moveFromSlot = bagSlotArray[i];
                    if (!moveFromSlot.IsValid || !moveFromSlot.IsFilled)
                    {
                        continue;
                    }

                    var curCount = bagSlotArray[moveToIndex].Count;
                    var result = moveFromSlot.Move(bagSlotArray[moveToIndex]);
                    if (result)
                    {
                        await Coroutine.Wait(3000, () => curCount != bagSlotArray[moveToIndex].Count);
                    }

                    await Coroutine.Yield();

                    var curMoveTo = bagSlotArray[moveToIndex];
                    if (curMoveTo.Count >= curMoveTo.Item.StackSize)
                    {
                        moveToIndex = Array.FindIndex(bagSlotArray, x => x.IsValid && x.IsFilled && x.Count < x.Item.StackSize);
                    }
                }
            }
        }
    }
}