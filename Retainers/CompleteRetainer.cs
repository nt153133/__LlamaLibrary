using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Managers;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Retainers
{
    /// <summary>
    /// Represents a comprehensive snapshot of a retainer's current state.
    /// This includes raw metadata from game memory, organized inventory lists, and calculated availability.
    /// </summary>
    public class CompleteRetainer
    {
        /// <summary>
        /// Gets the raw memory metadata for the retainer, including name, level, job, and gil.
        /// </summary>
        public RetainerInfo Info;

        /// <summary>
        /// Gets the number of items this retainer currently has listed for sale on the market board.
        /// </summary>
        public int MBCount => ItemsForSale.Count;

        /// <summary>
        /// Gets a list of items currently listed for sale by this retainer on the market board.
        /// </summary>
        public List<RetainerInventoryItem> ItemsForSale;

        /// <summary>
        /// Gets a list of items currently held in the retainer's personal inventory bags.
        /// </summary>
        public List<RetainerInventoryItem> Inventory;

        /// <summary>
        /// Gets the zero-based index of this retainer in the player's total list of retainers.
        /// </summary>
        public int Index;

        /// <summary>
        /// Gets the number of empty slots remaining in the retainer's standard inventory (out of 175).
        /// </summary>
        public int FreeSlots => 175 - Inventory.Count;

        /// <summary>
        /// Gets the number of empty market board listing slots remaining (out of 20).
        /// </summary>
        public int FreeSlotsMB => 20 - ItemsForSale.Count;

        /// <summary>
        /// Gets the localized <see cref="DateTime"/> indicating when the retainer's market board listing will time out.
        /// </summary>
        public DateTime MBUpdated => DateTimeOffset.FromUnixTimeSeconds(Info.MBTimeOutTimestamp).LocalDateTime;

        /// <summary>
        /// Gets the localized <see cref="DateTime"/> indicating when the retainer's current venture will be completed.
        /// </summary>
        public DateTime VentureEnd => DateTimeOffset.FromUnixTimeSeconds(Info.VentureEndTimestamp).LocalDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteRetainer"/> class by capturing the state from provided <see cref="BagSlot"/> lists.
        /// </summary>
        /// <param name="info">The raw retainer metadata.</param>
        /// <param name="index">The retainer's index in the list.</param>
        /// <param name="itemsForSale">The raw list of slots from the market board bag.</param>
        /// <param name="inventory">The raw list of slots from the retainer's inventory bags.</param>
        public CompleteRetainer(RetainerInfo info, int index, List<BagSlot> itemsForSale, List<BagSlot> inventory)
        {
            Info = info;
            ItemsForSale = itemsForSale.Select(x => new RetainerInventoryItem(x.TrueItemId, x.RawItemId, x.Count, x.Slot)).ToList();
            Inventory = inventory.Select(x => new RetainerInventoryItem(x.TrueItemId, x.RawItemId, x.Count, x.Slot)).ToList();
            Index = index;
        }
    }
}
