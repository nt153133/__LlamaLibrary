using System.Collections.Generic;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class CollectablesShop : RemoteWindow<CollectablesShop>
    {
        public int RowCount => Elements[20].Int - 1;
        public int TurninCount => Elements[4843].Int;
        public CollectablesShop() : base("CollectablesShop")
        {
        }

        public void SelectJob(int job)
        {
            SendAction(2, 3, 0xE, 4, (ulong)job);
        }

        public void SelectItem(int line)
        {
            SendAction(2, 3, 0xC, 4, (ulong)line);
        }

        public void Trade()
        {
            SendAction(2, 3, 0xf, 4, 0);
        }

        public List<string> ListItems()
        {
            var count = Elements[20].Int - 1;
            var currentElements = Elements;
            var result = new List<string>();
            for (var j = 0; j < count; j++)
            {
                if (currentElements[32 + (j * 11)].Int == Elements[21].Int)
                {
                    continue; //IconID
                }

                var itemID = currentElements[34 + (j * 11)].Int;
                if (itemID is 0 or > 1500000 or < 500000)
                {
                    continue;
                }

                result.Add($"{j}: {DataManager.GetItem((uint)(itemID - 500000))} {itemID}");

                //Logger.Info($"{itemID}");
            }

            return result;
        }

        public List<(uint ItemId, int Line)> GetItems()
        {
            var count = Elements[20].Int - 1;
            var currentElements = Elements;
            var result = new List<(uint ItemId, int Line)>();
            var index = 0;
            for (var j = 0; j < count; j++)
            {
                if (currentElements[34 + (j * 11)].Type == 0)
                {
                    continue;
                }

                if (currentElements[32 + (j * 11)].Int == Elements[21].Int)
                {
                    continue; //IconID
                }

                var itemID = currentElements[34 + (j * 11)].TrimmedData;
                if (itemID is 0 or > 1500000 or < 500000)
                {
                    continue;
                }

                result.Add(((uint)(itemID - 500000), index));
                index++;
            }

            return result;
        }
    }
}