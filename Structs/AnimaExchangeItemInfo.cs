using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Sequential, Size = 0x78)]
    public struct AnimaExchangeItemInfo
    {
        public IntPtr ItemPtr;
        public uint ResultingItemId;
        public uint PatternGroup;
        public uint ExdKey;
        public uint ResultingItemQuantity;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * 8)]
        public uint[] ItemInfo;

        public (uint ItemId, int QtyRequired, int QtyHave)[] Items
        {
            get
            {
                var items = new (uint, int, int)[8];
                for (var i = 0; i < 8; i++)
                {
                    items[i] = (ItemInfo[i * 3], (int)ItemInfo[(i * 3) + 1], (int)ItemInfo[(i * 3) + 2]);
                }

                return items;
            }
        }

        public List<AnimaExchangeRequiredItem> RequiredItems => (from item in Items where item.ItemId != 0 select new AnimaExchangeRequiredItem(item.ItemId, item.QtyRequired, item.QtyHave)).ToList();

        public string Name => ItemPtr != IntPtr.Zero ? Core.Memory.ReadStringUTF8(ItemPtr) : string.Empty;

        public Item ResultingItem => DataManager.GetItem(ResultingItemId);

        public int Index => (int)(ExdKey - 1);
    }

    public struct AnimaExchangeRequiredItem
    {
        public uint ItemId;
        public int Quantity;
        public int QuantityHave;

        public AnimaExchangeRequiredItem(uint itemId, int qtyRequired, int qtyHave)
        {
            ItemId = itemId;
            Quantity = qtyRequired;
            QuantityHave = qtyHave;
        }

        public bool HasEnough => QuantityHave >= Quantity;
        public int CanAffordAmount => QuantityHave / Quantity;
        public Item Item => DataManager.GetItem(ItemId);

        public override string ToString()
        {
            return $"{Item.CurrentLocaleName} ({QuantityHave}/{Quantity})";
        }
    }
}