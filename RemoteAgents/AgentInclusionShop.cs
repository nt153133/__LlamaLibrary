using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentInclusionShop : AgentInterface<AgentInclusionShop>, IAgent
    {
        internal static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? 48 8D 05 ? ? ? ? 48 89 41 ? E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B DA Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //(__int64 AgentPointer, char Category)
            [Offset("Search 48 8B 41 ? 4C 8B D1 80 88 ? ? ? ? ?")]
            internal static IntPtr SetCategory;

            //0x18
            [Offset("Search 48 8B 49 ? 4C 89 74 24 ? 48 85 C9 Add 3 Read8")]
            [OffsetDawntrail("Search 48 8B 49 ? 48 89 6C 24 ? 48 85 C9 Add 3 Read8")]
            internal static int FirstPointer;

            //0x10
            [Offset("Search 48 8B 49 ? 49 8B C0 48 8B DA Add 3 Read8")]
            [OffsetDawntrail("Search 48 8B 69 ? 49 8B F0 48 8B DA 48 8B F9 Add 3 Read8")]
            internal static int SecondPointer;

            //0x20
            //7.1
            [Offset("Search 41 8B 44 24 ? 3D ? ? ? ? 74 ? 3D ? ? ? ? 75 ? Add 4 Read8")]
            //[OffsetCN("Search 8B 45 ? 3D ? ? ? ? 74 ? 3D ? ? ? ? 75 ? B2 ? Add 2 Read8")]
            internal static int ShopKey;

            //0x38
            [Offset("Search 48 8B 4F ? 89 81 ? ? ? ? 48 8B 47 ? C6 80 ? ? ? ? ? Add 3 Read8")]
            internal static int PointerToStartOfShopThing;

            //0x1177
            [Offset("Search 44 3A B0 ? ? ? ? 0F 82 ? ? ? ? 4C 8B 7C 24 ? Add 3 Read32")]
            [OffsetDawntrail("Search 40 3A AB ? ? ? ? 0F 82 ? ? ? ? Add 3 Read32")]
            internal static int NumberOfCategories;

            //0x1223
            //7.1
            [Offset("Search 40 38 B9 ? ? ? ? 0F 86 ? ? ? ? 4C 8B 6C 24 ? Add 3 Read32")]
            //[OffsetCN("Search 40 38 B9 ? ? ? ? 0F 86 ? ? ? ? 45 8B FE Add 3 Read32")]
            internal static int NumberOfSubCategories;

            //0x11D1
            //6.5Done
            [Offset("Search 41 0F B6 80 ? ? ? ? 4C 6B E8 ? Add 4 Read32")]
            [OffsetDawntrail("Search 0F B6 82 ? ? ? ? 4C 6B C0 ? Add 3 Read32")]
            internal static int SubCategory;

            //0x11A8
            [Offset("Search 41 0F B6 80 ? ? ? ? 42 0F B6 94 00 ? ? ? ? Add 4 Read32")]
            internal static int Category;

            //0x1180
            //6.5Done
            [Offset("Search 42 0F B6 94 00 ? ? ? ? 48 69 C2 ? ? ? ? Add 5 Read32")]
            [OffsetDawntrail("Search 42 0F B6 94 00 ? ? ? ? 32 C0 Add 5 Read32")]
            internal static int CategoryArray;

            //0x208
            [Offset("Search 4E 03 AC 00 ? ? ? ? Add 4 Read32")]
            [OffsetDawntrail("Search 4C 03 84 10 ? ? ? ? Add 4 Read32")]
            internal static int SubCategoryArrayStart;

            //0x88
            //6.5
            [Offset("Search 48 69 C1 ? ? ? ? 4E 03 AC 00 ? ? ? ? Add 3 Read8")]
            [OffsetDawntrail("Search 48 69 C1 ? ? ? ? 4C 03 84 10 ? ? ? ? Add 3 Read32")]
            internal static int StructSizeCategory;


            [Offset("Search 0F B6 98 ? ? ? ? E8 ? ? ? ? 80 7C 24 ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 0F B6 98 ? ? ? ? E8 ? ? ? ? 4C 8B 7C 24 ? Add 3 Read32")]
            internal static int ItemCount;

            //0x19d0
            //6.5 Done
            [Offset("Search 48 69 D1 ? ? ? ? 49 8B 89 ? ? ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 48 69 D1 ? ? ? ? 4B 8B 8C 01 ? ? ? ? Add 3 Read32")]
            internal static int StructSizeSubCategory;

            //0x6C
            //6.5Done
            [Offset("Search 8B 4B ? 85 C9 74 ? E8 ? ? ? ? 48 85 C0 74 ? 8B 4B ? Add 2 Read8")]
            internal static int StructSizeItem;

            //0x175
            [Offset("Search 41 3A 81 ? ? ? ? 72 ? Add 3 Read32")]
            [OffsetDawntrail("Search 43 3A 84 01 ? ? ? ? Add 4 Read32")]
            internal static int CategorySubCount;

            //0x19C9
            [Offset("Search 80 BC 0A ? ? ? ? ? 74 ? 49 8B 52 ? Add 3 Read32")]
            internal static int SubCategoryEnabled;

            //7.1
            [Offset("Search 47 8B 64 B5 ? Add 4 Read8")]
            //[OffsetCN("Search 45 8B 64 B7 ? Add 4 Read8")]
            internal static int ItemStructAdjustment;
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        public uint ShopKey => Core.Memory.Read<uint>(Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + Offsets.FirstPointer) + Offsets.SecondPointer) + Offsets.ShopKey);

        public IntPtr StartOfShopThing => Core.Memory.Read<IntPtr>(Pointer + Offsets.PointerToStartOfShopThing);

        public byte NumberOfSubCategories => Core.Memory.Read<byte>(StartOfShopThing + Offsets.NumberOfSubCategories);

        public byte SelectedCategory => Core.Memory.Read<byte>(StartOfShopThing + Offsets.Category);
        public byte SelectedSubCategory => Core.Memory.Read<byte>(StartOfShopThing + Offsets.SubCategory);

        public byte ItemCount => Core.Memory.Read<byte>(StartOfShopThing + Offsets.ItemCount);

        public byte CategoryCount => Core.Memory.Read<byte>(StartOfShopThing + Offsets.NumberOfCategories);

        public byte[] CategoryArray => Core.Memory.ReadArray<byte>(StartOfShopThing + Offsets.CategoryArray, CategoryCount);

        public int TrueCategory => Core.Memory.Read<byte>(StartOfShopThing + Offsets.CategoryArray + SelectedCategory);

        public IntPtr CategoryPtr => Core.Memory.Read<IntPtr>(StartOfShopThing + (TrueCategory * Offsets.StructSizeCategory) + Offsets.SubCategoryArrayStart);

        public IntPtr CategoryPtrPtr => StartOfShopThing + (TrueCategory * Offsets.StructSizeCategory);

        public IntPtr CategoryPtrPtrByIndex(int index)
        {
            return StartOfShopThing + (index * Offsets.StructSizeCategory);
        }

        public IntPtr CategoryPtrByIndex(int index)
        {
            return Core.Memory.Read<IntPtr>(StartOfShopThing + (index * Offsets.StructSizeCategory) + Offsets.SubCategoryArrayStart);
        }

        public IntPtr SubCategoryPtr => CategoryPtr + (Offsets.StructSizeSubCategory * SelectedSubCategory) + Offsets.StructSizeCategory;

        public IntPtr SubCategoryPtrByIndex(IntPtr categoryPtr, int sub)
        {
            return categoryPtr + (Offsets.StructSizeSubCategory * sub);
        }

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
                        var enabledByte = Core.Memory.Read<byte>(subCategory + Offsets.SubCategoryEnabled);
                        var itemNum = 0;
                        InclusionShopItemStruct shopItemStruct;
                        do
                        {
                            var pointer = subCategory + Offsets.StructSizeCategory + ((Offsets.StructSizeItem + Offsets.ItemStructAdjustment) * itemNum);
                            shopItemStruct = Core.Memory.Read<InclusionShopItemStruct>(pointer);
                            if (shopItemStruct.ItemId == 0)
                            {
                                break;
                            }

                            //shopItems.Add(new InclusionShopItem(shopItemStruct, cat, i, itemNum, (enabledByte & 1) == 1));
                            shopItems.Add(new InclusionShopItem(shopItemStruct, cat, i, itemNum, false));
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