using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Helpers
{
    //TODO So many hardcoded values...
    public static class IshgardHandin
    {
        private static readonly LLogger Log = new LLogger(typeof(IshgardHandin).Name, Colors.Aquamarine);

        private const int FirmamentZoneId = 886;
        private const float InteractMaxRange = 4f;
        private const int SkybuildersScripMax = 10_000; // TODO: Properly support currency maximums/caps

        /// <summary>
        /// Gets the Firmament's gathering "Resource Inspector" NPC.
        /// </summary>
        private static GameObject GatherInspectNpc => GameObjectManager.GameObjects.FirstOrDefault(i => i.NpcId == 1031693 && i.IsVisible);

        /// <summary>
        /// Gets the Firmament's crafting "Collectable Appraiser" NPC.
        /// </summary>
        private static GameObject CraftInspectNpc => GameObjectManager.GameObjects.FirstOrDefault(i => new uint[] { 1031690, 1031677 }.Contains(i.NpcId) && i.IsVisible);

        /// <summary>
        /// Gets the Firmament's "Kupo of Fortune" minigame NPC.
        /// </summary>
        private static GameObject KupoFortuneNpc => GameObjectManager.GameObjects.FirstOrDefault(i => i.NpcId == 1031692 && i.IsVisible);

        /// <summary>
        /// Gets the Firmament's Skybuilder Scrip + Fête Token vendor NPC.
        /// </summary>
        private static GameObject ScripAndFeteVendorNpc => GameObjectManager.GameObjects.FirstOrDefault(i => i.NpcId == 1031680 && i.IsVisible);

        /// <summary>
        /// Travels to the Firmament and converts Skybuilders' gatherables into "Approved" versions for specified gathering job.
        /// </summary>
        /// <param name="job">Gathering job to approve gatherables for.</param>
        /// <param name="stopScripMax">Whether to stop approving at max scrips or allow overcapping.</param>
        /// <returns><see langword="false"/> if execution should continue from here.</returns>
        public static async Task<bool> HandInGatheringItem(int job, bool stopScripMax = false)
        {
            Log.Information("Trying to turn in Skybuilders' gatherables.");

            if ((!HWDGathereInspect.Instance.IsOpen && GatherInspectNpc == null) || GatherInspectNpc.Location.Distance(Core.Me.Location) > 5f)
            {
                Log.Information("Resource Inspector NPC not nearby.  Moving to correct area first.");
                await Navigation.GetTo(FirmamentZoneId, new Vector3(-20.04274f, -16f, 141.3337f));
            }

            if (!HWDGathereInspect.Instance.IsOpen && GatherInspectNpc.Location.Distance(Core.Me.Location) > InteractMaxRange)
            {
                Log.Information("Approaching interaction range of Resource Inspector NPC.");
                await Navigation.OffMeshMove(GatherInspectNpc.Location);
                await Coroutine.Sleep(500);
            }

            if (!HWDGathereInspect.Instance.IsOpen)
            {
                Log.Information($"Interacting with Resource Inspector NPC: {GatherInspectNpc.Name} ({GatherInspectNpc.NpcId}).");
                GatherInspectNpc.Interact();
                await Coroutine.Wait(5000, () => HWDGathereInspect.Instance.IsOpen || Talk.DialogOpen);
                await Coroutine.Sleep(100);

                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(5000, () => !Talk.DialogOpen);
                }

                Log.Verbose("Talking done.");

                await Coroutine.Wait(5000, () => HWDGathereInspect.Instance.IsOpen);
                Log.Information("Resource Inspector should be ready.");
            }

            if (HWDGathereInspect.Instance.IsOpen)
            {
                Log.Information("Resource Inspector is ready.");
                HWDGathereInspect.Instance.ClickClass(job);
                await Coroutine.Sleep(500);

                if (HWDGathereInspect.Instance.CanAutoSubmit())
                {
                    Log.Debug("Auto-submitting Skybuilders' gatherables.");
                    HWDGathereInspect.Instance.ClickAutoSubmit();

                    await Coroutine.Wait(6000, () => HWDGathereInspect.Instance.CanRequestInspection());
                    if (HWDGathereInspect.Instance.CanRequestInspection())
                    {
                        Log.Information($"Inspecting Skybuilders' gatherables for gathering job {job}.");
                        HWDGathereInspect.Instance.ClickRequestInspection();
                        if (ScriptConditions.Helpers.GetSkybuilderScrips() > 9000)
                        {
                            await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
                        }
                        else
                        {
                            await Coroutine.Sleep(100);
                        }

                        if (SelectYesno.IsOpen && !stopScripMax)
                        {
                            Log.Warning($"Have {ScriptConditions.Helpers.GetSkybuilderScrips():N}/{SkybuildersScripMax:N} Skybuilders' Scrips and will overcap, but force-inspecting anyway.");
                            SelectYesno.Yes();
                        }
                        else if (SelectYesno.IsOpen && stopScripMax)
                        {
                            Log.Warning($"Have {ScriptConditions.Helpers.GetSkybuilderScrips():N}/{SkybuildersScripMax:N} Skybuilders' Scrips and will overcap.  Stopping here.");

                            SelectYesno.No();
                            await Coroutine.Wait(3000, () => !SelectYesno.IsOpen);

                            HWDGathereInspect.Instance.Close();
                            await Coroutine.Wait(3000, () => !HWDGathereInspect.Instance.IsOpen);

                            return false;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Travels to the Firmament and plays "Kupo of Fortune" minigame with every available Kupo Voucher.
        /// </summary>
        /// <param name="slot">Slot to scratch off of Kupo Voucher.</param>
        /// <returns><see langword="false"/> if execution should continue from here.</returns>
        public static async Task<bool> HandInKupoVoucher(int slot)
        {
            Log.Information("Trying to play Kupo of Fortune minigame.");

            if ((!HWDLottery.Instance.IsOpen && KupoFortuneNpc == null) || KupoFortuneNpc.Location.Distance(Core.Me.Location) > InteractMaxRange)
            {
                Log.Information("Kupo of Fortune NPC not nearby.  Moving to correct area first.");
                await Navigation.GetTo(FirmamentZoneId, new Vector3(43.59162f, -16f, 170.3864f));
            }

            if (!HWDLottery.Instance.IsOpen && KupoFortuneNpc.Location.Distance(Core.Me.Location) > InteractMaxRange)
            {
                Log.Information("Approaching interaction range of Kupo of Fortune NPC.");
                await Navigation.OffMeshMove(KupoFortuneNpc.Location);
                await Coroutine.Sleep(500);
            }

            if (!HWDLottery.Instance.IsOpen && KupoFortuneNpc != null)
            {
                Log.Information($"Interacting with Kupo of Fortune NPC: {KupoFortuneNpc.Name} ({KupoFortuneNpc.NpcId}).");
                KupoFortuneNpc.Interact();
                await Coroutine.Wait(5000, () => HWDLottery.Instance.IsOpen || Talk.DialogOpen);
                await Coroutine.Sleep(100);

                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(5000, () => !Talk.DialogOpen);
                }

                Log.Verbose("Talking done.");

                await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
                if (SelectYesno.IsOpen)
                {
                    Log.Verbose("SelectYesNo open; clicking Yes to play.");
                    SelectYesno.Yes();

                    await Coroutine.Wait(5000, () => HWDLottery.Instance.IsOpen);
                    await Coroutine.Sleep(4000);
                    Log.Information("Kupo of Fortune should be ready.");
                }
            }

            if (HWDLottery.Instance.IsOpen)
            {
                Log.Information("Kupo of Fortune is ready.");
                Log.Information($"Scratching off slot {slot}.");
                await HWDLottery.Instance.ClickSpot(slot);
                await Coroutine.Sleep(700);

                Log.Verbose("Closing");
                HWDLottery.Instance.Close();

                await Coroutine.Wait(3000, () => !HWDLottery.Instance.IsOpen);
                if (HWDLottery.Instance.IsOpen)
                {
                    Log.Verbose("Closing Again");
                    HWDLottery.Instance.Close();
                }

                await Coroutine.Wait(5000, () => SelectYesno.IsOpen || Talk.DialogOpen);
                Log.Verbose($"Select Yes/No {SelectYesno.IsOpen} Talk {Talk.DialogOpen}");
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(3000, () => !Talk.DialogOpen);
                    await Coroutine.Wait(3000, () => Talk.DialogOpen || SelectYesno.IsOpen);
                }

                await Coroutine.Sleep(500);
                await HandInKupoVoucher(slot);
            }
            else
            {
                Log.Information("Out of Kupo Vouchers.");
            }

            Log.Information("Done playing Kupo of Fortune.");
            return false;
        }

        /// <summary>
        /// Travels to the Firmament and turns in Skybuilders' collectables for specified crafting job.
        /// </summary>
        /// <param name="itemId">ItemID of collectable to turn in.</param>
        /// <param name="index">Zero-based index of row for item in appraisal window, starting from top.</param>
        /// <param name="job">Crafting job to turn in collectables for.</param>
        /// <param name="stopScripMax">Whether to stop turn-ins at max scrips or allow overcapping.</param>
        /// <returns><see langword="false"/> if execution should continue from here.</returns>
        public static async Task<bool> HandInItem(uint itemId, int index, int job, bool stopScripMax = false)
        {
            if ((!HWDSupply.Instance.IsOpen && CraftInspectNpc == null) || CraftInspectNpc.Location.Distance(Core.Me.Location) > 5f)
            {
                Log.Information("Collectable Appraiser NPC not nearby.  Moving to correct area first.");
                await Navigation.GetTo(FirmamentZoneId, new Vector3(43.59162f, -16f, 170.3864f));
            }

            if (!HWDSupply.Instance.IsOpen && CraftInspectNpc.Location.Distance(Core.Me.Location) > InteractMaxRange)
            {
                Log.Information("Approaching interaction range of Collectable Appraiser NPC.");
                await Navigation.OffMeshMove(CraftInspectNpc.Location);
                await Coroutine.Sleep(500);
            }

            if (!HWDSupply.Instance.IsOpen)
            {
                Log.Information($"Interacting with Collectable Appraiser NPC: {CraftInspectNpc.Name} ({CraftInspectNpc.NpcId}).");
                CraftInspectNpc.Interact();
                await Coroutine.Wait(5000, () => HWDSupply.Instance.IsOpen || Talk.DialogOpen);
                await Coroutine.Sleep(1000);

                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(5000, () => !Talk.DialogOpen);
                }

                Log.Verbose("Talking done.");

                await Coroutine.Sleep(1000);
                Log.Information("Collectable Appraiser should be ready.");
            }

            if (HWDSupply.Instance.IsOpen)
            {
                Log.Information("Collectable Appraiser is ready.");

                if (HWDSupply.Instance.ClassSelected != job)
                {
                    HWDSupply.Instance.ClassSelected = job;
                    await Coroutine.Sleep(1000);
                }

                Log.Information($"Appraising Skybuilders' collectables for crafting job {job}.");
                var items = InventoryManager.FilledSlots.Where(i => i.RawItemId == itemId);
                foreach (var item in items)
                {
                    Log.Debug($"Appraising collectable: {item.Name} ({item.RawItemId})");
                    HWDSupply.Instance.ClickItem(index);

                    await Coroutine.Wait(5000, () => Request.IsOpen);
                    await Coroutine.Sleep(700);
                    item.Handover();
                    await Coroutine.Sleep(100);
                    await Coroutine.Wait(5000, () => Request.HandOverButtonClickable);
                    Request.HandOver();

                    if (ScriptConditions.Helpers.GetSkybuilderScrips() > 9000)
                    {
                        await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
                    }
                    else
                    {
                        await Coroutine.Wait(5000, () => !Request.IsOpen);
                        await Coroutine.Sleep(300);
                    }

                    if (Translator.Language != Language.Chn)
                    {
                        Log.Information($"Kupo Vouchers: {HWDSupply.Instance.GetKupoVoucherCount()}");

                        if (HWDSupply.Instance.GetKupoVoucherCount() >= 9)
                        {
                            Log.Information($"Going to turn in Kupo Vouchers: {HWDSupply.Instance.GetKupoVoucherCount()}");
                            if (SelectYesno.IsOpen)
                            {
                                SelectYesno.Yes();
                                await Coroutine.Sleep(1000);
                            }

                            HWDSupply.Instance.Close();
                            await Coroutine.Sleep(3000);
                            await HandInKupoVoucher(1);
                            break;
                        }
                    }

                    if (!SelectYesno.IsOpen)
                    {
                        continue; // No scrip overcap warning prompt yet
                    }

                    if (stopScripMax)
                    {
                        Log.Warning($"Have {ScriptConditions.Helpers.GetSkybuilderScrips():N}/{SkybuildersScripMax:N} Skybuilders' Scrips and will overcap.  Stopping here.");
                        if (SelectYesno.IsOpen)
                        {
                            SelectYesno.No();
                        }

                        await Coroutine.Sleep(500);
                        if (Request.IsOpen)
                        {
                            Request.Cancel();
                            await Coroutine.Sleep(500);
                        }

                        if (HWDSupply.Instance.IsOpen)
                        {
                            HWDSupply.Instance.Close();
                            await Coroutine.Wait(3000, () => !HWDSupply.Instance.IsOpen);
                        }

                        return false;
                    }

                    else
                    {
                        Log.Warning($"Have {ScriptConditions.Helpers.GetSkybuilderScrips():N}/{SkybuildersScripMax:N} Skybuilders' Scrips and will overcap, but force-inspecting anyway.");
                        SelectYesno.Yes();
                    }

                    await Coroutine.Sleep(1000);

                    if (!InventoryManager.FilledSlots.Any(i => i.RawItemId == itemId))
                    {
                        if (Request.IsOpen)
                        {
                            Request.Cancel();
                            await Coroutine.Sleep(500);
                        }

                        if (HWDSupply.Instance.IsOpen)
                        {
                            HWDSupply.Instance.Close();
                            await Coroutine.Wait(2000, () => !HWDSupply.Instance.IsOpen);
                        }

                        break;
                    }
                }
            }

            if (Request.IsOpen)
            {
                Request.Cancel();
                await Coroutine.Wait(3000, () => !Request.IsOpen);

                if (HWDSupply.Instance.IsOpen)
                {
                    HWDSupply.Instance.Close();
                    await Coroutine.Wait(3000, () => !HWDSupply.Instance.IsOpen);
                }
            }

            // TODO: Is this needed?  We already iterated all bagslots with itemId, so how is anything left?
            if (InventoryManager.FilledSlots.Any(i => i.RawItemId == itemId))
            {
                await HandInItem(itemId, index, job, stopScripMax);
            }

            return false;
        }

        /// <summary>
        /// Travels to the Firmament and buys the specified item with Skybuilders' Scrips.
        /// </summary>
        /// <param name="itemId">ItemID of item to be purchased.</param>
        /// <param name="maxCount">Max quantity to buy, limited by scrip balance and stack size. -1 buys as much as possible.</param>
        /// <param name="SelectStringLine">Zero-based index of row for shop category.</param>
        /// <returns><see langword="false"/> if execution should continue from here.</returns>
        public static async Task<bool> BuyScripItem(uint itemId, int maxCount = -1, int SelectStringLine = 0)
        {
            if ((!ShopExchangeCurrency.Open && ScripAndFeteVendorNpc == null) || ScripAndFeteVendorNpc.Location.Distance(Core.Me.Location) > 5f)
            {
                await Navigation.GetTo(FirmamentZoneId, new Vector3(36.33978f, -16f, 145.3877f));
            }

            if (!ShopExchangeCurrency.Open && ScripAndFeteVendorNpc.Location.Distance(Core.Me.Location) > InteractMaxRange)
            {
                await Navigation.OffMeshMove(ScripAndFeteVendorNpc.Location);
                await Coroutine.Sleep(500);
            }

            if (!ShopExchangeCurrency.Open)
            {
                ScripAndFeteVendorNpc.Interact();
                await Coroutine.Wait(5000, () => ShopExchangeCurrency.Open || Talk.DialogOpen || Conversation.IsOpen);
                if (Conversation.IsOpen)
                {
                    Conversation.SelectLine((uint) SelectStringLine);
                    await Coroutine.Wait(5000, () => ShopExchangeCurrency.Open);
                }
            }

            if (ShopExchangeCurrency.Open)
            {
                var items = SpecialShopManager.Items;
                var specialShopItem = items?.Cast<SpecialShopItem?>().FirstOrDefault(i => i.HasValue && i.Value.ItemIds.Contains(itemId));

                if (!specialShopItem.HasValue)
                {
                    return false;
                }

                var count = Math.Min(MaxAffordableViaScrips(specialShopItem.Value), maxCount);

                if (count > 0)
                {
                    ScripPurchase(itemId, (uint) count);
                }

                await Coroutine.Wait(5000, () => SelectYesno.IsOpen);

                SelectYesno.ClickYes();

                await Coroutine.Wait(500, () => !SelectYesno.IsOpen);
                await Coroutine.Wait(500, () => SelectYesno.IsOpen);
                if (SelectYesno.IsOpen)
                {
                    SelectYesno.ClickYes();
                    await Coroutine.Wait(500, () => !SelectYesno.IsOpen);
                }

                await Coroutine.Sleep(1000);

                ShopExchangeCurrency.Close();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to buy the specified item with Skybuilders' Scrips, up to the requested quantity.
        /// </summary>
        /// <param name="itemId">ItemID of item to buy.</param>
        /// <param name="quantity">Amount to buy. May be reduced by stack size or scrip balance.</param>
        /// <returns>Quantity actually purchased.</returns>
        internal static uint ScripPurchase(uint itemId, uint quantity)
        {
            var windowByName = RaptureAtkUnitManager.GetWindowByName("ShopExchangeCurrency");
            if (windowByName == null)
            {
                return 0u;
            }

            var specialShopItem = SpecialShopManager.Items?.Cast<SpecialShopItem?>().FirstOrDefault(i => i.HasValue && i.Value.ItemIds.Contains(itemId));

            if (!specialShopItem.HasValue)
            {
                return 0u;
            }

            var index = SpecialShopManager.Items.IndexOf(specialShopItem.Value);

            // Clamp purchaseQuantity to max stack size and most we can afford to buy
            quantity = Math.Min(quantity, specialShopItem.Value.Item0.StackSize);
            quantity = Math.Min(quantity, MaxAffordableViaScrips(specialShopItem.Value));

            var args = new ulong[]
            {
                3uL,
                0uL,
                3uL,
                (uint) index,
                3uL,
                quantity,
                0uL,
                0uL,
            };
            windowByName.SendAction(args.Length / 2, args);

            return quantity;
        }

        /// <summary>
        /// Determines how many of the specified item can be purchased with current Skybuilders' Scrip balance.
        /// </summary>
        /// <param name="item">Item to measure against.</param>
        /// <returns>Quantity able to be purchased.</returns>
        private static uint MaxAffordableViaScrips(SpecialShopItem item)
        {
            var currentScrips = (uint) ScriptConditions.Helpers.GetSkybuilderScrips();

            return currentScrips / item.CurrencyCosts[0];
        }
    }
}