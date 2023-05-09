using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary...Also wtf is this mess
    public class ContentsInfoDetail : RemoteWindow<ContentsInfoDetail>
    {
        private readonly int eleNumCrafting = 56;
        private readonly int eleNumGathering = 57;
        private readonly int eleCraftingItem = 233;
        private readonly int eleCraftingJob = 241;
        private readonly int eleCraftingQty = 249;
        private readonly int eleGatheringItem = 292;
        private readonly int eleGatheringJob = 295;
        private readonly int eleGatheringQty = 298;

        public ContentsInfoDetail() : base("ContentsInfoDetail")
        {
            /*
            if (Translator.Language == Language.Chn)
            {
                eleNumCrafting = 56;
                eleNumGathering = 57;
                eleCraftingItem = 215;
                eleCraftingJob = 223;
                eleCraftingQty = 231;
                eleGatheringItem = 274;
                eleGatheringJob = 277;
                eleGatheringQty = 280;
                //6.3
                /*
                eleNumCrafting = 56;
                eleNumGathering = 57;
                eleCraftingItem = 214;
                eleCraftingJob = 222;
                eleCraftingQty = 230;
                eleGatheringItem = 273;
                eleGatheringJob = 276;
                eleGatheringQty = 279;
                #1#

                /*eleNumCrafting = 51;
                eleNumGathering = 52;
                eleCraftingItem = 205;
                eleCraftingJob = 213;
                eleCraftingQty = 221;
                eleGatheringItem = 264;
                eleGatheringJob = 267;
                eleGatheringQty = 270;#1#
            }
            */

        }

        public int GetNumberOfCraftingTurnins()
        {
            return IsOpen ? Elements[eleNumCrafting].TrimmedData : 0;
        }

        public int GetNumberOfGatheringTurnins()
        {
            return IsOpen ? Elements[eleNumGathering].TrimmedData : 0;
        }

        public List<Item> GetCraftingTurninItemsIds()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, 209, GetNumberOfCraftingTurnins());

            return itemElements.Select(item => DataManager.GetItem((uint)item.TrimmedData)).ToList();
        }

        public List<Item> GetGatheringTurninItemsIds()
        {
            var currentElements = Elements;

            var itemElements = new ArraySegment<TwoInt>(currentElements, 268, GetNumberOfGatheringTurnins());

            return itemElements.Select(item => DataManager.GetItem((uint)item.TrimmedData)).ToList();
        }

        public Dictionary<Item, KeyValuePair<int, string>> GetCraftingTurninItems()
        {
            var result = new Dictionary<Item, KeyValuePair<int, string>>();
            var currentElements = Elements;
            var itemElements = new ArraySegment<TwoInt>(currentElements, eleCraftingItem, GetNumberOfCraftingTurnins()).ToArray();
            var jobElements = new ArraySegment<TwoInt>(currentElements, eleCraftingJob, GetNumberOfCraftingTurnins()).ToArray();
            var qtyElements = new ArraySegment<TwoInt>(currentElements, eleCraftingQty, GetNumberOfCraftingTurnins()).ToArray();

            for (var i = 0; i < GetNumberOfCraftingTurnins(); i++)
            {
                result.Add(DataManager.GetItem((uint)itemElements[i].TrimmedData), new KeyValuePair<int, string>(qtyElements[i].TrimmedData, ((RetainerRole)jobElements[i].TrimmedData).ToString()));
            }

            return result;
        }

        public Dictionary<Item, KeyValuePair<int, string>> GetGatheringTurninItems()
        {
            var result = new Dictionary<Item, KeyValuePair<int, string>>();
            var currentElements = Elements;
            var itemElements = new ArraySegment<TwoInt>(currentElements, eleGatheringItem, GetNumberOfGatheringTurnins()).ToArray();
            var jobElements = new ArraySegment<TwoInt>(currentElements, eleGatheringJob, GetNumberOfGatheringTurnins()).ToArray();
            var qtyElements = new ArraySegment<TwoInt>(currentElements, eleGatheringQty, GetNumberOfGatheringTurnins()).ToArray();

            for (var i = 0; i < GetNumberOfGatheringTurnins(); i++)
            {
                result.Add(DataManager.GetItem((uint)itemElements[i].TrimmedData), new KeyValuePair<int, string>(qtyElements[i].TrimmedData, ((RetainerRole)jobElements[i].TrimmedData).ToString()));
            }

            return result;
        }
    }
}