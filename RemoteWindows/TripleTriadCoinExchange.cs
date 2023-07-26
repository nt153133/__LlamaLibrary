using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    public class TripleTriadCoinExchange : RemoteWindow<TripleTriadCoinExchange>
    {
        public TripleTriadCoinExchange() : base("TripleTriadCoinExchange")
        {
        }

        public static Dictionary<string, int> Properties = new()
        {
            { "NumberOfItems", 1 },
            { "ElementPrices", 84 },
            { "ElementQuantities", 124 },
            { "TurninIdElements", 164 },
            { "ElementInDeck", 204 },
        };

        public int GetNumberOfItems => IsOpen ? Elements[Properties["NumberOfItems"]].TrimmedData : 0;

        public uint[] GetTurninItemsIds()
        {
            var currentElements = Elements;
            var turninIdElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninIdElements"], GetNumberOfItems).Select(i => (uint)i.TrimmedData).ToArray();
            return turninIdElements;
        }

        public uint[] GetCardPrices()
        {
            var currentElements = Elements;
            var costElements = new ArraySegment<TwoInt>(currentElements, Properties["ElementPrices"], GetNumberOfItems).Select(i => (uint)i.TrimmedData).ToArray();
            return costElements;
        }

        public uint[] GetCardQuantities()
        {
            var currentElements = Elements;
            var costElements = new ArraySegment<TwoInt>(currentElements, Properties["ElementQuantities"], GetNumberOfItems).Select(i => (uint)i.TrimmedData).ToArray();
            return costElements;
        }

        public bool[] GetCardInDeck()
        {
            var currentElements = Elements;
            var costElements = new ArraySegment<TwoInt>(currentElements, Properties["ElementInDeck"], GetNumberOfItems).Select(i => (int)i.TrimmedData == 1 ? true : false).ToArray();
            return costElements;
        }

        public void OpenShopCardDialog(uint index)
        {
            SendAction(2, 3, 0, 4, index);
        }
    }
}