using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopCardDialog : RemoteWindow<ShopCardDialog>
    {
        public ShopCardDialog() : base("ShopCardDialog")
        {
        }

        public static Dictionary<string, int> Properties = new()
        {
#if RB_DT
            { "CardId", 0 },
            { "ItemId", 1 },
#else
            { "ItemId", 0 },
            { "CardId", 1 },
#endif
            { "Price", 4 },
            { "QuantityOwned", 6 },
        };

        public int ItemId => IsOpen ? Elements[Properties["ItemId"]].TrimmedData : 0;
        public int CardId => IsOpen ? Elements[Properties["CardId"]].TrimmedData : 0;
        public int Price => IsOpen ? Elements[Properties["Price"]].TrimmedData : 0;
        public int QuantityOwned => IsOpen ? Elements[Properties["QuantityOwned"]].TrimmedData : 0;

        public void ExchangeCard(int qty)
        {
            SendAction(3, 3, 0, 3, (ulong)qty, 0, 7);
        }

        public void ExchangeAllCards()
        {
            ExchangeCard(QuantityOwned);
        }
    }
}