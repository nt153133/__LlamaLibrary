using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides high-level purchasing helpers for the Allagan Tomestone / Scripture scrip exchange shops
    /// (InclusionShop). Handles navigating to the NPC, selecting the correct category and sub-category,
    /// and completing the purchase exchange dialogue.
    /// </summary>
    public static class InclusionShopHelper
    {
        private static readonly LLogger Log = new(nameof(InclusionShopHelper), Colors.Moccasin);

        /// <summary>
        /// Purchases <paramref name="qty"/> of item <paramref name="itemId"/> from the currently open
        /// <see cref="InclusionShop"/> window. Navigates to the correct shop category and sub-category
        /// before purchasing. If <paramref name="qty"/> is 0, buys as many as the player can afford.
        /// Returns the actual number of items added to the inventory, or 0 on failure.
        /// </summary>
        /// <param name="itemId">The item to purchase (raw item ID).</param>
        /// <param name="qty">
        /// Desired quantity. Pass 0 to buy the maximum affordable quantity.
        /// Clamped to the affordable amount when non-zero.
        /// </param>
        /// <returns>Number of items actually acquired, or 0 on failure.</returns>
        public static async Task<int> BuyItem(uint itemId, int qty)
        {
            if (!InclusionShop.Instance.IsOpen)
            {
                Log.Information("InclusionShop window not open");
                return 0;
            }

            await Coroutine.Sleep(200);

            var shopItems = AgentInclusionShop.Instance.ShopItems;

            var shopItem = shopItems.FirstOrDefault(i => i.RawItemIds.Contains(itemId));
            if (shopItem == null)
            {
                Log.Information($"Item {itemId} not found");
                return 0;
            }

            if (shopItem.Hidden)
            {
                Log.Information($"Item {DataManager.GetItem(shopItem.ItemId).CurrentLocaleName} is in a hidden sub category");
                return 0;
            }

            var amtToBuy = qty == 0 ? shopItem.CanAffordQty() : Math.Min(qty, shopItem.CanAffordQty());

            Log.Information($"qty: {qty} amtToBuy: {amtToBuy} CanAfford: {shopItem.CanAffordQty()}");

            if (amtToBuy == 0)
            {
                var costs = shopItem.Costs.Select(cost =>
                                                      $"{DataManager.GetItem(cost.ItemId).CurrentLocaleName} x {cost.Qty}");
                Log.Information($"You can't afford {string.Join(",", costs)}");
                return 0;
            }

            InclusionShop.Instance.SetCategory(shopItem.Category);

            await Coroutine.Wait(10000, () => AgentInclusionShop.Instance.SelectedCategory == shopItem.Category);

            if (AgentInclusionShop.Instance.SelectedCategory != shopItem.Category)
            {
                Log.Information("Couldn't change category");
                return 0;
            }

            //7.1 Used to be +1
            InclusionShop.Instance.SetSubCategory(shopItem.SubCategory + 1);

            if (!await Coroutine.Wait(2000, () => AgentInclusionShop.Instance.SelectedSubCategory == shopItem.SubCategory))
            {
                InclusionShop.Instance.SetSubCategory(shopItem.SubCategory);
                await Coroutine.Wait(5000, () => AgentInclusionShop.Instance.SelectedSubCategory == shopItem.SubCategory);
            }

            if (AgentInclusionShop.Instance.SelectedSubCategory != shopItem.SubCategory)
            {
                Log.Information("Couldn't change subcategory");
                return 0;
            }

            await Coroutine.Wait(10000, () => AgentInclusionShop.Instance.ItemCount >= shopItem.Index);

            InclusionShop.Instance.BuyItem(shopItem.Index, amtToBuy);

            await Coroutine.Wait(10000, () => ShopExchangeItemDialog.Instance.IsOpen);

            if (!ShopExchangeItemDialog.Instance.IsOpen)
            {
                Log.Information("ShopExchangeItemDialog didn't open");
                return 0;
            }

            var currentAmt = InventoryManager.FilledSlots.Where(i => i.RawItemId == shopItem.ItemId).Sum(i => i.Count);

            ShopExchangeItemDialog.Instance.Exchange();

            await Coroutine.Wait(10000, () => !ShopExchangeItemDialog.Instance.IsOpen);

            await Coroutine.Wait(
                10000,
                () => (currentAmt != InventoryManager.FilledSlots.Where(i => i.RawItemId == shopItem.ItemId)
                                     .Sum(i => i.Count)) || SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
                await Coroutine.Wait(2000, () => !SelectYesno.IsOpen);
                await Coroutine.Wait(
                    10000,
                    () => currentAmt != InventoryManager.FilledSlots.Where(i => i.RawItemId == shopItem.ItemId)
                                         .Sum(i => i.Count));
            }

            await Coroutine.Sleep(100);

            if (InclusionShop.Instance.IsOpen)
            {
                InclusionShop.Instance.Close();
                await Coroutine.Wait(10000, () => !InclusionShop.Instance.IsOpen);
                await Coroutine.Sleep(300);
            }

            return (int)(InventoryManager.FilledSlots.Where(i => i.RawItemId == shopItem.ItemId).Sum(i => i.Count) -
                          currentAmt);
        }

        /// <summary>
        /// Locates an NPC that sells <paramref name="itemId"/> via the Inclusion Shop, teleports to them
        /// (preferring the cheapest aetheryte if not already in the same zone), interacts to open the shop,
        /// then delegates to <see cref="BuyItem"/> to complete the purchase.
        /// Uses <see cref="InclusionShopConstants.KnownItems"/> to find valid shop keys and
        /// <see cref="InclusionShopConstants.ShopNpcs"/> to find NPC locations.
        /// Skips NPCs whose required quest is not completed.
        /// </summary>
        /// <param name="itemId">The item to purchase (raw item ID).</param>
        /// <param name="qty">Desired quantity (0 = buy as many as affordable).</param>
        /// <returns>Number of items actually acquired, or 0 on failure.</returns>
        public static async Task<int> BuyItemGoToNpc(uint itemId, int qty)
        {
            var shopIds = InclusionShopConstants.KnownItems.Where(i => i.Value.Contains(itemId)).Select(i => i.Key).ToList();

            var npcs = InclusionShopConstants.ShopNpcs.Where(i => shopIds.Contains(i.ShopKey) && (i.RequiredQuest == 0 || QuestLogManager.IsQuestCompleted(i.RequiredQuest))).ToList();

            if (npcs.Count == 0)
            {
                Log.Information($"Found {npcs.Count} vendors with {DataManager.GetItem(itemId)}");
                return 0;
            }

            if (npcs.Count != 0)
            {
                Log.Information($"Found {npcs.Count} vendors with {InclusionShopConstants.ShopNpcs.Count(i => shopIds.Contains(i.ShopKey))}");
            }

            (uint ShopKey, uint NpcId, ushort ZoneId, uint RequiredQuest, Vector3 Location) npcToGoTo;
            if (npcs.Any(i => i.ZoneId == WorldManager.ZoneId))
            {
                npcToGoTo = npcs.First(i => i.ZoneId == WorldManager.ZoneId);
            }
            else
            {
                var locationCostByZone = WorldManager.AvailableLocations
                    .DistinctBy(i => i.ZoneId)
                    .ToDictionary(i => i.ZoneId, i => i.GilCost);
                npcToGoTo = npcs
                    .Where(j => locationCostByZone.ContainsKey(j.ZoneId))
                    .OrderBy(j => locationCostByZone[j.ZoneId])
                    .First();
            }

            if (!InclusionShop.Instance.IsOpen)
            {
                await Navigation.GetToInteractNpcSelectString(npcToGoTo.NpcId, npcToGoTo.ZoneId, npcToGoTo.Location, 0, InclusionShop.Instance);
            }

            return await BuyItem(itemId, qty);
        }
    }
}