using System.Collections.Generic;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interface for the FFXIV Collectables Shop window.
    /// Handles job selection, item selection, and item turn-ins for scrips.
    /// </summary>
    //TODO Move element numbers to dictionary
    public class CollectablesShop : RemoteWindow<CollectablesShop>
    {
        /// <summary>
        /// Gets the total number of items eligible for turn-in in the current list.
        /// Resolved via element index 20.
        /// </summary>
        public int RowCount => Elements[20].Int - 1;

        /// <summary>
        /// Gets the count of items currently selected or ready for trade.
        /// Resolved via element index 4843.
        /// </summary>
        public int TurninCount => Elements[4843].Int;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectablesShop"/> class.
        /// </summary>
        public CollectablesShop() : base("CollectablesShop")
        {
        }

        /// <summary>
        /// Selects a job category in the shop window.
        /// </summary>
        /// <param name="job">The zero-based index of the job to select.</param>
        public void SelectJob(int job)
        {
            SendAction(2, 3, 0xE, 4, (ulong)job);
        }

        /// <summary>
        /// Selects a specific item from the list to be traded.
        /// </summary>
        /// <param name="line">The zero-based index of the item line in the current list.</param>
        public void SelectItem(int line)
        {
            SendAction(2, 3, 0xC, 4, (ulong)line);
        }

        /// <summary>
        /// Executes the trade action for the currently selected item(s).
        /// </summary>
        public void Trade()
        {
            SendAction(2, 3, 0xf, 4, 0);
        }

        /// <summary>
        /// Lists all items currently visible in the shop window with their formatted display strings.
        /// Skips items with invalid IDs or IDs outside the 500,000–1,500,000 range.
        /// </summary>
        /// <returns>A list of strings in the format "Index: ItemName RawID".</returns>
        public List<string> ListItems()
        {
            var count = Elements[20].Int - 1;
            var currentElements = Elements;
            var result = new List<string>();
            for (var j = 0; j < count; j++)
            {
                if (currentElements[32 + (j * 11)].Int == Elements[21].Int)
                {
                    continue; //IconID
                }

                var itemID = currentElements[34 + (j * 11)].Int;
                if (itemID is 0 or > 1500000 or < 500000)
                {
                    continue;
                }

                result.Add($"{j}: {DataManager.GetItem((uint)(itemID - 500000))} {itemID}");

                //Logger.Info($"{itemID}");
            }

            return result;
        }

        /// <summary>
        /// Retrieves a list of eligible items from the shop window, mapping them to their trade line indices.
        /// Subtracts 500,000 from the raw game ID to resolve the actual <see cref="ff14bot.Managers.Item"/> ID.
        /// </summary>
        /// <returns>A list of tuples containing the Item ID and its corresponding line index.</returns>
        public List<(uint ItemId, int Line)> GetItems()
        {
            var count = Elements[20].Int - 1;
            var currentElements = Elements;
            var result = new List<(uint ItemId, int Line)>();
            var index = 0;
            for (var j = 0; j < count; j++)
            {
                if (currentElements[34 + (j * 11)].Type == 0)
                {
                    continue;
                }

                if (currentElements[32 + (j * 11)].Int == Elements[21].Int)
                {
                    continue; //IconID
                }

                var itemID = currentElements[34 + (j * 11)].TrimmedData;
                if (itemID is 0 or > 1500000 or < 500000)
                {
                    continue;
                }

                result.Add(((uint)(itemID - 500000), index));
                index++;
            }

            return result;
        }
    }
}