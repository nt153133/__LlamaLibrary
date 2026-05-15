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
    /// <summary>
    /// Provides methods for querying currency amounts from the player's inventory and the game's
    /// special-currency manager (e.g., tomestones, GC seals).
    /// Item ID lookups for special currencies are cached after the first injected game call.
    /// </summary>
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

        /// <summary>
        /// Gets the pointer to the game's special currency storage structure in memory.
        /// Used as the first argument to the <c>GetSpecialCurrencyItemId</c> injected function.
        /// </summary>
        public static IntPtr SpecialCurrencyStorage => Core.Memory.Read<IntPtr>(CurrencyHelperOffsets.SpecialCurrencyStorage);

        /// <summary>
        /// Returns the item ID for the special currency at the given index.
        /// Results are cached so the injected game call is only made once per index per session.
        /// </summary>
        /// <param name="index">Zero-based index into the special currency table.</param>
        /// <returns>The item ID corresponding to that currency slot.</returns>
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

        /// <summary>
        /// Returns the item ID for the tomestone currency at the given index.
        /// Results are cached so the injected game call is only made once per index per session.
        /// </summary>
        /// <param name="index">Index into the tomestone currency table.</param>
        /// <returns>The item ID for that tomestone type.</returns>
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

        /// <summary>
        /// Returns the total amount of the given currency held by the local player,
        /// summing across all main bags (Bag1–Bag4 and the dedicated Currency bag) as well as
        /// the game's <see cref="SpecialCurrencyManager"/>.
        /// </summary>
        /// <param name="itemId">
        /// The item ID of the currency to count (e.g., a <see cref="SpecialCurrency"/> cast to <see langword="uint"/>).
        /// </param>
        /// <returns>Total amount held across all sources.</returns>
        public static uint GetAmountOfCurrency(uint itemId)
        {
            return (uint)InventoryManager.GetBagsByInventoryBagId(CurrencyBags).SelectMany(i => i.FilledSlots)
                .Where(i => i.TrueItemId == itemId).Sum(i => i.Count) + SpecialCurrencyManager.GetCurrencyCount((SpecialCurrency)itemId);
        }
    }
}