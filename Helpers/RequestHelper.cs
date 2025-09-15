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
    public static class RequestHelper
    {
        private static readonly LLogger Log = new(nameof(RequestHelper), Colors.MediumPurple);

        

        public static ushort ItemCount => Core.Memory.Read<ushort>(RequestHelperOffsets.RequestInfo + RequestHelperOffsets.ItemCount);
        public static ushort ItemCount2 => Core.Memory.Read<ushort>(RequestHelperOffsets.RequestInfo + RequestHelperOffsets.ItemCount2);

        public static IntPtr ItemListStart => new(RequestHelperOffsets.RequestInfo + RequestHelperOffsets.ItemListStart);

        public static RequestItem[] GetItems()
        {
            return Core.Memory.ReadArray<RequestItem>(ItemListStart, ItemCount);
        }

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