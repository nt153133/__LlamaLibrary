using System;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Reads the pending item turn-in request from game memory (the <c>Request</c> window).
    /// Provides helpers to check item availability and submit the handover.
    /// </summary>
    public static class RequestHelper
    {
        private static readonly LLogger Log = new(nameof(RequestHelper), Colors.MediumPurple);

        

        /// <summary>Gets the number of distinct item types requested in the currently open Request window.</summary>
        public static ushort ItemCount => Core.Memory.Read<ushort>(RequestHelperOffsets.RequestInfo + RequestHelperOffsets.ItemCount);

        /// <summary>Gets a secondary item count field (used by some request variants).</summary>
        public static ushort ItemCount2 => Core.Memory.Read<ushort>(RequestHelperOffsets.RequestInfo + RequestHelperOffsets.ItemCount2);

        /// <summary>Gets the base memory address of the request item list.</summary>
        public static IntPtr ItemListStart => new(RequestHelperOffsets.RequestInfo + RequestHelperOffsets.ItemListStart);

        /// <summary>
        /// Reads all requested items from the current Request window.
        /// </summary>
        /// <returns>An array of <see cref="RequestItem"/> entries describing each required item and quantity.</returns>
        public static RequestItem[] GetItems()
        {
            return Core.Memory.ReadArray<RequestItem>(ItemListStart, ItemCount);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the player has all required items (with correct HQ flag) to satisfy the open Request window.
        /// </summary>
        public static bool HaveTurninItems()
        {
            if (!Request.IsOpen)
            {
                return false;
            }

            var haveAll = true;
            foreach (var item in GetItems())
            {
                var items = InventoryManager.FilledSlots.Where(i => i.RawItemId == item.ItemId && i.Count >= item.Count);
                if (item.HQ)
                {
                    haveAll = haveAll && items.Any(i => i.IsHighQuality);
                }
                else
                {
                    haveAll = haveAll && items.Any();
                }
            }

            return haveAll;
        }

        /// <summary>
        /// Hands over all required items from the player's inventory to satisfy the open Request window.
        /// </summary>
        /// <returns><see langword="true"/> if all items were handed over and the Hand Over button is clickable.</returns>
        public static bool HandOver()
        {
            if (!Request.IsOpen || !HaveTurninItems())
            {
                return false;
            }

            foreach (var item in GetItems())
            {
                var items = InventoryManager.FilledSlots.Where(i => i.RawItemId == item.ItemId && i.Count >= item.Count);
                if (item.HQ)
                {
                    items.First(i => i.IsHighQuality).Handover();
                }
                else
                {
                    items.First().Handover();
                }
            }

            return Request.HandOverButtonClickable;
        }
    }
}