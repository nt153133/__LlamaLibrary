using LlamaLibrary.RemoteWindows.Atk;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "InclusionShop" window.
    /// Used for various currency-based shops like Scrip Exchange, Hunt Billmaster, and Splendors Vendor.
    /// </summary>
    public class InclusionShop : RemoteWindow<InclusionShop>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InclusionShop"/> class.
        /// </summary>
        public InclusionShop() : base("InclusionShop")
        {
        }

        /// <summary>
        /// Selects a major category (tab) in the shop.
        /// </summary>
        /// <param name="category">The zero-based index of the category to select.</param>
        public void SetCategory(int category)
        {
            //SendAction(2, 3, 0xC, 4, (ulong)category);
            SendAction(true, (ValueType.Int, 0xC), (ValueType.UInt, category));
        }

        /// <summary>
        /// Selects a sub-category or specific item group within the active category.
        /// </summary>
        /// <param name="subCategory">The zero-based index of the sub-category to select.</param>
        public void SetSubCategory(int subCategory)
        {
            //SendAction(2, 3, 0xD, 4, (ulong)subCategory);
            SendAction(true, (ValueType.Int, 0xD), (ValueType.UInt, subCategory));
        }

        /// <summary>
        /// Purchases a specific quantity of an item from the current shop list.
        /// </summary>
        /// <param name="index">The zero-based index of the item in the displayed list.</param>
        /// <param name="qty">The quantity of the item to purchase.</param>
        public void BuyItem(int index, int qty)
        {
            //SendAction(3, 3, 0xE, 4, (ulong)index, 4, (ulong)qty);
            SendAction(true, (ValueType.Int, 0xE), (ValueType.UInt, index), (ValueType.UInt, qty));
        }
    }
}