using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentInclusionShop : AgentInterface<AgentInclusionShop>, IAgent
    {
        

        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentInclusionShop;

        public uint ShopKey => Core.Memory.Read<uint>(Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + AgentInclusionShopOffsets.FirstPointer) + AgentInclusionShopOffsets.SecondPointer) + AgentInclusionShopOffsets.ShopKey);

        public IntPtr StartOfShopThing => Core.Memory.Read<IntPtr>(Pointer + AgentInclusionShopOffsets.PointerToStartOfShopThing);

        public byte NumberOfSubCategories => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.NumberOfSubCategories);

        public byte SelectedCategory => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.Category);
        public byte SelectedSubCategory => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.SubCategory);

        public byte ItemCount => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.ItemCount);

        public byte CategoryCount => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.NumberOfCategories);

        public byte[] CategoryArray => Core.Memory.ReadArray<byte>(StartOfShopThing + AgentInclusionShopOffsets.CategoryArray, CategoryCount);

        public int TrueCategory => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.CategoryArray + SelectedCategory);

        public IntPtr CategoryPtr => Core.Memory.Read<IntPtr>(StartOfShopThing + (TrueCategory * AgentInclusionShopOffsets.StructSizeCategory) + AgentInclusionShopOffsets.SubCategoryArrayStart);

        public IntPtr CategoryPtrPtr => StartOfShopThing + (TrueCategory * AgentInclusionShopOffsets.StructSizeCategory);

        public IntPtr CategoryPtrPtrByIndex(int index)
        {
            return StartOfShopThing + (index * AgentInclusionShopOffsets.StructSizeCategory);
        }

        public IntPtr CategoryPtrByIndex(int index)
        {
            return Core.Memory.Read<IntPtr>(StartOfShopThing + (index * AgentInclusionShopOffsets.StructSizeCategory) + AgentInclusionShopOffsets.SubCategoryArrayStart);
        }

        public IntPtr SubCategoryPtr => CategoryPtr + (AgentInclusionShopOffsets.StructSizeSubCategory * SelectedSubCategory) + AgentInclusionShopOffsets.StructSizeCategory;

        public IntPtr SubCategoryPtrByIndex(IntPtr categoryPtr, int sub)
        {
            return categoryPtr + (AgentInclusionShopOffsets.StructSizeSubCategory * sub);
        }

        public uint SubCategoryCost => Core.Memory.Read<uint>(CategoryPtr + (AgentInclusionShopOffsets.StructSizeSubCategory * SelectedSubCategory) + AgentInclusionShopOffsets.StructSizeItem);

        public List<InclusionShopItem> ShopItems
        {
            get
            {
                var shopItems = new List<InclusionShopItem>();
                foreach (var cat in Instance.CategoryArray)
                {
                    var category = Instance.CategoryPtrByIndex(cat);
                    var categoryPtr = Instance.CategoryPtrPtrByIndex(cat);
                    var subCount = Core.Memory.Read<byte>(categoryPtr + AgentInclusionShopOffsets.CategorySubCount);

                    for (byte i = 0; i < subCount; i++)
                    {
                        var subCategory = Instance.SubCategoryPtrByIndex(category, i);
                        //var enabledByte = Core.Memory.Read<byte>(subCategory + AgentInclusionShopOffsets.SubCategoryEnabled);
                        var itemNum = 0;
                        InclusionShopItemStruct shopItemStruct;
                        do
                        {
                            var pointer = subCategory + AgentInclusionShopOffsets.StructSizeCategory + ((AgentInclusionShopOffsets.StructSizeItem + AgentInclusionShopOffsets.ItemStructAdjustment) * itemNum);
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