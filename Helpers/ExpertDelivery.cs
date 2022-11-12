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
    public static class ExpertDelivery
    {
        private static readonly LLogger Log = new(nameof(ExpertDelivery), Colors.DarkKhaki);

        public static Dictionary<GrandCompany, Npc> PersonnelOfficers = new()
        {
            { GrandCompany.Order_Of_The_Twin_Adder, new Npc(1002394, 132, new Vector3(-68.34107f, -0.5017813f, -7.787445f)) }, //serpent personnel officer New Gridania - Adders' Nest
            { GrandCompany.Maelstrom, new Npc(1002388, 128, new Vector3(93.70313f, 40.27537f, 74.40751f)) }, //storm personnel officer Limsa Lominsa Upper Decks - Maelstrom Command
            { GrandCompany.Immortal_Flames, new Npc(1002391, 130, new Vector3(-142.8766f, 4.099999f, -106.1056f)) }, //flame personnel officer Ul'dah - Steps of Nald - Hall of Flames
        };

        public static async Task<DeliveryStatus> DeliverItems(uint itemId)
        {
            return await DeliverItems(InventoryManager.FilledSlots.Where(i => i.RawItemId == itemId));
        }

        public static async Task<DeliveryStatus> DeliverItems(IEnumerable<uint> itemIds)
        {
            return await DeliverItems(InventoryManager.FilledSlots.Where(i => itemIds.Contains(i.RawItemId)));
        }

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

            return GrandCompanySupplyList.Instance.IsOpen;
        }

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

    public enum DeliveryStatus
    {
        Success,
        BagSlotError,
        WindowError,
        OtherError,
        MaxSeals,
        WrongItem
    }
}