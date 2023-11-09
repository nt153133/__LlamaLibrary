using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
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
        private static readonly LLogger Log = new(nameof(GeneralFunctions), Colors.Aquamarine);

        private static FrameCachedObject<QuestLayout[]>? _questLayouts = null;

        public static FrameCachedObject<QuestLayout[]>? QuestLayouts
        {
            get
            {
                if (_questLayouts == null)
                {
                    var field = typeof(QuestLogManager).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).FirstOrDefault(i => i.FieldType == typeof(FrameCachedObject<QuestLayout[]>));
                    _questLayouts = field?.GetValue(null) as FrameCachedObject<QuestLayout[]>;
                }

                return _questLayouts;
            }
        }

        public static readonly InventoryBagId[] MainBags = { InventoryBagId.Bag1, InventoryBagId.Bag2, InventoryBagId.Bag3, InventoryBagId.Bag4 };

        public static readonly InventoryBagId[] SaddlebagIds =
        {
            (InventoryBagId)0xFA0, (InventoryBagId)0xFA1 //, (InventoryBagId) 0x1004,(InventoryBagId) 0x1005
        };

        public static IEnumerable<BagSlot> MainBagsFilledSlots()
        {
            return InventoryManager.GetBagsByInventoryBagId(MainBags).SelectMany(x => x.FilledSlots);
        }

        public static ClassJobType QuestClass(int questId)
        {
            if (questId > 65536)
            {
                questId -= 65536;
            }

            var quest = QuestLayouts?.Value.FirstOrDefault(i => i.ID == questId);
            if (quest == null)
            {
                return ClassJobType.Adventurer;
            }

            return (ClassJobType)quest.Value.QuestBytes[7];
        }

        public static bool IsJumping => Core.Memory.NoCacheRead<byte>(Offsets.Conditions + Offsets.JumpingCondition) != 0;

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

            try
            {
                if (Core.Me.HasTarget && Core.Me.CurrentTargetObjId != Core.Me.ObjectId)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return false;
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

                if (GrandCompanySupplyList.Instance.IsOpen)
                {
                    GrandCompanySupplyList.Instance.Close();
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

                if (Gathering.Instance.IsOpen)
                {
                    Log.Information("Closing gathering window.");
                    Gathering.Instance.Close();
                    await Coroutine.Wait(3000, () => !Gathering.Instance.IsOpen);
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
            return slotId switch
            {
                0        => ItemWeight.MainHands,
                1        => ItemWeight.OffHands,
                2        => new List<ItemUiCategory> { ItemUiCategory.Head },
                3        => new List<ItemUiCategory> { ItemUiCategory.Body },
                4        => new List<ItemUiCategory> { ItemUiCategory.Hands },
                5        => new List<ItemUiCategory> { ItemUiCategory.Waist },
                6        => new List<ItemUiCategory> { ItemUiCategory.Legs },
                7        => new List<ItemUiCategory> { ItemUiCategory.Feet },
                8        => new List<ItemUiCategory> { ItemUiCategory.Earrings },
                9        => new List<ItemUiCategory> { ItemUiCategory.Necklace },
                10       => new List<ItemUiCategory> { ItemUiCategory.Bracelets },
                11 or 12 => new List<ItemUiCategory> { ItemUiCategory.Ring },
                13       => new List<ItemUiCategory> { ItemUiCategory.Soul_Crystal },
                _        => new List<ItemUiCategory>(),
            };
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

        /*
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
                        Core.Memory.CallInjected64<IntPtr>(repairWindow,
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
        */

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

            var privateHouses = ae.Where(x => privateHousing.Contains(x.AetheryteId)).OrderBy(x => x.SubIndex);
            var fcHouses = ae.Where(x => fcHousing.Contains(x.AetheryteId)).OrderBy(x => x.SubIndex);

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
                { 31750, new CraftingRelicTurnin(31750, 0, 0, 470, 31736) }, //Carpenter Oddly Delicate Pine Lumber --> Oddly Delicate Saw Part
                { 31751, new CraftingRelicTurnin(31751, 1, 0, 470, 31737) }, //Blacksmith Oddly Delicate Silver gear --> Oddly Delicate Cross-pein Hammer part
                { 31752, new CraftingRelicTurnin(31752, 2, 0, 470, 31738) }, //Armorer Oddly Delicate Wolfram Square --> Oddly Delicate Raising Hammer part
                { 31753, new CraftingRelicTurnin(31753, 3, 0, 470, 31739) }, //Goldsmith Oddly Delicate Celestine --> Oddly Delicate Lapidary Hammer Part
                { 31754, new CraftingRelicTurnin(31754, 4, 0, 470, 31740) }, //Leatherworker Oddly Delicate Gazelle Leather --> Oddly Delicate Round Knife Part
                { 31755, new CraftingRelicTurnin(31755, 5, 0, 470, 31741) }, //Weaver Oddly Delicate Rhea Cloth --> Oddly Delicate Needle Part
                { 31756, new CraftingRelicTurnin(31756, 6, 0, 470, 31742) }, //Alchemist Oddly Delicate Holy Water --> Oddly Delicate Alembic Part
                { 31757, new CraftingRelicTurnin(31757, 7, 0, 470, 31743) }, //Cooking Oddly Delicate Shark Oil --> Oddly Delicate Frypan Part
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
                { 36311, new CraftingRelicTurnin(36311, 0, 2, 1200, 33194) },
                { 36312, new CraftingRelicTurnin(36312, 1, 2, 1200, 33195) },
                { 36313, new CraftingRelicTurnin(36313, 2, 2, 1200, 33196) },
                { 36314, new CraftingRelicTurnin(36314, 3, 2, 1200, 33197) },
                { 36315, new CraftingRelicTurnin(36315, 4, 2, 1200, 33198) },
                { 36316, new CraftingRelicTurnin(36316, 5, 2, 1200, 33199) },
                { 36317, new CraftingRelicTurnin(36317, 6, 2, 1200, 33200) },
                { 36318, new CraftingRelicTurnin(36318, 7, 2, 1200, 33201) },

                { 36319, new CraftingRelicTurnin(36319, 0, 1, 1230, 33202) },
                { 36320, new CraftingRelicTurnin(36320, 1, 1, 1230, 33203) },
                { 36321, new CraftingRelicTurnin(36321, 2, 1, 1230, 33204) },
                { 36322, new CraftingRelicTurnin(36322, 3, 1, 1230, 33205) },
                { 36323, new CraftingRelicTurnin(36323, 4, 1, 1230, 33206) },
                { 36324, new CraftingRelicTurnin(36324, 5, 1, 1230, 33207) },
                { 36325, new CraftingRelicTurnin(36325, 6, 1, 1230, 33208) },
                { 36326, new CraftingRelicTurnin(36326, 7, 1, 1230, 33209) },

                { 36327, new CraftingRelicTurnin(36327, 0, 0, 1330, 33210) },
                { 36328, new CraftingRelicTurnin(36328, 1, 0, 1330, 33211) },
                { 36329, new CraftingRelicTurnin(36329, 2, 0, 1330, 33212) },
                { 36330, new CraftingRelicTurnin(36330, 3, 0, 1330, 33213) },
                { 36331, new CraftingRelicTurnin(36331, 4, 0, 1330, 33214) },
                { 36332, new CraftingRelicTurnin(36332, 5, 0, 1330, 33215) },
                { 36333, new CraftingRelicTurnin(36333, 6, 0, 1330, 33216) },
                { 36334, new CraftingRelicTurnin(36334, 7, 0, 1330, 33217) }
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

        public static async Task TurninSplendorousCrafting()
        {
            var turnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                // Connoisseur's Item, job index, position on list, collectability, reward item
                { 39781, new CraftingRelicTurnin(39781, 0, 0, 780, 39797) },
                { 39782, new CraftingRelicTurnin(39782, 1, 0, 780, 39798) },
                { 39783, new CraftingRelicTurnin(39783, 2, 0, 780, 39799) },
                { 39784, new CraftingRelicTurnin(39784, 3, 0, 780, 39800) },
                { 39785, new CraftingRelicTurnin(39785, 4, 0, 780, 39801) },
                { 39786, new CraftingRelicTurnin(39786, 5, 0, 780, 39802) },
                { 39787, new CraftingRelicTurnin(39787, 6, 0, 780, 39803) },
                { 39788, new CraftingRelicTurnin(39788, 7, 0, 780, 39804) },
                { 39811, new CraftingRelicTurnin(39811, 8, 0, 580, 39821) },
                { 39813, new CraftingRelicTurnin(39813, 9, 0, 570, 39822) },

                { 39773, new CraftingRelicTurnin(39773, 0, 1, 720, 39789) },
                { 39774, new CraftingRelicTurnin(39774, 1, 1, 720, 39790) },
                { 39775, new CraftingRelicTurnin(39775, 2, 1, 720, 39791) },
                { 39776, new CraftingRelicTurnin(39776, 3, 1, 720, 39792) },
                { 39777, new CraftingRelicTurnin(39777, 4, 1, 720, 39793) },
                { 39778, new CraftingRelicTurnin(39778, 5, 1, 720, 39794) },
                { 39779, new CraftingRelicTurnin(39779, 6, 1, 720, 39795) },
                { 39780, new CraftingRelicTurnin(39780, 7, 1, 720, 39796) },
                { 39805, new CraftingRelicTurnin(39805, 8, 1, 720, 39817) },
                { 39807, new CraftingRelicTurnin(39807, 9, 1, 720, 39818) },

                { 38764, new CraftingRelicTurnin(38764, 0, 2, 660, 38780) },
                { 38765, new CraftingRelicTurnin(38765, 1, 2, 660, 38781) },
                { 38766, new CraftingRelicTurnin(38766, 2, 2, 660, 38782) },
                { 38767, new CraftingRelicTurnin(38767, 3, 2, 660, 38783) },
                { 38768, new CraftingRelicTurnin(38768, 4, 2, 660, 38784) },
                { 38769, new CraftingRelicTurnin(38769, 5, 2, 660, 38785) },
                { 38770, new CraftingRelicTurnin(38770, 6, 2, 660, 38786) },
                { 38771, new CraftingRelicTurnin(38771, 7, 2, 660, 38787) },
                { 38796, new CraftingRelicTurnin(38790, 8, 2, 570, 38805) },
                { 38794, new CraftingRelicTurnin(38794, 9, 2, 570, 38800) },

                { 38756, new CraftingRelicTurnin(38756, 0, 3, 540, 38772) },
                { 38757, new CraftingRelicTurnin(38757, 1, 3, 540, 38773) },
                { 38758, new CraftingRelicTurnin(38758, 2, 3, 540, 38774) },
                { 38759, new CraftingRelicTurnin(38759, 3, 3, 540, 38775) },
                { 38760, new CraftingRelicTurnin(38760, 4, 3, 540, 38776) },
                { 38761, new CraftingRelicTurnin(38761, 5, 3, 540, 38777) },
                { 38762, new CraftingRelicTurnin(38762, 6, 3, 540, 38778) },
                { 38763, new CraftingRelicTurnin(38763, 7, 3, 540, 38779) },
                { 38790, new CraftingRelicTurnin(38790, 8, 3, 570, 38801) },
                { 38788, new CraftingRelicTurnin(38788, 9, 3, 570, 38804) },

                { 39815, new CraftingRelicTurnin(39815, 10, 0, 94, 39823) },
                { 39816, new CraftingRelicTurnin(39816, 10, 1, 47, 39824) },
                { 39809, new CraftingRelicTurnin(39809, 10, 2, 82, 39819) },
                { 39810, new CraftingRelicTurnin(39810, 10, 3, 322, 39820) },
                { 38798, new CraftingRelicTurnin(38798, 10, 4, 9, 38806) },
                { 38799, new CraftingRelicTurnin(38799, 10, 5, 425, 38807) },
                { 38792, new CraftingRelicTurnin(38792, 10, 6, 47, 38802) },
                { 38793, new CraftingRelicTurnin(38793, 10, 7, 283, 38803) },
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

                var npc = GameObjectManager.GetObjectByNPCId(1045069);
                if (npc == null)
                {
                    await Navigation.GetTo(819, new Vector3(-39.10532f, 20.04979f, -171.9984f));
                    npc = GameObjectManager.GetObjectByNPCId(1045069);
                }

                if (npc != null && !npc.IsWithinInteractRange)
                {
                    await Navigation.GetTo(819, new Vector3(-39.10532f, 20.04979f, -171.9984f));
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

        public static async Task TurninSplendorous651Crafting()
        {
            var turnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                // Connoisseur's Item, job index, position on list, collectability, reward item
                { 41262, new CraftingRelicTurnin(41262, 0, 0, 800, 41278) },
                { 41263, new CraftingRelicTurnin(41263, 1, 0, 800, 41279) },
                { 41264, new CraftingRelicTurnin(41264, 2, 0, 800, 41280) },
                { 41265, new CraftingRelicTurnin(41265, 3, 0, 800, 41281) },
                { 41266, new CraftingRelicTurnin(41266, 4, 0, 800, 41282) },
                { 41267, new CraftingRelicTurnin(41267, 5, 0, 800, 41283) },
                { 41268, new CraftingRelicTurnin(41268, 6, 0, 800, 41284) },
                { 41269, new CraftingRelicTurnin(41269, 7, 0, 800, 41285) },
                { 41290, new CraftingRelicTurnin(41290, 8, 0, 580, 41296) },
                { 41292, new CraftingRelicTurnin(41292, 9, 0, 580, 41297) },

                { 41254, new CraftingRelicTurnin(41254, 0, 1, 750, 41270) },
                { 41255, new CraftingRelicTurnin(41255, 1, 1, 750, 41271) },
                { 41256, new CraftingRelicTurnin(41256, 2, 1, 750, 41272) },
                { 41257, new CraftingRelicTurnin(41257, 3, 1, 750, 41273) },
                { 41258, new CraftingRelicTurnin(41258, 4, 1, 750, 41274) },
                { 41259, new CraftingRelicTurnin(41259, 5, 1, 750, 41275) },
                { 41260, new CraftingRelicTurnin(41260, 6, 1, 750, 41276) },
                { 41261, new CraftingRelicTurnin(41261, 7, 1, 750, 41277) },
                { 41286, new CraftingRelicTurnin(41286, 8, 1, 570, 41294) },
                { 41288, new CraftingRelicTurnin(41288, 9, 1, 570, 41295) },

                { 39781, new CraftingRelicTurnin(39781, 0, 2, 780, 39797) },
                { 39782, new CraftingRelicTurnin(39782, 1, 2, 780, 39798) },
                { 39783, new CraftingRelicTurnin(39783, 2, 2, 780, 39799) },
                { 39784, new CraftingRelicTurnin(39784, 3, 2, 780, 39800) },
                { 39785, new CraftingRelicTurnin(39785, 4, 2, 780, 39801) },
                { 39786, new CraftingRelicTurnin(39786, 5, 2, 780, 39802) },
                { 39787, new CraftingRelicTurnin(39787, 6, 2, 780, 39803) },
                { 39788, new CraftingRelicTurnin(39788, 7, 2, 780, 39804) },
                { 39811, new CraftingRelicTurnin(39811, 8, 2, 580, 39821) },
                { 39813, new CraftingRelicTurnin(39813, 9, 2, 570, 39822) },

                { 39773, new CraftingRelicTurnin(39773, 0, 3, 720, 39789) },
                { 39774, new CraftingRelicTurnin(39774, 1, 3, 720, 39790) },
                { 39775, new CraftingRelicTurnin(39775, 2, 3, 720, 39791) },
                { 39776, new CraftingRelicTurnin(39776, 3, 3, 720, 39792) },
                { 39777, new CraftingRelicTurnin(39777, 4, 3, 720, 39793) },
                { 39778, new CraftingRelicTurnin(39778, 5, 3, 720, 39794) },
                { 39779, new CraftingRelicTurnin(39779, 6, 3, 720, 39795) },
                { 39780, new CraftingRelicTurnin(39780, 7, 3, 720, 39796) },
                { 39805, new CraftingRelicTurnin(39805, 8, 3, 720, 39817) },
                { 39807, new CraftingRelicTurnin(39807, 9, 3, 720, 39818) },

                { 38764, new CraftingRelicTurnin(38764, 0, 4, 660, 38780) },
                { 38765, new CraftingRelicTurnin(38765, 1, 4, 660, 38781) },
                { 38766, new CraftingRelicTurnin(38766, 2, 4, 660, 38782) },
                { 38767, new CraftingRelicTurnin(38767, 3, 4, 660, 38783) },
                { 38768, new CraftingRelicTurnin(38768, 4, 4, 660, 38784) },
                { 38769, new CraftingRelicTurnin(38769, 5, 4, 660, 38785) },
                { 38770, new CraftingRelicTurnin(38770, 6, 4, 660, 38786) },
                { 38771, new CraftingRelicTurnin(38771, 7, 4, 660, 38787) },
                { 38796, new CraftingRelicTurnin(38790, 8, 4, 570, 38805) },
                { 38794, new CraftingRelicTurnin(38794, 9, 4, 570, 38800) },

                { 38756, new CraftingRelicTurnin(38756, 0, 5, 540, 38772) },
                { 38757, new CraftingRelicTurnin(38757, 1, 5, 540, 38773) },
                { 38758, new CraftingRelicTurnin(38758, 2, 5, 540, 38774) },
                { 38759, new CraftingRelicTurnin(38759, 3, 5, 540, 38775) },
                { 38760, new CraftingRelicTurnin(38760, 4, 5, 540, 38776) },
                { 38761, new CraftingRelicTurnin(38761, 5, 5, 540, 38777) },
                { 38762, new CraftingRelicTurnin(38762, 6, 5, 540, 38778) },
                { 38763, new CraftingRelicTurnin(38763, 7, 5, 540, 38779) },
                { 38790, new CraftingRelicTurnin(38790, 8, 5, 570, 38801) },
                { 38788, new CraftingRelicTurnin(38788, 9, 5, 570, 38804) },

                { 41300, new CraftingRelicTurnin(41300, 10, 0, 9, 41304) },
                { 41301, new CraftingRelicTurnin(41301, 10, 1, 384, 41304) },
                { 41298, new CraftingRelicTurnin(41298, 10, 2, 501, 41303) },
                { 41299, new CraftingRelicTurnin(41299, 10, 3, 501, 41303) },
                { 39815, new CraftingRelicTurnin(39815, 10, 4, 94, 39823) },
                { 39816, new CraftingRelicTurnin(39816, 10, 5, 47, 39824) },
                { 39809, new CraftingRelicTurnin(39809, 10, 6, 82, 39819) },
                { 39810, new CraftingRelicTurnin(39810, 10, 7, 322, 39820) },
                { 38798, new CraftingRelicTurnin(38798, 10, 8, 9, 38806) },
                { 38799, new CraftingRelicTurnin(38799, 10, 9, 425, 38807) },
                { 38792, new CraftingRelicTurnin(38792, 10, 10, 47, 38802) },
                { 38793, new CraftingRelicTurnin(38793, 10, 11, 283, 38803) },
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

                var npc = GameObjectManager.GetObjectByNPCId(1045069);
                if (npc == null)
                {
                    await Navigation.GetTo(819, new Vector3(-39.10532f, 20.04979f, -171.9984f));
                    npc = GameObjectManager.GetObjectByNPCId(1045069);
                }

                if (npc != null && !npc.IsWithinInteractRange)
                {
                    await Navigation.GetTo(819, new Vector3(-39.10532f, 20.04979f, -171.9984f));
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

        public static async Task TurninCNSplendorousCrafting()
        {
            var turnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                // Connoisseur's Item, job index, position on list, collectability, reward item
                { 38764, new CraftingRelicTurnin(38764, 0, 0, 660, 38780) },
                { 38765, new CraftingRelicTurnin(38765, 1, 0, 660, 38781) },
                { 38766, new CraftingRelicTurnin(38766, 2, 0, 660, 38782) },
                { 38767, new CraftingRelicTurnin(38767, 3, 0, 660, 38783) },
                { 38768, new CraftingRelicTurnin(38768, 4, 0, 660, 38784) },
                { 38769, new CraftingRelicTurnin(38769, 5, 0, 660, 38785) },
                { 38770, new CraftingRelicTurnin(38770, 6, 0, 660, 38786) },
                { 38771, new CraftingRelicTurnin(38771, 7, 0, 660, 38787) },
                { 38796, new CraftingRelicTurnin(38790, 8, 0, 570, 38805) },
                { 38794, new CraftingRelicTurnin(38794, 9, 0, 570, 38800) },
                { 38798, new CraftingRelicTurnin(38798, 10, 0, 9, 38806) },

                { 38756, new CraftingRelicTurnin(38756, 0, 1, 540, 38772) },
                { 38757, new CraftingRelicTurnin(38757, 1, 1, 540, 38773) },
                { 38758, new CraftingRelicTurnin(38758, 2, 1, 540, 38774) },
                { 38759, new CraftingRelicTurnin(38759, 3, 1, 540, 38775) },
                { 38760, new CraftingRelicTurnin(38760, 4, 1, 540, 38776) },
                { 38761, new CraftingRelicTurnin(38761, 5, 1, 540, 38777) },
                { 38762, new CraftingRelicTurnin(38762, 6, 1, 540, 38778) },
                { 38763, new CraftingRelicTurnin(38763, 7, 1, 540, 38779) },
                { 38790, new CraftingRelicTurnin(38790, 8, 1, 570, 38801) },
                { 38788, new CraftingRelicTurnin(38788, 9, 1, 570, 38804) },
                { 38799, new CraftingRelicTurnin(38799, 10, 1, 425, 38807) },

                { 38792, new CraftingRelicTurnin(38792, 10, 2, 47, 38802) },
                { 38793, new CraftingRelicTurnin(38793, 10, 3, 283, 38803) },
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

                var npc = GameObjectManager.GetObjectByNPCId(1045069);
                if (npc == null)
                {
                    await Navigation.GetTo(819, new Vector3(-39.10532f, 20.04979f, -171.9984f));
                    npc = GameObjectManager.GetObjectByNPCId(1045069);
                }

                if (npc != null && !npc.IsWithinInteractRange)
                {
                    await Navigation.GetTo(819, new Vector3(-39.10532f, 20.04979f, -171.9984f));
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

        public static async Task OpenChests()
        {
            LLogger lLogger = new("Treasure", Colors.Gold);

            var chests = GetTreasureChests().ToArray();

            for (var index = 0; index < chests.Length; index++)
            {
                var chest = chests[index];

                if (!await Navigation.GetTo(WorldManager.ZoneId, chest.Location))
                {
                    lLogger.Information("GetTo failed using offmesh");
                    await Navigation.OffMeshMoveInteract(chest);
                }

                if (chest.Location.Distance(Core.Me.Location) > 1)
                {
                    lLogger.Information("Moving closer");
                    Navigator.PlayerMover.MoveTowards(chest.Location);
                    while (chest.Location.Distance(Core.Me.Location) > 1)
                    {
                        Navigator.PlayerMover.MoveTowards(chest.Location);
                        await Coroutine.Sleep(100);
                    }

                    Navigator.PlayerMover.MoveStop();
                }

                if (chest.Location.Distance(Core.Me.Location) > 1)
                {
                    lLogger.Information("Couldn't get in range - Skipping");
                    continue;
                }

                chest.Target();
                chest.Interact();

                if (!await Coroutine.Wait(3000, () => chest.State != 0))
                {
                    lLogger.Information("Interact failed trying again");
                    chest.Target();
                    chest.Interact();
                    await Coroutine.Wait(3000, () => chest.State != 0);
                }

                lLogger.Information("Chest should be open");

                if (index < chests.Length - 1)
                {
                    lLogger.Information("There is multiple chests, sleeping 1s between");
                    await Coroutine.Sleep(1000);
                }
            }
        }

        public static IEnumerable<Treasure> GetTreasureChests()
        {
            return GameObjectManager.GetObjectsOfType<Treasure>().Where(i => i.Location.Distance2D(Core.Me.Location) <= 30 && i.InLineOfSight() && i.State == 0);
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
            var TurnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                { 31101, new CraftingRelicTurnin(31101, 0, 1, 360, 30315) },
                { 31109, new CraftingRelicTurnin(31109, 0, 0, 400, 30316) },
                { 31102, new CraftingRelicTurnin(31102, 1, 1, 360, 30317) },
                { 31110, new CraftingRelicTurnin(31110, 1, 0, 400, 30318) },
                { 31103, new CraftingRelicTurnin(31103, 2, 1, 360, 30319) },
                { 31111, new CraftingRelicTurnin(31111, 2, 0, 400, 30320) },
                { 31104, new CraftingRelicTurnin(31104, 3, 1, 360, 30321) },
                { 31112, new CraftingRelicTurnin(31112, 3, 0, 400, 30322) },
                { 31105, new CraftingRelicTurnin(31105, 4, 1, 360, 30323) },
                { 31113, new CraftingRelicTurnin(31113, 4, 0, 400, 30324) },
                { 31106, new CraftingRelicTurnin(31106, 5, 1, 360, 30325) },
                { 31114, new CraftingRelicTurnin(31114, 5, 0, 400, 30326) },
                { 31107, new CraftingRelicTurnin(31107, 6, 1, 360, 30327) },
                { 31115, new CraftingRelicTurnin(31115, 6, 0, 400, 30328) },
                { 31108, new CraftingRelicTurnin(31108, 7, 1, 360, 30329) },
                { 31116, new CraftingRelicTurnin(31116, 7, 0, 400, 30330) }
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
                        var i = 0;
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

        public static bool IsDutyComplete(uint dutyId)
        {
            return DataManager.InstanceContentResults.TryGetValue(dutyId, out var instanceContentResult) && IsInstanceContentCompleted(instanceContentResult.Content);
        }

        public static bool IsDutyUnlocked(uint dutyId)
        {
            return DataManager.InstanceContentResults.TryGetValue(dutyId, out var instanceContentResult) && Core.Memory.CallInjected64<bool>(Offsets.IsInstanceContentCompleted, instanceContentResult.Content);
        }

        public static async Task PassOnAllLoot()
        {
            if (!LlamaLibrary.RemoteWindows.NotificationLoot.Instance.IsOpen)
            {
                Log.Information($"Loot window not present, exiting");
                return;
            }

            var window = RaptureAtkUnitManager.GetWindowByName("_Notification");

            if (!NeedGreed.Instance.IsOpen && window != null)
            {
                window.SendAction(3, 3, 0, 3, 2, 6, 0x375B30E7);
                await Coroutine.Wait(5000, () => NeedGreed.Instance.IsOpen);
            }

            if (NeedGreed.Instance.IsOpen)
            {
                for (var i = 0; i < NeedGreed.Instance.NumberOfItems; i++)
                {
                    NeedGreed.Instance.PassItem(i);
                    await Coroutine.Sleep(500);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }

                    if (!NeedGreed.Instance.IsOpen)
                    {
                        break;
                    }
                }
            }

            if (NeedGreed.Instance.IsOpen)
            {
                NeedGreed.Instance.Close();
            }
        }

        public static async Task VoteMVPTask()
        {
            Log.Information("Voting on MVP");

            var name = await AgentVoteMVP.Instance.OpenAndVoteName();

            Log.Information($"Voted for {name}");
        }

        public static bool IsInstanceContentCompleted(uint instantContentId)
        {
            return Core.Memory.CallInjected64<bool>(Offsets.IsInstanceContentCompleted, instantContentId);
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static string SourceFileName()
        {
            var frame = new StackFrame(1, true);
            return frame.GetFileName();
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static DirectoryInfo? SourceDirectory()
        {
            var frame = new StackFrame(1, true);
            var file = frame.GetFileName();

            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                return new DirectoryInfo(Path.GetDirectoryName(file) ?? throw new InvalidOperationException());
            }

            return null;
        }
    }
}