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
    public static class InclusionShopHelper
    {
        private static readonly LLogger Log = new(nameof(InclusionShopHelper), Colors.Moccasin);

        public static async Task<int> BuyItem(uint itemId, int qty)
        {
            if (!LlamaLibrary.RemoteWindows.InclusionShop.Instance.IsOpen)
            {
                Log.Information("InclusionShop window not open");
                return 0;
            }

            await Coroutine.Sleep(200);

            var shopItems = AgentInclusionShop.Instance.ShopItems;

            if (!shopItems.Any(i => i.RawItemIds.Contains(itemId)))
            {
                Log.Information($"Item {itemId} not found");
                return 0;
            }

            var shopItem = shopItems.First(i => i.RawItemIds.Contains(itemId));

            if (shopItem.Hidden)
            {
                Log.Information($"Item {DataManager.GetItem(shopItem.ItemId).CurrentLocaleName} is in a hidden sub category");
                return 0;
            }

            int amtToBuy;

            amtToBuy = qty == 0 ? shopItem.CanAffordQty() : Math.Min(qty, shopItem.CanAffordQty());

            Log.Information($"qty: {qty} amtToBuy: {amtToBuy} CanAfford: {shopItem.CanAffordQty()}");
            /*
             return 0;
 */
            if (amtToBuy == 0)
            {
                var costs = shopItem.Costs.Select(cost =>
                                                      $"{DataManager.GetItem(cost.ItemId).CurrentLocaleName} x {cost.Qty}");
                Log.Information($"You can't afford {string.Join(",", costs)}");
                return 0;
            }

            LlamaLibrary.RemoteWindows.InclusionShop.Instance.SetCategory(shopItem.Category);

            await Coroutine.Wait(10000, () => AgentInclusionShop.Instance.SelectedCategory == shopItem.Category);

            if (AgentInclusionShop.Instance.SelectedCategory != shopItem.Category)
            {
                Log.Information("Couldn't change category");
                return 0;
            }

            LlamaLibrary.RemoteWindows.InclusionShop.Instance.SetSubCategory(shopItem.SubCategory + 1);

            await Coroutine.Wait(10000, () => AgentInclusionShop.Instance.SelectedSubCategory == shopItem.SubCategory);

            if (AgentInclusionShop.Instance.SelectedSubCategory != shopItem.SubCategory)
            {
                Log.Information("Couldn't change subcategory");
                return 0;
            }

            await Coroutine.Wait(10000, () => AgentInclusionShop.Instance.ItemCount >= shopItem.Index);

            LlamaLibrary.RemoteWindows.InclusionShop.Instance.BuyItem(shopItem.Index, amtToBuy);

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

        public static async Task<int> BuyItemGoToNpc(uint itemId, int qty)
        {
            var shopIds = InclusionShopConstants.KnownItems.Where(i => i.Value.Contains(itemId)).Select(i => i.Key);

            var npcs = InclusionShopConstants.ShopNpcs.Where(i => shopIds.Contains(i.ShopKey)).Where(i => i.RequiredQuest == 0 || QuestLogManager.IsQuestCompleted(i.RequiredQuest));

            if (!npcs.Any())
            {
                Log.Information($"Found {npcs.Count()} vendors with {DataManager.GetItem(itemId)}");
                return 0;
            }

            if (npcs.Any())
            {
                Log.Information($"Found {npcs.Count()} vendors with {InclusionShopConstants.ShopNpcs.Count(i => shopIds.Contains(i.ShopKey))}");
            }

            (uint ShopKey, uint NpcId, ushort ZoneId, uint RequiredQuest, Vector3 Location) npcToGoTo;
            if (npcs.Any(i => i.ZoneId == WorldManager.ZoneId))
            {
                npcToGoTo = npcs.First(i => i.ZoneId == WorldManager.ZoneId);
            }
            else
            {
                npcToGoTo = npcs.Where(j => WorldManager.AvailableLocations.Any(i => i.ZoneId == j.ZoneId)).OrderBy(j => WorldManager.AvailableLocations.First(i => i.ZoneId == j.ZoneId).GilCost)
                    .First();
            }

            if (!InclusionShop.Instance.IsOpen)
            {
                await Navigation.GetToInteractNpcSelectString(npcToGoTo.NpcId, npcToGoTo.ZoneId, npcToGoTo.Location, 0, LlamaLibrary.RemoteWindows.InclusionShop.Instance);
            }

            return await BuyItem(itemId, qty);
        }
    }
}