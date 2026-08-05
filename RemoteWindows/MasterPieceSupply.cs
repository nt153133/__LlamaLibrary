using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Managers;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    public class MasterPieceSupply : RemoteWindow<MasterPieceSupply>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "ClassSelected", 45 },
            { "NumberOfTurnins", 0 },
            { "ItemElementsStart", 87 },
            { "StarElementsStart", 447 },
        };

        public MasterPieceSupply() : base("MasterPieceSupply")
        {
        }

        public int ClassSelected
        {
            get => Elements[Properties["ClassSelected"]].Int;
            set
            {
                if (WindowByName != null && Elements[Properties["ClassSelected"]].Int != value)
                {
                    SendAction(2, 1, 2, 1, (ulong)value);
                }
            }
        }

        public int GetNumberOfTurnins()
        {
            return IsOpen ? Elements[Properties["NumberOfTurnins"]].Int : 0;
        }

        public List<Item> GetTurninItems()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["ItemElementsStart"], GetNumberOfTurnins());

            return itemElements.Select(item => DataManager.GetItem((item.UInt - 500000))).ToList();
        }

        public Dictionary<Item, bool> GetTurninItemsStarred()
        {
            var result = new Dictionary<Item, bool>();

            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["ItemElementsStart"], GetNumberOfTurnins()).ToArray();
            var starElements = new ArraySegment<TwoInt>(currentElements, Properties["StarElementsStart"], GetNumberOfTurnins()).ToArray();

            for (var i = 0; i < GetNumberOfTurnins(); i++)
            {
                result.Add(DataManager.GetItem((itemElements[i].UInt - 500000)), starElements[i].Bool);
            }

            return result;
        }

        public void ClickItem(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }
    }
}