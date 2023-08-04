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

namespace LlamaLibrary.Helpers
{
    public static class GrandCompanyShop
    {
        private static readonly LLogger Log = new(nameof(GrandCompanyShop), Colors.SeaGreen);

        internal static class Offsets
        {
            [Offset("Search 0F B6 15 ? ? ? ? 8D 42 ? 3C ? 77 ? FE CA 48 8D 0D ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr CurrentGC;

            [Offset("Search 48 83 EC ? 48 8B 05 ? ? ? ? 44 8B C1 BA ? ? ? ? 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ? C3 48 8B 00 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? 80 F9 ?")]
            internal static IntPtr GCGetMaxSealsByRank;

            [Offset("Search 48 8D 9E ? ? ? ? 4C 89 B4 24 ? ? ? ? Add 3 Read32")]
            internal static int GCArrayStart;

            [Offset("Search 41 83 FD ? 0F 82 ? ? ? ? 41 0F B6 97 ? ? ? ? Add 3 Read8")]
            internal static int GCShopCount;

            [Offset("Search 48 8B 05 ? ? ? ? 33 C9 40 84 FF Add 3 TraceRelative")]
            internal static IntPtr GCShopPtr;
        }

        public static IntPtr ActiveShopPtr => Core.Memory.Read<IntPtr>(Offsets.GCShopPtr);

        public static IntPtr ListStart => ActiveShopPtr + Offsets.GCArrayStart;

        public static List<GCShopItem> Items => Core.Memory.ReadArray<GCShopItem>(ActiveShopPtr + Offsets.GCArrayStart, Offsets.GCShopCount).Where(i => i.ItemID != 0).ToList();

        public static int CanAfford(GCShopItem item)
        {
            return (int)Math.Floor((double)(Core.Me.GCSeals() / item.Cost));
        }

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
                Log.Error($"Can't find item in Grand Company Shop 0x{(ActiveShopPtr + Offsets.GCArrayStart).ToString("X")}");
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
                Log.Information($"Clicked Yes");
                await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
            }

            await Coroutine.Wait(3000, () => Items.FirstOrDefault(i => i.ItemID == ItemId).InBag != oldBagQty);

            await CloseShop();

            return qtyCanBuy;
        }

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
                    Log.Error($"Can't find item {(ActiveShopPtr + Offsets.GCArrayStart).ToString("X")}");
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
                    Log.Information($"Clicked Yes");
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

        public static bool IsBuyableItem(uint itemId)
        {
            return ResourceManager.GCShopItems.SelectMany(i => i.Value.Select(j => j.ItemId)).Contains(itemId);
        }
    }

    public enum GCShopCategory
    {
        Materiel = 1,
        Weapons = 2,
        Armor = 3,
        Materials = 4
    }
}