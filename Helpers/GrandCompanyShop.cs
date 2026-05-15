using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helpers for reading the Grand Company seal exchange shop and purchasing items from it.
    /// Uses direct memory reads of the active shop pointer for live shop data.
    /// </summary>
    public static class GrandCompanyShop
    {
        private static readonly LLogger Log = new(nameof(GrandCompanyShop), Colors.SeaGreen);

        

        /// <summary>
        /// Gets the pointer to the currently open Grand Company exchange shop structure in game memory.
        /// </summary>
        public static IntPtr ActiveShopPtr => Core.Memory.Read<IntPtr>(GrandCompanyShopOffsets.GCShopPtr);

        /// <summary>
        /// Gets the pointer to the first element of the shop item array within the active shop structure.
        /// </summary>
        public static IntPtr ListStart => ActiveShopPtr + GrandCompanyShopOffsets.GCArrayStart;

        /// <summary>
        /// Gets all non-empty items currently visible in the open Grand Company exchange shop.
        /// </summary>
        public static List<GCShopItem> Items => Core.Memory.ReadArray<GCShopItem>(ActiveShopPtr + GrandCompanyShopOffsets.GCArrayStart, GrandCompanyShopOffsets.GCShopCount).Where(i => i.ItemID != 0).ToList();

        /// <summary>
        /// Returns the maximum quantity of the given shop item the player can afford with their current GC seal balance.
        /// </summary>
        /// <param name="item">The shop item to evaluate.</param>
        /// <returns>Maximum purchasable quantity (may be 0 if the player cannot afford even one).</returns>
        public static int CanAfford(GCShopItem item)
        {
            return (int)Math.Floor((double)(Core.Me.GCSeals() / item.Cost));
        }

        /// <summary>
        /// Opens the Grand Company seal shop (if necessary), buys up to <paramref name="qty"/> of the specified item,
        /// then closes the shop.
        /// </summary>
        /// <param name="ItemId">The item ID to purchase.</param>
        /// <param name="qty">Desired quantity; actual quantity purchased is capped by the player's seal balance.</param>
        /// <returns>The actual quantity purchased, or 0 on failure.</returns>
        public static async Task<int> BuyItem(uint ItemId, int qty)
        {
            if (!await OpenShop())
            {
                return 0;
            }

            var item = Items.FirstOrDefault(i => i.ItemID == ItemId);
            Log.Information($"Want to buy {qty:N0}x {DataManager.GetItem(item.ItemID).LocaleName()} ({ItemId}) with Grand Company Seals.");

            if (item.ItemID == 0)
            {
                Log.Error($"Can't find item in Grand Company Shop 0x{(ActiveShopPtr + GrandCompanyShopOffsets.GCArrayStart).ToString("X")}");
                foreach (var item1 in Items)
                {
                    Log.Error($"{item1}");
                }

                await CloseShop();

                return 0;
            }

            if (item.Cost > Core.Me.GCSeals())
            {
                Log.Error($"Don't have enough Grand Company Seals for {item.Item.LocaleName()} ({ItemId}). Have: {Core.Me.GCSeals():N0}, Costs: {item.Cost:N0}");
                await CloseShop();

                return 0;
            }

            var oldBagQty = item.InBag;
            var qtyCanBuy = Math.Min(qty, CanAfford(item));
            Log.Information($"CanBuy {qtyCanBuy}");

            AgentGrandCompanyExchange.Instance.BuyItem(item.Index, qtyCanBuy);
            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
                Log.Information("Clicked Yes");
                await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
            }

            await Coroutine.Wait(3000, () => Items.FirstOrDefault(i => i.ItemID == ItemId).InBag != oldBagQty);

            await CloseShop();

            return qtyCanBuy;
        }

        /// <summary>
        /// Purchases each item in <paramref name="items"/> from the Grand Company seal shop,
        /// automatically switching rank group and category as needed.
        /// Uses <see cref="ResourceManager.GCShopItems"/> to look up the correct shop tab for each item.
        /// </summary>
        /// <param name="items">A list of (item ID, quantity) tuples to purchase in order.</param>
        /// <returns><see langword="true"/> if all purchases succeeded; <see langword="false"/> if any failed.</returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:Tuple element names should use correct casing")]
        public static async Task<bool> BuyKnownItems(List<(uint ItemId, int qty)> items)
        {
            if (!await OpenShop())
            {
                Log.Error("Could not open the shop");
                return false;
            }

            foreach (var (ItemId, qty) in items)
            {
                var itemInfo = ResourceManager.GCShopItems[Core.Me.GrandCompany].FirstOrDefault(i => i.ItemId == ItemId);
                if (itemInfo == null)
                {
                    Log.Error($"Could not find item {ItemId} in the GC shop");
                    continue;
                }

                if (AgentGrandCompanyExchange.Instance.Rank != itemInfo.GCRankGroup)
                {
                    Log.Information($"Change GC Rank to {itemInfo.GCRankGroup}");
                    GrandCompanyExchange.Instance.ChangeRankGroup(itemInfo.GCRankGroup);
                    await Coroutine.Wait(2000, () => AgentGrandCompanyExchange.Instance.Rank == itemInfo.GCRankGroup);
                    await Coroutine.Sleep(500);
                }

                if (AgentGrandCompanyExchange.Instance.Category + 1 != (byte)itemInfo.Category)
                {
                    Log.Information($"Change ChangeItemGroup to {itemInfo.Category}");
                    GrandCompanyExchange.Instance.ChangeItemGroup((int)itemInfo.Category);
                    await Coroutine.Wait(3000, () => AgentGrandCompanyExchange.Instance.Category + 1 == (byte)itemInfo.Category);
                    await Coroutine.Sleep(500);
                }

                var item = Items.FirstOrDefault(i => i.ItemID == ItemId);
                Log.Information($"Want to buy {DataManager.GetItem(item.ItemID).LocaleName()}");
                if (item.ItemID == 0)
                {
                    Log.Error($"Can't find item {(ActiveShopPtr + GrandCompanyShopOffsets.GCArrayStart).ToString("X")}");
                    foreach (var item1 in Items)
                    {
                        Log.Error($"{item1}");
                    }

                    await CloseShop();

                    return false;
                }

                if (item.Cost > Core.Me.GCSeals())
                {
                    Log.Error($"Don't have enough seals for {item.Item.LocaleName()}");
                    continue;
                }

                var oldBagQty = item.InBag;
                var qtyCanBuy = Math.Min(qty, CanAfford(item));
                Log.Information($"CanBuy {qtyCanBuy}");
                AgentGrandCompanyExchange.Instance.BuyItem(item.Index, qtyCanBuy);
                await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                if (SelectYesno.IsOpen)
                {
                    SelectYesno.Yes();
                    Log.Information("Clicked Yes");
                    await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
                }

                await Coroutine.Wait(3000, () => Items.FirstOrDefault(i => i.ItemID == ItemId).InBag != oldBagQty);
            }

            await CloseShop();

            return true;

            /*
            if (KnownItems.TryGetValue(ItemId, out var itemInfo2))
            {
                Log.Information($"Found Known item {ItemId}");
                return await BuyItem(ItemId, qty, itemInfo.Item1, itemInfo.Item2);
            }

            return false;
            */
        }

        /// <summary>
        /// Purchases the specified item from the Grand Company seal shop using cached rank/category metadata.
        /// </summary>
        /// <param name="ItemId">The item ID to purchase.</param>
        /// <param name="qty">Desired quantity.</param>
        /// <returns>Actual quantity purchased, or 0 if the item is not found in <see cref="ResourceManager.GCShopItems"/>.</returns>
        public static async Task<int> BuyKnownItem(uint ItemId, int qty)
        {
            var item = ResourceManager.GCShopItems[Core.Me.GrandCompany].FirstOrDefault(i => i.ItemId == ItemId);

            if (item == null)
            {
                Log.Error($"Can't find item {ItemId}");
                return 0;
            }

            Log.Information($"Found Known item {ItemId}");
            return await BuyItem(ItemId, qty, item.GCRankGroup, item.Category);
        }

        /// <summary>
        /// Opens the GC seal shop, switches to the specified rank group and category, then purchases
        /// up to <paramref name="qty"/> of the given item.
        /// </summary>
        /// <param name="ItemId">The item ID to purchase.</param>
        /// <param name="qty">Desired quantity.</param>
        /// <param name="GCRankGroup">Rank group tab index in the shop UI.</param>
        /// <param name="Category">Item category tab in the shop UI.</param>
        /// <returns>Actual quantity purchased, or 0 on failure.</returns>
        public static async Task<int> BuyItem(uint ItemId, int qty, int GCRankGroup, GCShopCategory Category)
        {
            if (!await OpenShop())
            {
                return 0;
            }

            if (AgentGrandCompanyExchange.Instance.Rank != GCRankGroup)
            {
                Log.Information($"Change GC Rank to {GCRankGroup}");
                GrandCompanyExchange.Instance.ChangeRankGroup(GCRankGroup);
                await Coroutine.Wait(2000, () => AgentGrandCompanyExchange.Instance.Rank == GCRankGroup);
                await Coroutine.Sleep(500);
            }

            if (AgentGrandCompanyExchange.Instance.Category + 1 != (byte)Category)
            {
                Log.Information($"Change ChangeItemGroup to {Category}");
                GrandCompanyExchange.Instance.ChangeItemGroup((int)Category);
                await Coroutine.Wait(3000, () => AgentGrandCompanyExchange.Instance.Category + 1 == (byte)Category);
                await Coroutine.Sleep(500);
            }

            return await BuyItem(ItemId, qty);
        }

        /// <summary>
        /// Ensures the Grand Company exchange window is open, navigating to the Quartermaster NPC if needed.
        /// </summary>
        /// <returns><see langword="true"/> if the shop is open; <see langword="false"/> otherwise.</returns>
        public static async Task<bool> OpenShop()
        {
            if (!GrandCompanyExchange.Instance.IsOpen)
            {
                if (Navigator.NavigationProvider == null)
                {
                    Navigator.PlayerMover = new SlideMover();
                    Navigator.NavigationProvider = new ServiceNavigationProvider();
                }

                Log.Information("Calling interact with npc");
                await GrandCompanyHelper.InteractWithNpc(GCNpc.Quartermaster);

                if (!await Coroutine.Wait(5000, () => GrandCompanyExchange.Instance.IsOpen))
                {
                    await GrandCompanyHelper.InteractWithNpc(GCNpc.Quartermaster);
                    await Coroutine.Wait(5000, () => GrandCompanyExchange.Instance.IsOpen);
                }
            }

            return GrandCompanyExchange.Instance.IsOpen;
        }

        /// <summary>
        /// Closes the Grand Company exchange window and clears the current target.
        /// </summary>
        /// <returns><see langword="true"/> if the shop closed successfully; <see langword="false"/> otherwise.</returns>
        public static async Task<bool> CloseShop()
        {
            GrandCompanyExchange.Instance.Close();
            await Coroutine.Wait(5000, () => !GrandCompanyExchange.Instance.IsOpen);

            if (!GrandCompanyExchange.Instance.IsOpen)
            {
                Core.Me.ClearTarget();
                await Coroutine.Sleep(500);
            }

            return !GrandCompanyExchange.Instance.IsOpen;
        }

        /// <summary>
        /// Checks whether the given item ID is available for purchase at any Grand Company seal shop,
        /// using <see cref="ResourceManager.GCShopItems"/> as the source of truth.
        /// </summary>
        /// <param name="itemId">The item ID to check.</param>
        /// <returns><see langword="true"/> if the item is sold at a GC shop; <see langword="false"/> otherwise.</returns>
        public static bool IsBuyableItem(uint itemId)
        {
            return ResourceManager.GCShopItems.SelectMany(i => i.Value.Select(j => j.ItemId)).Contains(itemId);
        }
    }

    /// <summary>
    /// Represents the item category tabs available in the Grand Company seal exchange shop.
    /// </summary>
    public enum GCShopCategory
    {
        /// <summary>Materiel tab (crafting/gathering materials purchasable with GC seals).</summary>
        Materiel = 1,
        /// <summary>Weapons tab.</summary>
        Weapons = 2,
        /// <summary>Armor tab.</summary>
        Armor = 3,
        /// <summary>Materials tab (raw materials).</summary>
        Materials = 4
    }
}