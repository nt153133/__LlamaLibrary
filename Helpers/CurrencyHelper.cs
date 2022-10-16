using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers
{
    public static class CurrencyHelper
    {
        private static class Offsets
        {
            [Offset(
                "48 8B 1D ? ? ? ? 48 85 DB 74 ? 48 8B CB E8 ? ? ? ? BA ? ? ? ? 48 8B CB E8 ? ? ? ? 33 D2 Add 3 TraceRelative")]
            internal static IntPtr SpecialCurrencyStorage;

            [Offset("44 0F B6 C2 84 D2 74 ? 48 8B 09")]
            internal static IntPtr GetSpecialCurrencyItemId;

            [Offset("48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 8B F1 E8 ? ? ? ? 33 DB 8B F8 85 C0 74 ? 66 90 8B CB E8 ? ? ? ? 39 70 ? 74 ? FF C3 3B DF 72 ? 33 C0 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 8B 00")]
            internal static IntPtr GetTomeItemId;
        }

        private static readonly Dictionary<uint, uint> CurrencyCache = new();

        private static readonly Dictionary<uint, uint> TomeCache = new();

        internal static InventoryBagId[] CurrencyBags = new InventoryBagId[5]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4,
            InventoryBagId.Currency
        };

        public static IntPtr SpecialCurrencyStorage => Core.Memory.Read<IntPtr>(Offsets.SpecialCurrencyStorage);

        public static uint GetCurrencyItemId(uint index)
        {
            if (CurrencyCache.ContainsKey(index))
            {
                return CurrencyCache[index];
            }

            uint result;
            lock (Core.Memory.Executor.AssemblyLock)
            {
                result = Core.Memory.CallInjected64<uint>(
                    Offsets.GetSpecialCurrencyItemId,
                    SpecialCurrencyStorage,
                    (byte)index);
            }

            CurrencyCache.Add(index, result);

            return result;
        }

        public static uint GetTomeItemId(uint index)
        {
            if (TomeCache.ContainsKey(index))
            {
                return TomeCache[index];
            }

            uint result;
            lock (Core.Memory.Executor.AssemblyLock)
            {
                result = Core.Memory.CallInjected64<uint>(
                    Offsets.GetTomeItemId,
                    (int)index);
            }

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