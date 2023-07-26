using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopCardDialog : RemoteWindow<ShopCardDialog>
    {
        public ShopCardDialog() : base("ShopCardDialog")
        {
        }

        public static Dictionary<string, int> Properties = new()
        {
            { "ItemId", 0 },
            { "CardId", 1 },
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