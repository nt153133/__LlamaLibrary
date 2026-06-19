using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Inclusion Shop interface.
    /// Manages complex currency-based exchange shops, including Scrip Exchange, Hunt Billmaster, and Splendors Vendors.
    /// </summary>
    public class AgentInclusionShop : AgentInterface<AgentInclusionShop>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentInclusionShopOffsets.Vtable;

        /// <summary>
        /// Gets the unique identifier for the current shop instance.
        /// </summary>
        public uint ShopKey => Core.Memory.Read<uint>(Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + AgentInclusionShopOffsets.FirstPointer) + AgentInclusionShopOffsets.SecondPointer) + AgentInclusionShopOffsets.ShopKey);

        /// <summary>
        /// Gets the pointer to the primary data structure containing shop state and categories.
        /// </summary>
        public IntPtr StartOfShopThing => Core.Memory.Read<IntPtr>(Pointer + AgentInclusionShopOffsets.PointerToStartOfShopThing);

        /// <summary>
        /// Gets the total number of sub-categories (tabs) available in the currently selected major category.
        /// </summary>
        public byte NumberOfSubCategories => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.NumberOfSubCategories);

        /// <summary>
        /// Gets the zero-based index of the currently selected major category.
        /// </summary>
        public byte SelectedCategory => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.Category);

        /// <summary>
        /// Gets the zero-based index of the currently selected sub-category.
        /// </summary>
        public byte SelectedSubCategory => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.SubCategory);

        /// <summary>
        /// Gets the total number of items in the currently displayed shop list.
        /// </summary>
        public byte ItemCount => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.ItemCount);

        /// <summary>
        /// Gets the total number of major categories available in the shop.
        /// </summary>
        public byte CategoryCount => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.NumberOfCategories);

        /// <summary>
        /// Gets an array of category identifiers from game memory.
        /// </summary>
        public byte[] CategoryArray => Core.Memory.ReadArray<byte>(StartOfShopThing + AgentInclusionShopOffsets.CategoryArray, CategoryCount);

        /// <summary>
        /// Gets the actual (true) category identifier based on the user's selection index.
        /// </summary>
        public int TrueCategory => Core.Memory.Read<byte>(StartOfShopThing + AgentInclusionShopOffsets.CategoryArray + SelectedCategory);

        /// <summary>
        /// Gets the pointer to the currently active category data.
        /// </summary>
        public IntPtr CategoryPtr => Core.Memory.Read<IntPtr>(StartOfShopThing + (TrueCategory * AgentInclusionShopOffsets.StructSizeCategory) + AgentInclusionShopOffsets.SubCategoryArrayStart);

        /// <summary>
        /// Gets the base pointer for the currently active category's structure.
        /// </summary>
        public IntPtr CategoryPtrPtr => StartOfShopThing + (TrueCategory * AgentInclusionShopOffsets.StructSizeCategory);

        /// <summary>
        /// Retrieves the base pointer for a category structure by its index.
        /// </summary>
        /// <param name="index">The category index.</param>
        /// <returns>The memory pointer to the category structure.</returns>
        public IntPtr CategoryPtrPtrByIndex(int index)
        {
            return StartOfShopThing + (index * AgentInclusionShopOffsets.StructSizeCategory);
        }

        /// <summary>
        /// Retrieves the category data pointer for a specific category index.
        /// </summary>
        /// <param name="index">The category index.</param>
        /// <returns>The memory pointer to the category data.</returns>
        public IntPtr CategoryPtrByIndex(int index)
        {
            return Core.Memory.Read<IntPtr>(StartOfShopThing + (index * AgentInclusionShopOffsets.StructSizeCategory) + AgentInclusionShopOffsets.SubCategoryArrayStart);
        }

        /// <summary>
        /// Gets the pointer to the currently active sub-category data.
        /// </summary>
        public IntPtr SubCategoryPtr => CategoryPtr + (AgentInclusionShopOffsets.StructSizeSubCategory * SelectedSubCategory) + AgentInclusionShopOffsets.StructSizeCategory;

        /// <summary>
        /// Retrieves the pointer for a specific sub-category within a major category.
        /// </summary>
        /// <param name="categoryPtr">The pointer to the major category.</param>
        /// <param name="sub">The sub-category index.</param>
        /// <returns>The memory pointer to the sub-category structure.</returns>
        public IntPtr SubCategoryPtrByIndex(IntPtr categoryPtr, int sub)
        {
            return categoryPtr + (AgentInclusionShopOffsets.StructSizeSubCategory * sub);
        }

        /// <summary>
        /// Gets the cost associated with the currently selected sub-category.
        /// </summary>
        public uint SubCategoryCost => Core.Memory.Read<uint>(CategoryPtr + (AgentInclusionShopOffsets.StructSizeSubCategory * SelectedSubCategory) + AgentInclusionShopOffsets.StructSizeItem);

        /// <summary>
        /// Gets a list of all <see cref="InclusionShopItem"/>s available across all categories and sub-categories in the shop.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentInclusionShop"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentInclusionShop(IntPtr pointer) : base(pointer)
        {
        }
    }
}