using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    public static class CurrencyHelper
    {
        

        private static readonly Dictionary<uint, uint> CurrencyCache = new();

        private static readonly Dictionary<uint, uint> TomeCache = new();

        internal static InventoryBagId[] CurrencyBags = {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4,
            InventoryBagId.Currency
        };

        public static IntPtr SpecialCurrencyStorage => Core.Memory.Read<IntPtr>(CurrencyHelperOffsets.SpecialCurrencyStorage);

        public static uint GetCurrencyItemId(uint index)
        {
            if (CurrencyCache.ContainsKey(index))
            {
                return CurrencyCache[index];
            }

            uint result = Core.Memory.CallInjectedWraper<uint>(
                                                              CurrencyHelperOffsets.GetSpecialCurrencyItemId,
                                                              SpecialCurrencyStorage,
                                                              (byte)index);

            CurrencyCache.Add(index, result);

            return result;
        }

        public static uint GetTomeItemId(uint index)
        {
            if (TomeCache.ContainsKey(index))
            {
                return TomeCache[index];
            }

            uint result = Core.Memory.CallInjectedWraper<uint>(
                                                              CurrencyHelperOffsets.GetTomeItemId,
                                                              (int)index);

            TomeCache.Add(index, result);

            return result;
        }

        public static uint GetAmountOfCurrency(uint itemId)
        {
            return (uint)InventoryManager.GetBagsByInventoryBagId(CurrencyBags).SelectMany(i => i.FilledSlots)
                .Where(i => i.TrueItemId == itemId).Sum(i => i.Count) + SpecialCurrencyManager.GetCurrencyCount((SpecialCurrency)itemId);
        }
    }
}