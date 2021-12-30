using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using Character = LlamaLibrary.RemoteWindows.Character;

namespace LlamaLibrary.Helpers
{
    public static class GeneralFunctions
    {
        private static readonly string Name = "GeneralFunctions";
        private static readonly Color LogColor = Colors.Aquamarine;
        private static readonly LLogger Log = new LLogger(Name, LogColor);

        public static readonly InventoryBagId[] MainBags = { InventoryBagId.Bag1, InventoryBagId.Bag2, InventoryBagId.Bag3, InventoryBagId.Bag4 };

        public static readonly InventoryBagId[] SaddlebagIds =
        {
            (InventoryBagId)0xFA0, (InventoryBagId)0xFA1//, (InventoryBagId) 0x1004,(InventoryBagId) 0x1005
        };

        public static IEnumerable<BagSlot> MainBagsFilledSlots()
        {
            return InventoryManager.GetBagsByInventoryBagId(MainBags).SelectMany(x => x.FilledSlots);
        }

        private static bool IsJumping => Core.Memory.NoCacheRead<byte>(Offsets.Conditions + Offsets.JumpingCondition) != 0;

        private static bool CheckIfBusy(bool leaveDuty, bool stopFishing, bool dismount)
        {
            if (stopFishing && FishingManager.State != FishingState.None)
            {
                return true;
            }

            if (leaveDuty && DutyManager.InInstance)
            {
                return true;
            }

            if (dismount && Core.Me.IsMounted)
            {
                return true;
            }

            if (CraftingLog.IsOpen)
            {
                return true;
            }

            if (CraftingManager.IsCrafting)
            {
                return true;
            }

            if (MovementManager.IsOccupied)
            {
                return true;
            }

            if (InSmallTalk)
            {
                return true;
            }

            if (Core.Me.HasTarget && Core.Me.CurrentTargetObjId != Core.Me.ObjectId)
            {
                return true;
            }

            return false;
        }

        public static async Task StopBusy(bool leaveDuty = true, bool stopFishing = true, bool dismount = true)
        {
            for (var tryStep = 1; tryStep < 6; tryStep++)
            {
                if (!CheckIfBusy(leaveDuty, stopFishing, dismount))
                {
                    break;
                }

                Log.Information($"We're occupied. Trying to exit out. Attempt #{tryStep}");

                if (stopFishing && FishingManager.State != FishingState.None)
                {
                    var quit = ActionManager.CurrentActions.Values.FirstOrDefault(i => i.Id == 299);
                    if (quit != default(SpellData))
                    {
                        Log.Information($"Exiting Fishing.");
                        if (ActionManager.CanCast(quit, Core.Me))
                        {
                            ActionManager.DoAction(quit, Core.Me);
                            await Coroutine.Wait(6000, () => FishingManager.State == FishingState.None);
                        }
                    }
                }

                if (CraftingLog.IsOpen || CraftingManager.IsCrafting || Synthesis.IsOpen)
                {
                    Log.Information($"Closing Crafting Window.");
                    await Lisbeth.ExitCrafting();
                    Synthesis.Close();
                    await Coroutine.Wait(6000, () => !Synthesis.IsOpen);
                    await Coroutine.Wait(1500, () => CraftingLog.IsOpen);
                    CraftingLog.Close();
                    await Coroutine.Wait(6000, () => !CraftingLog.IsOpen);
                    await Coroutine.Wait(6000, () => !CraftingManager.IsCrafting && !MovementManager.IsOccupied);
                }

                if (leaveDuty && DutyManager.InInstance)
                {
                    Log.Information($"Leaving Diadem.");
                    DutyManager.LeaveActiveDuty();

                    if (await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
                    {
                        await Coroutine.Yield();
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                        await Coroutine.Sleep(5000);
                    }
                }

                if (dismount && Core.Me.IsMounted)
                {
                    Log.Information("Dismounting.");
                    ActionManager.Dismount();
                    await Coroutine.Wait(3000, () => !Core.Me.IsMounted);
                }

                if (InSmallTalk)
                {
                    Log.Information("Skipping smalltalk.");
                    await SmallTalk();
                }

                if (Core.Me.HasTarget)
                {
                    Log.Information($"Untargeting {Core.Me.CurrentTarget.Name}");
                    Core.Me.ClearTarget();
                    await Coroutine.Wait(1500, () => !Core.Me.HasTarget);
                    if (Core.Me.HasTarget)
                    {
                        Log.Warning("Couldn't untarget NPC. Trying to target ourselves...");
                        Core.Me.Target();
                        await Coroutine.Wait(1500, () => Core.Me.CurrentTargetObjId == Core.Me.ObjectId);
                    }
                }

                await Coroutine.Wait(2500, () => !CheckIfBusy(leaveDuty, stopFishing, dismount));
                await Coroutine.Sleep(500);
            }

            if (CheckIfBusy(leaveDuty, stopFishing, dismount))
            {
                Log.Error("Something went wrong, we're still occupied.");
                TreeRoot.Stop("Stopping bot.");
            }
        }

        private static bool InSmallTalk => SelectYesno.IsOpen || SelectString.IsOpen || SelectIconString.IsOpen || Talk.DialogOpen || JournalAccept.IsOpen || QuestLogManager.InCutscene || CommonBehaviors.IsLoading;

        public static async Task SmallTalk(int waitTime = 500)
        {
            await Coroutine.Wait(waitTime, () => InSmallTalk);

            while (InSmallTalk)
            {
                await Coroutine.Yield();

                if (CommonBehaviors.IsLoading)
                {
                    await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                }

                if (SelectYesno.IsOpen)
                {
                    SelectYesno.ClickNo();
                }

                if (SelectString.IsOpen)
                {
                    if (!await WindowEscapeSpam("SelectString"))
                    {
                        if (SelectString.Lines().Contains("Cancel"))
                        {
                            SelectString.ClickLineContains("Cancel");
                        }
                        else if (SelectString.Lines().Contains("Quit"))
                        {
                            SelectString.ClickLineContains("Quit");
                        }
                        else if (SelectString.Lines().Contains("Exit"))
                        {
                            SelectString.ClickLineContains("Exit");
                        }
                        else if (SelectString.Lines().Contains("Nothing"))
                        {
                            SelectString.ClickLineContains("Nothing");
                        }
                        else
                        {
                            SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                        }
                    }
                }

                if (SelectIconString.IsOpen)
                {
                    if (!await WindowEscapeSpam("SelectIconString"))
                    {
                        if (SelectIconString.Lines().Contains("Cancel"))
                        {
                            SelectString.ClickLineContains("Cancel");
                        }
                        else if (SelectIconString.Lines().Contains("Quit"))
                        {
                            SelectString.ClickLineContains("Quit");
                        }
                        else if (SelectIconString.Lines().Contains("Exit"))
                        {
                            SelectString.ClickLineContains("Exit");
                        }
                        else if (SelectIconString.Lines().Contains("Nothing"))
                        {
                            SelectString.ClickLineContains("Nothing");
                        }
                        else
                        {
                            SelectIconString.ClickSlot((uint)(SelectIconString.LineCount - 1));
                        }
                    }
                }

                while (QuestLogManager.InCutscene)
                {
                    AgentCutScene.Instance.PromptSkip();
                    if (AgentCutScene.Instance.CanSkip && SelectString.IsOpen)
                    {
                        SelectString.ClickSlot(0);
                    }

                    await Coroutine.Yield();
                }

                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(100, () => !Talk.DialogOpen);
                    await Coroutine.Wait(100, () => Talk.DialogOpen);
                    await Coroutine.Yield();
                }

                if (JournalAccept.IsOpen)
                {
                    JournalAccept.Decline();
                }

                await Coroutine.Wait(500, () => InSmallTalk);
            }
        }

        private static async Task<bool> WindowEscapeSpam(string windowName)
        {
            for (var i = 0; i < 5 && RaptureAtkUnitManager.GetWindowByName(windowName) != null; i++)
            {
                RaptureAtkUnitManager.Update();

                if (RaptureAtkUnitManager.GetWindowByName(windowName) != null)
                {
                    RaptureAtkUnitManager.GetWindowByName(windowName).SendAction(1, 3UL, uint.MaxValue);
                }

                await Coroutine.Wait(300, () => RaptureAtkUnitManager.GetWindowByName(windowName) == null);
                await Coroutine.Wait(300, () => RaptureAtkUnitManager.GetWindowByName(windowName) != null);
                await Coroutine.Yield();
            }

            return RaptureAtkUnitManager.GetWindowByName(windowName) == null;
        }

        public static async Task InventoryEquipBest(bool updateGearSet = true, bool useRecommendEquip = true)
        {
            await StopBusy(leaveDuty: false, dismount: false);
            if (!Character.Instance.IsOpen)
            {
                AgentCharacter.Instance.Toggle();
                await Coroutine.Wait(5000, () => Character.Instance.IsOpen);
            }

            var armoryCount = 0;

            if (useRecommendEquip)
            {
                foreach (var bagSlot in InventoryManager.EquippedItems)
                {
                    if (!bagSlot.IsValid)
                    {
                        continue;
                    }

                    if (bagSlot.Slot == 0 && !bagSlot.IsFilled)
                    {
                        Log.Error("MainHand slot isn't filled. How?");
                        continue;
                    }

                    var itemWeight = bagSlot.IsFilled ? ItemWeight.GetItemWeight(bagSlot.Item) : -1;

                    var betterItem = InventoryManager.FilledArmorySlots
                        .Where(bs =>
                                   GetEquipUiCategory(bagSlot.Slot).Contains(bs.Item.EquipmentCatagory) &&
                                   bs.Item.IsValidForCurrentClass &&
                                   bs.Item.RequiredLevel <= Core.Me.ClassLevel &&
                                   bs.BagId != InventoryBagId.EquippedItems)
                        .OrderByDescending(r => ItemWeight.GetItemWeight(r.Item))
                        .FirstOrDefault();

                    if (betterItem == null || !betterItem.IsValid || !betterItem.IsFilled || betterItem == bagSlot || itemWeight >= ItemWeight.GetItemWeight(betterItem.Item))
                    {
                        continue;
                    }

                    armoryCount++;
                }

                if (armoryCount > 1)
                {
                    if (!RecommendEquip.Instance.IsOpen)
                    {
                        AgentRecommendEquip.Instance.Toggle();
                    }

                    await Coroutine.Wait(3500, () => RecommendEquip.Instance.IsOpen);
                    RecommendEquip.Instance.Confirm();
                    await Coroutine.Sleep(800);
                }
            }

            foreach (var bagSlot in InventoryManager.EquippedItems)
            {
                if (!bagSlot.IsValid)
                {
                    continue;
                }

                if (bagSlot.Slot == 0 && !bagSlot.IsFilled)
                {
                    Log.Error("MainHand slot isn't filled. How?");
                    continue;
                }

                var itemWeight = bagSlot.IsFilled ? ItemWeight.GetItemWeight(bagSlot.Item) : -1;

                var betterItem = InventoryManager.FilledInventoryAndArmory
                    .Where(bs =>
                               GetEquipUiCategory(bagSlot.Slot).Contains(bs.Item.EquipmentCatagory) &&
                               bs.Item.IsValidForCurrentClass &&
                               bs.Item.RequiredLevel <= Core.Me.ClassLevel &&
                               bs.BagId != InventoryBagId.EquippedItems)
                    .OrderByDescending(r => ItemWeight.GetItemWeight(r.Item))
                    .FirstOrDefault();

                /*Log.Information($"# of Candidates: {betterItemCount}");
                if (betterItem != null)
                {
                    Log.Information($"{betterItem.Name}");
                }
                else
                {
                    Log.Warning("betterItem was null.");
                }*/

                if (betterItem == null || !betterItem.IsValid || !betterItem.IsFilled || betterItem == bagSlot || itemWeight >= ItemWeight.GetItemWeight(betterItem.Item))
                {
                    continue;
                }

                Log.Information(bagSlot.IsFilled ? $"Equipping {betterItem.Name} over {bagSlot.Name}." : $"Equipping {betterItem.Name}.");
                var currentItem = bagSlot.Item;
                betterItem.Move(bagSlot);
                await Coroutine.Wait(3000, () => bagSlot.Item != currentItem);
                if (bagSlot.Item == currentItem)
                {
                    Log.Error("Something went wrong. Item remained unchanged.");
                    continue;
                }

                await Coroutine.Sleep(500);
            }

            if (useRecommendEquip)
            {
                if (!RecommendEquip.Instance.IsOpen)
                {
                    AgentRecommendEquip.Instance.Toggle();
                }

                await Coroutine.Wait(3500, () => RecommendEquip.Instance.IsOpen);
                RecommendEquip.Instance.Confirm();
                await Coroutine.Sleep(800);
            }

            if (updateGearSet)
            {
                await UpdateGearSet();
            }

            Character.Instance.Close();
        }

        public static async Task<bool> UpdateGearSet()
        {
            if (!Character.Instance.IsOpen)
            {
                AgentCharacter.Instance.Toggle();
                await Coroutine.Wait(10000, () => Character.Instance.IsOpen);
                if (!Character.Instance.IsOpen)
                {
                    Log.Error("Character window didn't open.");
                    return false;
                }
            }

            if (!Character.Instance.IsOpen)
            {
                return false;
            }

            if (!await Coroutine.Wait(1200, () => Character.Instance.CanUpdateGearSet()))
            {
                Character.Instance.Close();
                return false;
            }

            Character.Instance.UpdateGearSet();

            if (await Coroutine.Wait(1500, () => SelectYesno.IsOpen))
            {
                SelectYesno.Yes();
            }
            else
            {
                if (Character.Instance.IsOpen)
                {
                    Character.Instance.Close();
                }

                return true;
            }

            await Coroutine.Wait(10000, () => !SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                return true;
            }

            if (Character.Instance.IsOpen)
            {
                Character.Instance.Close();
            }

            return true;
        }

        private static List<ItemUiCategory> GetEquipUiCategory(ushort slotId)
        {
            switch (slotId)
            {
                case 0:
                    return ItemWeight.MainHands;
                case 1:
                    return ItemWeight.OffHands;
                case 2:
                    return new List<ItemUiCategory> { ItemUiCategory.Head };
                case 3:
                    return new List<ItemUiCategory> { ItemUiCategory.Body };
                case 4:
                    return new List<ItemUiCategory> { ItemUiCategory.Hands };
                case 5:
                    return new List<ItemUiCategory> { ItemUiCategory.Waist };
                case 6:
                    return new List<ItemUiCategory> { ItemUiCategory.Legs };
                case 7:
                    return new List<ItemUiCategory> { ItemUiCategory.Feet };
                case 8:
                    return new List<ItemUiCategory> { ItemUiCategory.Earrings };
                case 9:
                    return new List<ItemUiCategory> { ItemUiCategory.Necklace };
                case 10:
                    return new List<ItemUiCategory> { ItemUiCategory.Bracelets };
                case 11:
                case 12:
                    return new List<ItemUiCategory> { ItemUiCategory.Ring };
                case 13:
                    return new List<ItemUiCategory> { ItemUiCategory.Soul_Crystal };
                default:
                    return null;
            }
        }

        public static IEnumerable<BagSlot> NonGearSetItems()
        {
            return InventoryManager.FilledArmorySlots.Where(bs => !GearsetManager.GearSets.SelectMany(gs => gs.Gear).Select(g => g.Item).Contains(bs.Item));
        }

        public static async Task RetainerSellItems(IEnumerable<BagSlot> items)
        {
            if (await HelperFunctions.GetNumberOfRetainers() == 0)
            {
                Log.Error("No retainers found to sell items to.");
                return;
            }

            var bagSlots = items.ToList();
            if (!bagSlots.Any())
            {
                Log.Information("No items found to sell.");
                return;
            }

            await StopBusy();
            if (!await HelperFunctions.UseSummoningBell())
            {
                Log.Error("Couldn't get to summoning bell.");
                return;
            }

            await RetainerRoutine.SelectRetainer(0);
            RetainerTasks.OpenInventory();
            if (!await Coroutine.Wait(3000, RetainerTasks.IsInventoryOpen))
            {
                Log.Information("Couldn't get Retainer inventory open.");
                RetainerTasks.CloseInventory();
                await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);
                RetainerTasks.CloseTasks();
                await Coroutine.Wait(3000, () => Talk.DialogOpen);
                if (Talk.DialogOpen)
                {
                    Talk.Next();
                }

                await Coroutine.Wait(3000, () => RetainerList.Instance.IsOpen);
                await RetainerRoutine.CloseRetainers();
                return;
            }

            var itemCount = bagSlots.Count;
            var i = 1;
            foreach (var bagSlot in bagSlots)
            {
                if (!bagSlot.IsValid || !bagSlot.IsFilled)
                {
                    Log.Information("BagSlot isn't valid or filled.");
                    i++;
                    continue;
                }

                var name = bagSlot.Name;
                Log.Information($"Attempting to sell #{i++} of {itemCount}: {name}");
                var waitTime = 600;

                bagSlot.RetainerSellItem();

                if (await Coroutine.Wait(500, () => SelectYesno.IsOpen))
                {
                    SelectYesno.ClickYes();
                }
                else
                {
                    waitTime -= 500;
                }

                if (!await Coroutine.Wait(5000, () => !bagSlot.IsValid || !bagSlot.IsFilled))
                {
                    Log.Error($"We couldn't sell {name}.");
                }
                else
                {
                    Log.Information($"Sold {name}.");
                }

                await Coroutine.Sleep(waitTime);
            }

            RetainerTasks.CloseInventory();
            await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);
            RetainerTasks.CloseTasks();
            await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
            SelectYesno.ClickYes();
            await Coroutine.Wait(3000, () => Talk.DialogOpen);
            if (Talk.DialogOpen)
            {
                Talk.Next();
            }

            await Coroutine.Wait(3000, () => RetainerList.Instance.IsOpen);
            await RetainerRoutine.CloseRetainers();
        }

        public static async Task<bool> ExitRetainer(bool exitList = false)
        {
            if (RetainerTasks.IsInventoryOpen())
            {
                RetainerTasks.CloseInventory();

                await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);
            }

            if (RetainerTasks.IsOpen)
            {
                RetainerTasks.CloseTasks();

                await Coroutine.Wait(3000, () => Talk.DialogOpen);
            }

            if (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(3000, () => RetainerList.Instance.IsOpen);
            }

            if (!exitList)
            {
                return RetainerList.Instance.IsOpen;
            }

            if (!RetainerList.Instance.IsOpen)
            {
                return true;
            }

            await RetainerRoutine.CloseRetainers();
            await Coroutine.Wait(3000, () => !RetainerList.Instance.IsOpen);
            return !RetainerList.Instance.IsOpen;
        }

        public static async Task RepairAll()
        {
            if (InventoryManager.EquippedItems.Any(item => item.Item != null && item.Item.RepairItemId != 0 && item.Condition < 50))
            {
                Log.Information("Repairing items.");
                await StopBusy(leaveDuty: false, stopFishing: false, dismount: false);
                if (!Repair.IsOpen)
                {
                    var repairVTable = Offsets.RepairVTable;
                    var repairVendor = Offsets.RepairVendor;
                    var repairWindow = Offsets.RepairWindowOpen;
                    var repairAgent = AgentModule.FindAgentIdByVtable(repairVTable);
                    var AgentId = repairAgent;
                    Log.Information($"OPEN: AgentId {AgentId} Offset {repairVendor.ToInt64():X} Func {repairWindow.ToInt64():X}");
                    lock (Core.Memory.Executor.AssemblyLock)
                    {
                        Core.Memory.CallInjected64<IntPtr>(
                            repairWindow,
                            ff14bot.Managers.AgentModule.GetAgentInterfaceById(AgentId).Pointer,
                            0,
                            0,
                            repairVendor);
                    }

                    await Coroutine.Wait(1500, () => Repair.IsOpen);
                }

                Repair.RepairAll();
                await Coroutine.Wait(1500, () => SelectYesno.IsOpen);
                SelectYesno.ClickYes();
                Repair.Close();
            }
            else
            {
                Log.Information("No items to repair.");
            }
        }

        public static int GetGearSetiLvl(GearSet gs)
        {
            var gear = gs.Gear.Select(i => i.Item.ItemLevel).Where(x => x > 0).ToList();
            if (!gear.Any())
            {
                return 0;
            }

            return gear.Sum(i => i) / gear.Count;
        }

        public static async Task GoHome()
        {
            var privateHousing = new uint[] { 59, 60, 61, 97 };
            var fcHousing = new uint[] { 56, 57, 58, 96 };

            var ae = WorldManager.AvailableLocations;

            var privateHouses = ae.Where(x => privateHousing.Contains(x.AetheryteId)).OrderBy(x => x.GilCost);
            var fcHouses = ae.Where(x => fcHousing.Contains(x.AetheryteId)).OrderBy(x => x.GilCost);

            var havePrivateHousing = privateHouses.Any();
            var haveFcHousing = fcHouses.Any();

            Log.Information($"Private House Access: {havePrivateHousing} FC House Access: {haveFcHousing}");

            //await GoToHousingBell(FCHouses.First());

            if (havePrivateHousing)
            {
                await GoToHousingBell(privateHouses.First());
            }
            else if (haveFcHousing)
            {
                await GoToHousingBell(fcHouses.First());
            }
        }

        private static async Task<bool> GoToHousingBell(WorldManager.TeleportLocation house)
        {
            Log.Information($"Teleporting to housing: (ZID: {house.ZoneId}, AID: {house.AetheryteId}) {house.Name}");
            await CommonTasks.Teleport(house.AetheryteId);

            Log.Information("Waiting for zone to change");
            await Coroutine.Wait(20000, () => WorldManager.ZoneId == house.ZoneId);

            Log.Information("Getting closest housing entrance");
            uint houseEntranceId = 2002737;
            uint aptEntranceId = 2007402;

            var entranceIds = new uint[] { houseEntranceId, aptEntranceId };

            var entrance = GameObjectManager.GetObjectsByNPCIds<GameObject>(entranceIds).OrderBy(x => x.Distance2D()).FirstOrDefault();
            if (entrance != null)
            {
                Log.Information("Found housing entrance, approaching");
                await Navigation.FlightorMove(entrance.Location);

                if (entrance.IsWithinInteractRange)
                {
                    Navigator.NavigationProvider.ClearStuckInfo();
                    Navigator.Stop();
                    await Coroutine.Wait(5000, () => !IsJumping);

                    entrance.Interact();

                    // Handle different housing entrance menus
                    if (entrance.NpcId == houseEntranceId)
                    {
                        Log.Information("Entering house");

                        await Coroutine.Wait(10000, () => SelectYesno.IsOpen);
                        if (SelectYesno.IsOpen)
                        {
                            SelectYesno.Yes();
                        }
                    }
                    else if (entrance.NpcId == aptEntranceId)
                    {
                        Log.Information("Entering apartment");

                        await Coroutine.Wait(10000, () => SelectString.IsOpen);
                        if (SelectString.IsOpen)
                        {
                            SelectString.ClickSlot(0);
                        }
                    }

                    await CommonTasks.HandleLoading();

                    Log.Information("Getting best summoning bell");
                    var bell = HelperFunctions.FindSummoningBell();
                    if (bell != null)
                    {
                        Log.Information("Found summoning bell, approaching");
                        await HelperFunctions.GoToSummoningBell();
                        return true;
                    }
                    else
                    {
                        Log.Error("Couldn't find any summoning bells");
                    }
                }
            }
            else
            {
                Log.Error($"Couldn't find any housing entrances.  Are we in the right zone?  Current: ({WorldManager.ZoneId}) {WorldManager.CurrentZoneName}");
            }

            return false;
        }

        public static async Task TurninOddlyDelicate()
        {
            var turnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                // BaseItemID, Tab, Index, Mini Collectability, RewardItemID
                { 31750, new CraftingRelicTurnin(31750, 0, 0, 2500, 31736) }, //Carpenter Oddly Delicate Pine Lumber --> Oddly Delicate Saw Part
                { 31751, new CraftingRelicTurnin(31751, 1, 0, 2500, 31737) }, //Blacksmith Oddly Delicate Silver gear --> Oddly Delicate Cross-pein Hammer part
                { 31752, new CraftingRelicTurnin(31752, 2, 0, 2500, 31738) }, //Armorer Oddly Delicate Wolfram Square --> Oddly Delicate Raising Hammer part
                { 31753, new CraftingRelicTurnin(31753, 3, 0, 2500, 31739) }, //Goldsmith Oddly Delicate Celestine --> Oddly Delicate Lapidary Hammer Part
                { 31754, new CraftingRelicTurnin(31754, 4, 0, 2500, 31740) }, //Leatherworker Oddly Delicate Gazelle Leather --> Oddly Delicate Round Knife Part
                { 31755, new CraftingRelicTurnin(31755, 5, 0, 2500, 31741) }, //Weaver Oddly Delicate Rhea Cloth --> Oddly Delicate Needle Part
                { 31756, new CraftingRelicTurnin(31756, 6, 0, 2500, 31742) }, //Alchemist Oddly Delicate Holy Water --> Oddly Delicate Alembic Part
                { 31757, new CraftingRelicTurnin(31757, 7, 0, 2500, 31743) }, //Cooking Oddly Delicate Shark Oil --> Oddly Delicate Frypan Part
                { 31768, new CraftingRelicTurnin(31768, 8, 0, 400, 31746) }, //Mining Oddly Delicate Adamantite Ore --> Oddly Delicate Pickaxe Part
                { 31766, new CraftingRelicTurnin(31766, 9, 0, 400, 31744) }, //Botany Oddly Delicate Feather --> Oddly Delicate Hatchet Part
                { 31770, new CraftingRelicTurnin(31770, 10, 0, 126, 31748) }, //Fishing Flinstrike --> Oddly Delicate Fishing Rod part
                { 31771, new CraftingRelicTurnin(31771, 10, 1, 62, 31749) } //Fishing Pickled Pom --> Oddly Delicate Fishing Reel part
            };

            var collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();
            var collectablesAll = InventoryManager.FilledSlots.Where(i => i.IsCollectable);
            var npcId = GameObjectManager.GetObjectByNPCId(1035014);

            if (collectables.Any(i => turnItemList.Keys.Contains(i)))
            {
                Log.Information("Have collectables");
                foreach (var collectable in collectablesAll)
                {
                    if (turnItemList.Keys.Contains(collectable.RawItemId))
                    {
                        var turnin = turnItemList[collectable.RawItemId];
                        if (collectable.Collectability < turnin.MinCollectability)
                        {
                            Log.Information($"Discarding {collectable.Name} is at {collectable.Collectability} which is under {turnin.MinCollectability}");
                            collectable.Discard();
                        }
                    }
                }

                collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();

                if (!npcId.IsWithinInteractRange)
                {
                    var target = npcId.Location;
                    Navigator.PlayerMover.MoveTowards(target);
                    while (target.Distance2D(Core.Me.Location) >= 4)
                    {
                        Navigator.PlayerMover.MoveTowards(target);
                        await Coroutine.Sleep(100);
                    }

                    Navigator.PlayerMover.MoveStop();
                }

                npcId.Interact();

                await Coroutine.Wait(10000, () => SelectIconString.IsOpen);

                if (!SelectIconString.IsOpen)
                {
                    npcId.Interact();
                    await Coroutine.Wait(10000, () => SelectIconString.IsOpen);
                }

                await Coroutine.Sleep(500);
                {
                    Log.Debug("Choosing 'Oddly Delicate Materials Exchange'.");
                    SelectIconString.ClickSlot(0);
                }

                await Coroutine.Wait(10000, () => CollectablesShop.Instance.IsOpen);

                if (CollectablesShop.Instance.IsOpen)
                {
                    Log.Verbose("Collectable window open");
                    foreach (var item in collectables)
                    {
                        if (!turnItemList.Keys.Contains(item))
                        {
                            continue;
                        }

                        Log.Information($"Turning in {DataManager.GetItem(item).CurrentLocaleName}");
                        var turnin = turnItemList[item];

                        Log.Verbose($"Pressing job {turnin.Job}");
                        CollectablesShop.Instance.SelectJob(turnin.Job);
                        await Coroutine.Sleep(500);

                        Log.Verbose($"Pressing position {turnin.Position}");
                        CollectablesShop.Instance.SelectItem(turnin.Position);
                        await Coroutine.Sleep(1000);
                        var i = 0;
                        while (CollectablesShop.Instance.TurninCount > 0)
                        {
                            Log.Verbose($"Pressing trade {i}");
                            i++;
                            CollectablesShop.Instance.Trade();
                            await Coroutine.Sleep(100);
                        }
                    }

                    CollectablesShop.Instance.Close();
                    await Coroutine.Wait(10000, () => !CollectablesShop.Instance.IsOpen);
                }
            }
        }

        public static async Task TurninResplendentCrafting()
        {
            var turnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                { 33162, new CraftingRelicTurnin(33162, 0, 2, 6300, 33194) },
                { 33163, new CraftingRelicTurnin(33163, 1, 2, 6300, 33195) },
                { 33164, new CraftingRelicTurnin(33164, 2, 2, 6300, 33196) },
                { 33165, new CraftingRelicTurnin(33165, 3, 2, 6300, 33197) },
                { 33166, new CraftingRelicTurnin(33166, 4, 2, 6300, 33198) },
                { 33167, new CraftingRelicTurnin(33167, 5, 2, 6300, 33199) },
                { 33168, new CraftingRelicTurnin(33168, 6, 2, 6300, 33200) },
                { 33169, new CraftingRelicTurnin(33169, 7, 2, 6300, 33201) },

                { 33170, new CraftingRelicTurnin(33170, 0, 1, 6500, 33202) },
                { 33171, new CraftingRelicTurnin(33171, 1, 1, 6500, 33203) },
                { 33172, new CraftingRelicTurnin(33172, 2, 1, 6500, 33204) },
                { 33173, new CraftingRelicTurnin(33173, 3, 1, 6500, 33205) },
                { 33174, new CraftingRelicTurnin(33174, 4, 1, 6500, 33206) },
                { 33175, new CraftingRelicTurnin(33175, 5, 1, 6500, 33207) },
                { 33176, new CraftingRelicTurnin(33176, 6, 1, 6500, 33208) },
                { 33177, new CraftingRelicTurnin(33177, 7, 1, 6500, 33209) },

                { 33178, new CraftingRelicTurnin(33178, 0, 0, 7000, 33210) },
                { 33179, new CraftingRelicTurnin(33179, 1, 0, 7000, 33211) },
                { 33180, new CraftingRelicTurnin(33180, 2, 0, 7000, 33212) },
                { 33181, new CraftingRelicTurnin(33181, 3, 0, 7000, 33213) },
                { 33182, new CraftingRelicTurnin(33182, 4, 0, 7000, 33214) },
                { 33183, new CraftingRelicTurnin(33183, 5, 0, 7000, 33215) },
                { 33184, new CraftingRelicTurnin(33184, 6, 0, 7000, 33216) },
                { 33185, new CraftingRelicTurnin(33185, 7, 0, 7000, 33217) }
            };

            var collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();
            var collectablesAll = InventoryManager.FilledSlots.Where(i => i.IsCollectable);

            if (collectables.Any(i => turnItemList.Keys.Contains(i)))
            {
                Log.Information("Have collectables");
                foreach (var collectable in collectablesAll)
                {
                    if (turnItemList.Keys.Contains(collectable.RawItemId))
                    {
                        var turnin = turnItemList[collectable.RawItemId];
                        if (collectable.Collectability < turnin.MinCollectability)
                        {
                            Log.Information($"Discarding {collectable.Name} is at {collectable.Collectability} which is under {turnin.MinCollectability}");
                            collectable.Discard();
                        }
                    }
                }

                collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();

                var npc = GameObjectManager.GetObjectByNPCId(1027566);
                if (npc == null)
                {
                    await Navigation.GetTo(820, new Vector3(21.06303f, 82.05f, -14.24131f));
                    npc = GameObjectManager.GetObjectByNPCId(1027566);
                }

                if (npc != null && !npc.IsWithinInteractRange)
                {
                    await Navigation.GetTo(820, new Vector3(21.06303f, 82.05f, -14.24131f));
                }

                if (npc != null && npc.IsWithinInteractRange)
                {
                    npc.Interact();
                    await Coroutine.Wait(10000, () => Conversation.IsOpen);
                    if (Conversation.IsOpen)
                    {
                        Conversation.SelectLine(0U);
                    }
                }

                await Coroutine.Wait(10000, () => CollectablesShop.Instance.IsOpen);

                if (CollectablesShop.Instance.IsOpen)
                {
                    Log.Verbose("CollectableShop window open");
                    foreach (var item in collectables)
                    {
                        if (!turnItemList.Keys.Contains(item))
                        {
                            continue;
                        }

                        Log.Information($"Turning in {DataManager.GetItem(item).CurrentLocaleName}");
                        var turnin = turnItemList[item];

                        Log.Verbose($"Pressing job {turnin.Job}");
                        CollectablesShop.Instance.SelectJob(turnin.Job);
                        await Coroutine.Sleep(500);

                        Log.Verbose($"Pressing position {turnin.Position}");
                        CollectablesShop.Instance.SelectItem(turnin.Position);
                        await Coroutine.Sleep(1000);
                        var i = 0;
                        while (CollectablesShop.Instance.TurninCount > 0)
                        {
                            Log.Verbose($"Pressing trade {i}");
                            i++;
                            CollectablesShop.Instance.Trade();
                            await Coroutine.Sleep(100);
                        }
                    }

                    CollectablesShop.Instance.Close();
                    await Coroutine.Wait(10000, () => !CollectablesShop.Instance.IsOpen);
                }
            }
        }

        public static async Task InteractWithDenys(int selectString)
        {
            var npc = GameObjectManager.GetObjectByNPCId(1032900);
            if (npc == null)
            {
                await Navigation.GetTo(418, new Vector3(-160.28f, 17.00897f, -55.8437f));
                npc = GameObjectManager.GetObjectByNPCId(1032900);
            }

            if (npc != null && !npc.IsWithinInteractRange)
            {
                await Navigation.GetTo(418, new Vector3(-160.28f, 17.00897f, -55.8437f));
            }

            if (npc != null && npc.IsWithinInteractRange)
            {
                npc.Interact();
                await Coroutine.Wait(10000, () => Conversation.IsOpen);
                if (Conversation.IsOpen)
                {
                    Conversation.SelectLine((uint)selectString);
                }
            }
        }

        public static async Task TurninSkySteelCrafting()
        {
            Dictionary<uint, CraftingRelicTurnin> TurnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                { 31101, new CraftingRelicTurnin(31101, 0, 1, 2000, 30315) },
                { 31109, new CraftingRelicTurnin(31109, 0, 0, 3000, 30316) },
                { 31102, new CraftingRelicTurnin(31102, 1, 1, 2000, 30317) },
                { 31110, new CraftingRelicTurnin(31110, 1, 0, 3000, 30318) },
                { 31103, new CraftingRelicTurnin(31103, 2, 1, 2000, 30319) },
                { 31111, new CraftingRelicTurnin(31111, 2, 0, 3000, 30320) },
                { 31104, new CraftingRelicTurnin(31104, 3, 1, 2000, 30321) },
                { 31112, new CraftingRelicTurnin(31112, 3, 0, 3000, 30322) },
                { 31105, new CraftingRelicTurnin(31105, 4, 1, 2000, 30323) },
                { 31113, new CraftingRelicTurnin(31113, 4, 0, 3000, 30324) },
                { 31106, new CraftingRelicTurnin(31106, 5, 1, 2000, 30325) },
                { 31114, new CraftingRelicTurnin(31114, 5, 0, 3000, 30326) },
                { 31107, new CraftingRelicTurnin(31107, 6, 1, 2000, 30327) },
                { 31115, new CraftingRelicTurnin(31115, 6, 0, 3000, 30328) },
                { 31108, new CraftingRelicTurnin(31108, 7, 1, 2000, 30329) },
                { 31116, new CraftingRelicTurnin(31116, 7, 0, 3000, 30330) }
            };

            var collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();
            var collectablesAll = InventoryManager.FilledSlots.Where(i => i.IsCollectable);

            if (collectables.Any(i => TurnItemList.Keys.Contains(i)))
            {
                Log.Information("Have collectables");
                foreach (var collectable in collectablesAll)
                {
                    if (TurnItemList.Keys.Contains(collectable.RawItemId))
                    {
                        var turnin = TurnItemList[collectable.RawItemId];
                        if (collectable.Collectability < turnin.MinCollectability)
                        {
                            Log.Information($"Discarding {collectable.Name} is at {collectable.Collectability} which is under {turnin.MinCollectability}");
                            collectable.Discard();
                        }
                    }
                }

                collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();

                await InteractWithDenys(2);
                await Coroutine.Wait(10000, () => CollectablesShop.Instance.IsOpen);

                if (CollectablesShop.Instance.IsOpen)
                {
                    // Log.Information("Window open");
                    foreach (var item in collectables)
                    {
                        Log.Information($"Turning in {DataManager.GetItem(item).CurrentLocaleName}");
                        var turnin = TurnItemList[item];

                        // Log.Information($"Pressing job {turnin.Job}");
                        CollectablesShop.Instance.SelectJob(turnin.Job);
                        await Coroutine.Sleep(500);

                        //  Log.Information($"Pressing position {turnin.Position}");
                        CollectablesShop.Instance.SelectItem(turnin.Position);
                        await Coroutine.Sleep(1000);
                        int i = 0;
                        while (CollectablesShop.Instance.TurninCount > 0)
                        {
                            // Log.Information($"Pressing trade {i}");
                            i++;
                            CollectablesShop.Instance.Trade();
                            await Coroutine.Sleep(100);
                        }
                    }

                    CollectablesShop.Instance.Close();
                    await Coroutine.Wait(10000, () => !CollectablesShop.Instance.IsOpen);
                }
            }
        }

        public static async Task TurninSkySteelGathering()
        {
            var GatheringItems = new Dictionary<uint, (uint Reward, uint Cost)>
            {
                { 31125, (30331, 10) },
                { 31130, (30333, 10) },
                { 31127, (30335, 10) },
                { 31132, (30337, 10) },
                { 31129, (30339, 10) },
                { 31134, (30340, 10) }
            };

            var turninItems = InventoryManager.FilledSlots.Where(i => i.IsHighQuality && GatheringItems.Keys.Contains(i.RawItemId));

            if (turninItems.Any())
            {
                await InteractWithDenys(3);
                await Coroutine.Wait(10000, () => ShopExchangeItem.Instance.IsOpen);
                if (ShopExchangeItem.Instance.IsOpen)
                {
                    Log.Information($"Window Open");
                    foreach (var turnin in turninItems)
                    {
                        var reward = GatheringItems[turnin.RawItemId].Reward;
                        var amt = turnin.Count / GatheringItems[turnin.RawItemId].Cost;
                        Log.Information($"Buying {amt}x{DataManager.GetItem(reward).CurrentLocaleName}");
                        await ShopExchangeItem.Instance.Purchase(reward, amt);
                        await Coroutine.Sleep(500);
                    }

                    ShopExchangeItem.Instance.Close();
                    await Coroutine.Wait(10000, () => !ShopExchangeItem.Instance.IsOpen);
                }
            }
        }
    }
}