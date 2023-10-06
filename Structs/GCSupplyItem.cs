using System;
using System.Runtime.InteropServices;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0xA0)]
    public struct GCSupplyItem
    {
        [FieldOffset(0)]
        public IntPtr ItemPtr;

        [FieldOffset(0x68)]
        public uint BagId;

        [FieldOffset(0x78)]
        public uint Seals;

        [FieldOffset(0x84)]
        public uint ItemId;

        [FieldOffset(0x94)]
        public short ItemLevel;

        [FieldOffset(0x97)]
        public ushort BagSlotId;

        [FieldOffset(0x99)]
        public byte HandInType;

        [FieldOffset(0x9C)]
        public bool InGearSet;

        public bool InArmory => BagId == 3500 || (BagId - 3200) <= 9 || BagId == 3300 || BagId == 3400;

        public BagSlot BagSlot => InventoryManager.GetBagByInventoryBagId((InventoryBagId)BagId)[BagSlotId];

        public bool IsHQ => BagSlot.IsHighQuality;

        public override string ToString()
        {
            return $"Item: {DataManager.GetItem(ItemId).CurrentLocaleName}, {nameof(InGearSet)}: {InGearSet}, {nameof(InArmory)}: {InArmory} HQ: {IsHQ}";
        }
    }
}