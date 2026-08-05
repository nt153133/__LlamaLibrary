using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Structs;
#pragma warning disable CS0618 // Type or member is obsolete

namespace LlamaLibrary.RemoteWindows
{
    public class SubmarinePartsMenu : RemoteWindow<SubmarinePartsMenu>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "CraftItemID", 0 },
            { "NumberOfTurnins", 11 },
            { "TurninItemsStart", 12 },
            { "TurninItemsQtyStart", 60 },
            { "ItemAvailCountStart", 72 },
            { "TurninsDoneStart", 108 },
            { "TurninsRequiredStart", 120 },
        };

        public SubmarinePartsMenu() : base("SubmarinePartsMenu")
        {
        }

        public void ClickItem(int index)
        {
            SendAction(3, 3, 0, 4, (ulong)index, 4, 6);
        }

        public int GetNumberOfTurnins()
        {
            return IsOpen ? Elements[Properties["NumberOfTurnins"]].Int : 0;
        }

        public int GetCraftItemID()
        {
            return IsOpen ? Elements[Properties["CraftItemID"]].Int : 0;
        }

        public List<Item> GetTurninItemsObjs()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninItemsStart"], GetNumberOfTurnins());

            return itemElements.Select(item => DataManager.GetItem(item.UInt)).ToList();
        }

        public List<int> GetTurninItemsIds()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninItemsStart"], GetNumberOfTurnins());

            return itemElements.Select(item => item.Int).ToList();
        }

        public List<int> GetTurninItemsQty()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninItemsQtyStart"], GetNumberOfTurnins());

            return itemElements.Select(item => item.Int).ToList();
        }

        public List<int> GetTurninsRequired()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninsRequiredStart"], GetNumberOfTurnins());

            return itemElements.Select(item => item.Int).ToList();
        }

        public List<int> GetTurninsDone()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["TurninsDoneStart"], GetNumberOfTurnins());

            return itemElements.Select(item => item.Int).ToList();
        }

        public List<int> GetItemAvailCount()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, Properties["ItemAvailCountStart"], GetNumberOfTurnins());

            return itemElements.Select(item => item.Int).ToList();
        }

        public List<FCWorkshopItem> GetCraftingTurninItems()
        {
            var result = new List<FCWorkshopItem>();
            var itemElements = GetTurninItemsIds();
            var requiredElements = GetTurninsRequired();
            var qtyElements = GetTurninItemsQty();

            for (var i = 0; i < GetNumberOfTurnins(); i++)
            {
                result.Add(new FCWorkshopItem(itemElements[i], qtyElements[i], requiredElements[i]));
            }

            return result;
        }
    }
}