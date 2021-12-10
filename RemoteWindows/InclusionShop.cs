namespace LlamaLibrary.RemoteWindows
{
    public class InclusionShop : RemoteWindow<InclusionShop>
    {
        private const string WindowName = "InclusionShop";

        public InclusionShop() : base(WindowName)
        {
        }

        public void SetCategory(int category)
        {
            SendAction(2, 3, 0xC, 4, (ulong)category);
        }

        public void SetSubCategory(int subCategory)
        {
            SendAction(2, 3, 0xD, 4, (ulong)subCategory);
        }

        public void BuyItem(int index, int qty)
        {
            SendAction(3, 3, 0xE, 4, (ulong)index, 4, (ulong)qty);
        }
    }
}