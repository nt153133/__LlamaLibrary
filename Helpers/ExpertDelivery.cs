using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Automates the Grand Company Expert Delivery process: navigating to the Personnel Officer,
    /// opening the supply window, delivering items for GC seals, and closing the window.
    /// Deliveries are rejected if they would push the player's seal count over the cap.
    /// </summary>
    public static class ExpertDelivery
    {
        private static readonly LLogger Log = new(nameof(ExpertDelivery), Colors.DarkKhaki);

        /// <summary>
        /// Maps each Grand Company to the Personnel Officer NPC that handles Expert Delivery.
        /// </summary>
        public static Dictionary<GrandCompany, Npc> PersonnelOfficers = new()
        {
            { GrandCompany.Order_Of_The_Twin_Adder, new Npc(1002394, 132, new Vector3(-68.34107f, -0.5017813f, -7.787445f)) }, //serpent personnel officer New Gridania - Adders' Nest
            { GrandCompany.Maelstrom, new Npc(1002388, 128, new Vector3(93.70313f, 40.27537f, 74.40751f)) }, //storm personnel officer Limsa Lominsa Upper Decks - Maelstrom Command
            { GrandCompany.Immortal_Flames, new Npc(1002391, 130, new Vector3(-142.8766f, 4.099999f, -106.1056f)) }, //flame personnel officer Ul'dah - Steps of Nald - Hall of Flames
        };

        /// <summary>
        /// Delivers all inventory slots whose <see cref="BagSlot.RawItemId"/> matches <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">The item ID to deliver.</param>
        /// <returns>A <see cref="DeliveryStatus"/> indicating the outcome.</returns>
        public static async Task<DeliveryStatus> DeliverItems(uint itemId)
        {
            return await DeliverItems(InventoryManager.FilledSlots.Where(i => i.RawItemId == itemId));
        }

        /// <summary>
        /// Delivers all inventory slots whose <see cref="BagSlot.RawItemId"/> is contained in <paramref name="itemIds"/>.
        /// Returns <see cref="DeliveryStatus.Success"/> immediately if no matching slots are found.
        /// </summary>
        /// <param name="itemIds">Collection of item IDs to deliver.</param>
        /// <returns>A <see cref="DeliveryStatus"/> indicating the outcome.</returns>
        public static async Task<DeliveryStatus> DeliverItems(IEnumerable<uint> itemIds)
        {
            var slots = InventoryManager.FilledSlots.Where(i => itemIds.Contains(i.RawItemId)).ToList();
            if (slots.Count == 0)
            {
                return DeliveryStatus.Success;
            }

            return await DeliverItems(slots);
        }

        /// <summary>
        /// Delivers each bag slot in <paramref name="bagSlots"/> via the Expert Delivery window.
        /// Opens the window if it is not already open, and closes it when finished.
        /// </summary>
        /// <param name="bagSlots">The bag slots to deliver.</param>
        /// <returns>A <see cref="DeliveryStatus"/> indicating the outcome.</returns>
        public static async Task<DeliveryStatus> DeliverItems(IEnumerable<BagSlot> bagSlots)
        {
            if (!await MakeSureWindowOpen())
            {
                return DeliveryStatus.WindowError;
            }

            foreach (var bagSlot in bagSlots)
            {
                var result = await DeliverItem(bagSlot, false);
                if (result != DeliveryStatus.Success)
                {
                    await CloseWindow();
                    return result;
                }
            }

            return await CloseWindow() ? DeliveryStatus.Success : DeliveryStatus.WindowError;
        }

        /// <summary>
        /// Delivers a single bag slot item via Expert Delivery.
        /// Validates that the slot has no materia, ensures the window is open and set to Expert mode,
        /// confirms the reward matches the expected seal count, and optionally closes the window.
        /// Returns <see cref="DeliveryStatus.MaxSeals"/> without delivering if the reward would exceed the seal cap.
        /// </summary>
        /// <param name="bagSlot">The bag slot to deliver.</param>
        /// <param name="closeWindow">
        /// When <c>true</c> (default), the supply window is closed after a successful delivery.
        /// Pass <c>false</c> when delivering multiple items in sequence to leave the window open.
        /// </param>
        /// <returns>A <see cref="DeliveryStatus"/> indicating the outcome.</returns>
        public static async Task<DeliveryStatus> DeliverItem(BagSlot bagSlot, bool closeWindow = true)
        {
            //Sanity check on bagslot
            if (!bagSlot.IsValid || bagSlot.MateriaCount() > 0)
            {
                Log.Debug("Item is invalid or has materia");
                return DeliveryStatus.BagSlotError;
            }

            //Make sure window is open
            if (!GrandCompanySupplyList.Instance.IsOpen)
            {
                Log.Debug("Window is not open");
                if (!await MakeSureWindowOpen())
                {
                    return DeliveryStatus.WindowError;
                }
            }

            //Make sure on Expert Delivery
            if (AgentGrandCompanySupply.Instance.HandinType != GCSupplyType.Expert)
            {
                Log.Debug("Switching to Expert Delivery");
                await GrandCompanySupplyList.Instance.SwitchToExpertDelivery();
                await Coroutine.Sleep(500);
            }

            //Check Filter
            if (AgentGrandCompanySupply.Instance.ExpertFilter != GCFilter.HideArmory)
            {
                Log.Debug($"Setting filter to hide armory it's currently {AgentGrandCompanySupply.Instance.ExpertFilter}");
                GrandCompanySupplyList.Instance.SetExpertFilter((byte)GCFilter.HideArmory);
                await Coroutine.Wait(2000, () => AgentGrandCompanySupply.Instance.ExpertFilter == GCFilter.HideArmory);
            }

            //Get them items
            var items = AgentGrandCompanySupply.Instance.ExpertSupplyItems;

            if (items.Length < 1)
            {
                Log.Error("Item array is empty, bad sign.");
                return DeliveryStatus.OtherError;
            }

            GCSupplyItem supplyItem = default;
            var itemIndex = -1;

            //Search for item in the list
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                if (item.BagSlot.Pointer == bagSlot.Pointer)
                {
                    itemIndex = index;
                    supplyItem = item;
                    break;
                }
            }

            //Didn't find it
            if (itemIndex == -1)
            {
                Log.Error("Couldn't find bagslot in list");
                return DeliveryStatus.BagSlotError;
            }

            if (Core.Me.GCSeals() + supplyItem.Seals >= Core.Me.MaxGCSeals())
            {
                Log.Error("Item would put seals over max");
                return DeliveryStatus.MaxSeals;
            }

            //Save current item list count
            var oldCount = AgentGrandCompanySupply.Instance.SupplyItemCount;

            var resultString = $"{bagSlot.Item.LocaleName()} for {supplyItem.Seals} seals";
            GrandCompanySupplyList.Instance.ClickItem(itemIndex);

            if (supplyItem.IsHQ)
            {
                Log.Debug("Waiting for select yes/no");
                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                {
                    SelectYesno.Yes();
                }
            }

            if (!await GrandCompanySupplyReward.Instance.WaitTillWindowOpen(10000))
            {
                Log.Error("Reward window did not show");
                return DeliveryStatus.WindowError;
            }

            if (GrandCompanySupplyReward.Instance.SealReward != supplyItem.Seals)
            {
                Log.Error("Seals don't match so something went wrong");
                GrandCompanySupplyReward.Instance.Close();
                return DeliveryStatus.WrongItem;
            }

            GrandCompanySupplyReward.Instance.Confirm();

            Log.Information($"Delivered {resultString}");

            if (!await GrandCompanySupplyList.Instance.WaitTillWindowOpen(10000))
            {
                return DeliveryStatus.WindowError;
            }

            if (closeWindow)
            {
                return await CloseWindow() ? DeliveryStatus.Success : DeliveryStatus.WindowError;
            }

            return await Coroutine.Wait(5000, () => AgentGrandCompanySupply.Instance.SupplyItemCount < oldCount) ? DeliveryStatus.Success : DeliveryStatus.OtherError;
        }

        /// <summary>
        /// Navigates to the appropriate Personnel Officer and opens the GC Supply window, switching
        /// it to Expert Delivery mode. Returns <c>true</c> if the window is open on return.
        /// </summary>
        /// <returns><c>true</c> when the <see cref="GrandCompanySupplyList"/> window is open; otherwise <c>false</c>.</returns>
        public static async Task<bool> MakeSureWindowOpen()
        {
            if (GrandCompanySupplyList.Instance.IsOpen)
            {
                return true;
            }

            //await GrandCompanyHelper.InteractWithNpc(GCNpc.Personnel_Officer);
            if (!await Navigation.GetToInteractNpcSelectString(PersonnelOfficers[Core.Me.GrandCompany], 0))
            {
                if (!SelectString.IsOpen)
                {
                    Log.Information("Window did not open trying again");
                    await GrandCompanyHelper.InteractWithNpc(GCNpc.Personnel_Officer);
                    await Coroutine.Wait(10000, () => SelectString.IsOpen);

                    if (!SelectString.IsOpen)
                    {
                        Log.Error("Window is not open...maybe it didn't get to npc?");
                        return false;
                    }
                }

                SelectString.ClickSlot(0);
                await Coroutine.Wait(10000, () => !SelectString.IsOpen);
            }

            if (!await GrandCompanySupplyList.Instance.WaitTillWindowOpen())
            {
                Log.Information("Window is not open...maybe it didn't get to npc?");
                return false;
            }

            await GrandCompanySupplyList.Instance.SwitchToExpertDelivery();
            await Coroutine.Sleep(250);
            return GrandCompanySupplyList.Instance.IsOpen;
        }

        /// <summary>
        /// Closes the GC Supply window and dismisses any lingering conversation dialogue.
        /// Returns <c>true</c> if the window is confirmed closed.
        /// </summary>
        /// <returns><c>true</c> when the window is no longer open; otherwise <c>false</c>.</returns>
        public static async Task<bool> CloseWindow()
        {
            if (!GrandCompanySupplyList.Instance.IsOpen)
            {
                return true;
            }

            GrandCompanySupplyList.Instance.Close();
            await Coroutine.Wait(5000, () => !GrandCompanySupplyList.Instance.IsOpen);

            await Coroutine.Wait(5000, () => SelectString.IsOpen);
            if (Conversation.IsOpen)
            {
                Conversation.SelectQuit();
                await Coroutine.Wait(5000, () => !SelectString.IsOpen);
                await Coroutine.Wait(5000, () => !MovementManager.IsOccupied);
            }

            return !GrandCompanySupplyList.Instance.IsOpen;
        }
    }

    /// <summary>
    /// Indicates the result of an Expert Delivery attempt.
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>The item was delivered successfully.</summary>
        Success,
        /// <summary>The bag slot was invalid (empty, missing, or contains materia).</summary>
        BagSlotError,
        /// <summary>A required UI window could not be opened or closed.</summary>
        WindowError,
        /// <summary>An unspecified error occurred (e.g., the supply item list was empty).</summary>
        OtherError,
        /// <summary>Delivering the item would exceed the player's maximum GC seal capacity.</summary>
        MaxSeals,
        /// <summary>The reward window showed a different item than expected.</summary>
        WrongItem
    }
}