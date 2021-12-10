using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using NavigationTest.InclusionShop;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentInclusionShop : AgentInterface<AgentInclusionShop>, IAgent
    {
        internal static class Offsets
        {
            //0x
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? 48 8D 05 ? ? ? ? 48 89 41 ? E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //(__int64 AgentPointer, char Category)
            [Offset("Search 48 8B 41 ? 4C 8B D1 80 88 ? ? ? ? ?")]
            internal static IntPtr SetCategory;

            //0x18
            [Offset("Search 48 8B 49 ? 4C 89 74 24 ? 48 85 C9 Add 3 Read8")]
            internal static int FirstPointer;

            //0x10
            [Offset("Search 48 8B 49 ? 49 8B C0 48 8B DA Add 3 Read8")]
            internal static int SecondPointer;

            //0x20
            [Offset("Search 41 8B 46 ? 48 C1 EE ? Add 3 Read8")]
            internal static int ShopKey;

            //0x38
            [Offset("48 8B 4F ? 89 81 ? ? ? ? 48 8B 47 ? C6 80 ? ? ? ? ? Add 3 Read8")]
            internal static int PointerToStartOfShopThing;

            //0x1177
            [Offset("44 3A B0 ? ? ? ? 0F 82 ? ? ? ? 4C 8B 7C 24 ? Add 3 Read32")]
            internal static int NumberOfCategories;

            //0x1223
            [Offset("40 38 B1 ? ? ? ? 0F 86 ? ? ? ? 66 0F 1F 44 00 ? Add 3 Read32")]
            internal static int NumberOfSubCategories;

            //0x11A9
            [Offset("41 0F B6 81 ? ? ? ? 48 69 D1 ? ? ? ? 48 69 C8 ? ? ? ? 41 0F B6 81 ? ? ? ? 4E 8B AC 0A ? ? ? ? Add 4 Read32")]
            internal static int SubCategory;

            //0x11A8
            [Offset("41 0F B6 80 ? ? ? ? 42 0F B6 94 00 ? ? ? ? Add 4 Read32")]
            internal static int Category;

            //0x1158
            [Offset("42 0F B6 8C 08 ? ? ? ? 41 0F B6 81 ? ? ? ? 48 69 D1 ? ? ? ? 48 69 C8 ? ? ? ? 41 0F B6 81 ? ? ? ? 4E 8B AC 0A ? ? ? ? Add 5 Read32")]
            internal static int CategoryArray;

            //0x1E0
            [Offset("4E 8B AC 0A ? ? ? ? Add 4 Read32")]
            internal static int SubCategoryArrayStart;

            //0x88
            [Offset("48 69 D1 ? ? ? ? 48 69 C8 ? ? ? ? 41 0F B6 81 ? ? ? ? 4E 8B AC 0A ? ? ? ? Add 3 Read8")]
            internal static int StructSizeCategory;

            //
            [Offset("0F B6 98 ? ? ? ? E8 ? ? ? ? 80 7C 24 ? ? Add 3 Read32")]
            internal static int ItemCount;

            //0x19d0
            [Offset("48 69 C8 ? ? ? ? 41 0F B6 81 ? ? ? ? 4E 8B AC 0A ? ? ? ? Add 3 Read32")]
            internal static int StructSizeSubCategory;

            //0x6C
            [Offset("48 6B C8 ? 48 8B 47 ? 4C 03 E9 Add 3 Read8")]
            internal static int StructSizeItem;

            //0x175
            [Offset("41 3A 81 ? ? ? ? 72 ? Add 3 Read32")]
            internal static int CategorySubCount;

            //0x19C9
            [Offset("F6 84 0A ? ? ? ? ? 75 ? 49 8B 52 ? Add 3 Read32")]
            internal static int SubCategoryEnabled;
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        public uint ShopKey => Core.Memory.Read<uint>(
            Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + Offsets.FirstPointer)
                                     + Offsets.SecondPointer) + Offsets.ShopKey);

        public IntPtr StartOfShopThing => Core.Memory.Read<IntPtr>(Pointer + Offsets.PointerToStartOfShopThing);

        public byte NumberOfSubCategories => Core.Memory.Read<byte>(StartOfShopThing + Offsets.NumberOfSubCategories);

        public byte SelectedCategory => Core.Memory.Read<byte>(StartOfShopThing + Offsets.Category);
        public byte SelectedSubCategory => Core.Memory.Read<byte>(StartOfShopThing + Offsets.SubCategory);

        public byte ItemCount => Core.Memory.Read<byte>(StartOfShopThing + Offsets.ItemCount);

        public byte CategoryCount => Core.Memory.Read<byte>(StartOfShopThing + Offsets.NumberOfCategories);

        public byte[] CategoryArray => Core.Memory.ReadArray<byte>(StartOfShopThing + Offsets.CategoryArray, CategoryCount);

        public int TrueCategory => Core.Memory.Read<byte>(StartOfShopThing + Offsets.CategoryArray + SelectedCategory);

        public IntPtr CategoryPtr => Core.Memory.Read<IntPtr>(StartOfShopThing +
                                                              (TrueCategory * Offsets.StructSizeCategory) +
                                                              Offsets.SubCategoryArrayStart);

        public IntPtr CategoryPtrPtr => StartOfShopThing + (TrueCategory * Offsets.StructSizeCategory);

        public IntPtr CategoryPtrPtrByIndex(int index) => StartOfShopThing +
                                                           (index * Offsets.StructSizeCategory);

        public IntPtr CategoryPtrByIndex(int index) => Core.Memory.Read<IntPtr>(StartOfShopThing +
                                                                              (index * Offsets.StructSizeCategory) +
                                                                              Offsets.SubCategoryArrayStart);
        public IntPtr SubCategoryPtr => CategoryPtr + (Offsets.StructSizeSubCategory * SelectedSubCategory) + Offsets.StructSizeCategory;

        public IntPtr SubCategoryPtrByIndex(IntPtr categoryPtr, int sub) => categoryPtr + (Offsets.StructSizeSubCategory * sub);

        public uint SubCategoryCost => Core.Memory.Read<uint>(CategoryPtr + (Offsets.StructSizeSubCategory * SelectedSubCategory) + Offsets.StructSizeItem);

        public List<InclusionShopItem> ShopItems
        {
            get
            {
                var shopItems = new List<InclusionShopItem>();
                foreach (var cat in Instance.CategoryArray)
                {
                    var category = Instance.CategoryPtrByIndex(cat);
                    var categoryPtr = Instance.CategoryPtrPtrByIndex(cat);
                    var subCount = Core.Memory.Read<byte>(categoryPtr + Offsets.CategorySubCount);

                    for (byte i = 0; i < subCount; i++)
                    {
                        var subCategory = Instance.SubCategoryPtrByIndex(category, i);
                        var enabledByte =
                            Core.Memory.Read<byte>(subCategory + Offsets.SubCategoryEnabled);

                        int itemNum = 0;
                        InclusionShopItemStruct shopItemStruct;
                        do
                        {
                            var pointer = subCategory + Offsets.StructSizeCategory + (Offsets.StructSizeItem * itemNum);
                            shopItemStruct = Core.Memory.Read<InclusionShopItemStruct>(pointer);
                            if (shopItemStruct.ItemId == 0)
                            {
                                break;
                            }

                            shopItems.Add(new InclusionShopItem(shopItemStruct, cat, i, itemNum, (enabledByte & 1) == 1));
                            itemNum += shopItemStruct.NumberOfItems;
                        }
                        while (shopItemStruct.ItemId > 0);
                    }
                }

                return shopItems;
            }
        }

        protected AgentInclusionShop(IntPtr pointer) : base(pointer)
        {
        }
    }
}