using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "GrandCompanyExchange" window.
    /// Manages the purchase of items using Grand Company Seals.
    /// </summary>
    public class GrandCompanyExchange : RemoteWindow<GrandCompanyExchange>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "GetNumberOfItems", 1 },
            { "GCRankGroup", 2 },
            { "TurninIdElementsStart", 317 },
            { "CostElementsStart", 67 },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="GrandCompanyExchange"/> class.
        /// </summary>
        public GrandCompanyExchange() : base("GrandCompanyExchange")
        {
        }

        /// <summary>
        /// Gets the total number of items currently listed in the active exchange category.
        /// </summary>
        public int GetNumberOfItems => IsOpen ? Elements[Properties["GetNumberOfItems"]].Int : 0;

        /// <summary>
        /// Gets the index of the currently selected rank group (e.g., Private, Corporal).
        /// </summary>
        public int GCRankGroup => IsOpen ? Elements[Properties["GCRankGroup"]].Int : 0;

        /// <summary>
        /// Retrieves the raw item IDs for all items listed in the current exchange category.
        /// </summary>
        /// <returns>An array of item identifiers.</returns>
        public uint[] GetTurninItemsIds()
        {
            var currentElements = Elements;
            var turninIdElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninIdElementsStart"], GetNumberOfItems).Select(i => i.UInt).ToArray();
            return turninIdElements;
        }

        /// <summary>
        /// Retrieves the seal costs for all items listed in the current exchange category.
        /// </summary>
        /// <returns>An array of seal costs corresponding to the items in the current list.</returns>
        public uint[] GetItemCosts()
        {
            var currentElements = Elements;
            var costElements = new ArraySegment<TwoInt>(currentElements, Properties["CostElementsStart"], GetNumberOfItems).Select(i => i.UInt).ToArray();
            return costElements;
        }

        /// <summary>
        /// Initiates the purchase of a specific item by its index in the current list.
        /// </summary>
        /// <param name="index">The zero-based index of the item to purchase.</param>
        /// <param name="qty">The quantity of the item to buy.</param>
        public void BuyItemByIndex(uint index, int qty)
        {
            SendAction(4, 3, 0, 3, index, 3, (ulong)qty, 0, 7); //Click item and qty
        }

        /// <summary>
        /// Switches to a different rank-based item category (e.g., Private, Corporal, Master).
        /// </summary>
        /// <param name="rankGroup">The zero-based index of the rank group to select.</param>
        public void ChangeRankGroup(int rankGroup)
        {
            SendAction(2, 3, 1, 3, (ulong)rankGroup);
        }

        /// <summary>
        /// Switches between the "Materials" and "Others" item tabs within a rank group.
        /// </summary>
        /// <param name="itemGroup">The zero-based index of the item group (tab) to select.</param>
        public void ChangeItemGroup(int itemGroup)
        {
            SendAction(2, 3, 2, 3, (ulong)itemGroup);
        }
    }
}