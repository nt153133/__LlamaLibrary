using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class GrandCompanyExchange : RemoteWindow<GrandCompanyExchange>
    {
        private const string WindowName = "GrandCompanyExchange";

        public GrandCompanyExchange() : base(WindowName)
        {
            _name = WindowName;
        }

        public int GetNumberOfItems => IsOpen ? Elements[1].TrimmedData : 0;

        public int GCRankGroup => IsOpen ? Elements[2].TrimmedData : 0;

        public uint[] GetTurninItemsIds()
        {
            var currentElements = Elements;
            var turninIdElements = new ArraySegment<TwoInt>(currentElements, 317, GetNumberOfItems).Select(i => (uint)i.TrimmedData).ToArray();
            return turninIdElements;
        }

        public uint[] GetItemCosts()
        {
            var currentElements = Elements;
            var costElements = new ArraySegment<TwoInt>(currentElements, 67, GetNumberOfItems).Select(i => (uint)i.TrimmedData).ToArray();
            return costElements;
        }

        public void BuyItemByIndex(uint index, int qty)
        {
            SendAction(4, 3, 0, 3, index, 3, (ulong)qty, 0, 7); //Click item and qty
        }

        public void ChangeRankGroup(int rankGroup)
        {
            SendAction(2, 3, 1, 3, (ulong)rankGroup);
        }

        public void ChangeItemGroup(int itemGroup)
        {
            SendAction(2, 3, 2, 3, (ulong)itemGroup);
        }
    }
}