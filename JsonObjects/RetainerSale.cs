using System;
using ff14bot;
using LlamaLibrary.Extensions;
using LlamaLibrary.RemoteWindows;
using Newtonsoft.Json;

namespace LlamaLibrary.JsonObjects
{
    public class RetainerSale
    {
        public uint ItemId { get; set; }
        public uint TrueItemId { get; set; }
        public int Qty { get; set; }
        public bool HQ { get; set; }
        public DateTime SaleDate { get; set; }
        public string Buyer { get; set; }
        public int Price { get; set; }

        [JsonConstructor]
        public RetainerSale()
        {
        }

        public RetainerSale(HistoryNumber historyNumber, HistoryString historyString)
        {
            ItemId = historyNumber.ItemId;
            TrueItemId = historyNumber.TrueItemId;
            Qty = historyString.Qty;
            HQ = historyNumber.HQ;
            SaleDate = historyNumber.SoldDateTime;
            Buyer = Core.Memory.ReadStringUTF8(historyString.Buyer);
            Price = historyNumber.Price;
        }

        [JsonIgnore]
        public string ItemName => BagSlotExtensions.GetItemName(TrueItemId);

        public override string ToString()
        {
            return $"{Qty} x {ItemName} sold for {Price:N0} on {SaleDate} to {Buyer} ";
        }
    }
}