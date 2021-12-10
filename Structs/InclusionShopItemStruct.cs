using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace NavigationTest.InclusionShop
{
    [StructLayout(LayoutKind.Explicit, Size = 0x6C)]
    public struct InclusionShopItemStruct
    {
        [FieldOffset(0x0)]
        public uint SetQty;

        [FieldOffset(0x8)]
        public uint Cost1;

        [FieldOffset(0xC)]
        public uint Cost2;

        [FieldOffset(0x10)]
        public uint Cost3;

        [FieldOffset(0x14)]
        public uint ItemId;

        [FieldOffset(0x18)]
        public uint ItemId2;

        [FieldOffset(0x24)]
        public uint CostType1;

        [FieldOffset(0x28)]
        public uint CostType2;

        [FieldOffset(0x2C)]
        public uint CostType3;

        [FieldOffset(0x44)]
        public CostFlag CostFlag1;

        [FieldOffset(0x45)]
        public CostFlag CostFlag2;

        [FieldOffset(0x46)]
        public CostFlag CostFlag3;

        [FieldOffset(0x48)]
        public bool ResultItemHQ;

        public Item Item => DataManager.GetItem(ItemId, ResultItemHQ);

        public Item Item2 => DataManager.GetItem(ItemId2, ResultItemHQ);

        public string Name => Item != null ? Item.CurrentLocaleName : "";

        public string Name2 => Item2 != null ? Item2.CurrentLocaleName : "";

        public int NumberOfItems => ItemId2 != 0 ? 2 : 1;

        public (uint Amount, uint Type, CostFlag Flag)[] Costs => new (uint Amount, uint Type, CostFlag Flag)[]
            {
                (Cost1, CostType1, CostFlag1), (Cost2, CostType2, CostFlag2), (Cost3, CostType3, CostFlag3)
            };

        public override string ToString()
        {
            return $"{SetQty} x {Name} Costs {Cost1}";
        }
    }

    public enum CostFlag : byte
    {
        Item = 0,
        HQ = 1,
        Tome = 2,
        SpecialCurrency = 3
    }
}