using LlamaLibrary.RemoteWindows.Atk;

namespace LlamaLibrary.RemoteWindows
{
    public class InclusionShop : RemoteWindow<InclusionShop>
    {
        public InclusionShop() : base("InclusionShop")
        {
        }

        public void SetCategory(int category)
        {
            //SendAction(2, 3, 0xC, 4, (ulong)category);
            SendAction(true, (ValueType.Int, 0xC), (ValueType.UInt, category));
        }

        public void SetSubCategory(int subCategory)
        {
            //SendAction(2, 3, 0xD, 4, (ulong)subCategory);
            SendAction(true, (ValueType.Int, 0xD), (ValueType.UInt, subCategory));
        }

        public void BuyItem(int index, int qty)
        {
            //SendAction(3, 3, 0xE, 4, (ulong)index, 4, (ulong)qty);
            SendAction(true, (ValueType.Int, 0xE), (ValueType.UInt, index), (ValueType.UInt, qty));
        }
    }
}