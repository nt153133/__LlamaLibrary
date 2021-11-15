using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteWindows;
using TreeSharp;

namespace LlamaBotBases.Reduce
{
    public class Reduce : BotBase
    {
        private static readonly string BotName = "Aetherial Reduction";
        private static readonly Version V = new Version(1, 0, 3);

        private static readonly LLogger Log = new LLogger(BotName, Colors.Green);

        public static bool IsBusy => DutyManager.InInstance || DutyManager.InQueue || DutyManager.DutyReady || Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                                     MovementManager.IsOccupied;

        private static readonly InventoryBagId[] InventoryBagIds = new InventoryBagId[4]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4
        };

        private static readonly InventoryBagId[] ArmoryBagIds = new InventoryBagId[12]
        {
            InventoryBagId.Armory_MainHand,
            InventoryBagId.Armory_OffHand,
            InventoryBagId.Armory_Helmet,
            InventoryBagId.Armory_Chest,
            InventoryBagId.Armory_Glove,
            InventoryBagId.Armory_Belt,
            InventoryBagId.Armory_Pants,
            InventoryBagId.Armory_Boots,
            InventoryBagId.Armory_Earrings,
            InventoryBagId.Armory_Necklace,
            InventoryBagId.Armory_Writs,
            InventoryBagId.Armory_Rings
        };

        private static readonly List<string> DesynthList = new List<string>
        {
            "Warg",
            "Amaurotine",
            "Lakeland",
            "Voeburtite",
            "Fae",
            "Ravel",
            "Nabaath",
            "Anamnesis"
        };

        //private IntPtr offset => offsetInt;

        private Composite _root;

        private Settings _settings;

        public override string Name =>
#if RB_CN
                        return "精炼&分解";
#else
                "Reduce/Desynth";
#endif

#if RB_CN
                private IntPtr offset = Core.Memory.GetAbsolute(new IntPtr(0xA6E170)) ; //0xA90fd0;
#else

        //static GreyMagic.PatternFinder  patternFinder = new GreyMagic.PatternFinder(Core.Memory);
        //private readonly IntPtr offset = patternFinder.Find("48 85 D2 0F 84 ? ? ? ? 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 80 7A ? ? 41 8B E8 48 8B FA 48 8B F1 74 ? 48 8B CA E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? EB ? 0F B6 42 ? A8 ? 74 ? 8B 42 ? 05 ? ? ? ? EB ? A8 ? 8B 42 ? 74 ? 05 ? ? ? ? 85 C0 0F 84 ? ? ? ? 48 89 9C 24 ? ? ? ? 48 8B CE 4C 89 B4 24 ? ? ? ? E8 ? ? ? ? 8B 9E ? ? ? ?");
        //private IntPtr offsetInt = Core.Memory.GetAbsolute(new IntPtr(0xA6E170)); //0xA90fd0;
#endif

        //private const int offsetInt = 0xa910c0;

        public override bool WantButton => true;

        public override string EnglishName => "Reduce/Desynth";

        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        //Change here if you want to look for items that start with another string
        private string NameStarts => "Warg";

        private string NameStarts1 => "Amaurotine";

        private static bool ShouldDesynth(string name)
        {
            return DesynthList.Any(name.Contains);
        }

        public override void OnButtonPress()
        {
            if (_settings == null)
            {
                _settings = new Settings
                {
                    Text = "Reduce and Desynth v" + V //title
                };

                _settings.Closed += (o, e) => { _settings = null; };
            }

            try
            {
                _settings.Show();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void Initialize()
        {
            OffsetManager.Init();
        }

        private async Task<bool> CofferTask()
        {
            foreach (var bagslot in InventoryManager.FilledSlots.Where(bagslot => bagslot.Item.ItemAction == 388))
            {
                Log.Information($"Opening Coffer {bagslot.Name}");
                bagslot.UseItem();
                await Coroutine.Sleep(5000);
            }

            return true;
        }

        public override void Start()
        {
            Log.Information("Settings:");
            Log.Information($"Armory: {ReduceSettings.Instance.IncludeArmory}");
            Log.Information($"Include DE index < 10000: {ReduceSettings.Instance.IncludeDE10000}");
            Log.Information($"Stay running: {ReduceSettings.Instance.StayRunning}");
            Log.Information($"Zone: {ReduceSettings.Instance.AEZoneCheck} {ReduceSettings.Instance.AEZone}");
            Log.Information($"Offset Desynth: {Offsets.SalvageAgent.ToInt64():x}");
            _root = new ActionRunCoroutine(r => Run());
        }

        public override void Stop()
        {
            ReduceSettings.Instance.Save();
        }

        private async Task<bool> Run()
        {
            if (!IsBusy)
            {
                await Reduction();
                await Desynth();
                if (ReduceSettings.Instance.OpenCoffers)
                {
                    await CofferTask();
                }

                if (Translator.Language != Language.Chn)
                {
                    await Extract();
                }

                if (InventoryManager.EquippedItems.Any(i => i.Condition < 30))
                {
                    await LlamaLibrary.Helpers.Lisbeth.SelfRepair();
                }

                //ReduceSettings.Instance.Save();
            }

            if (!ReduceSettings.Instance.StayRunning)
            {
                TreeRoot.Stop("Stop Requested");
            }

            await Coroutine.Sleep(1000);
            return true;
        }

        private async Task<bool> Reduction()
        {
            //Reduce
            if (ReduceSettings.Instance.AEZoneCheck && ReduceSettings.Instance.AEZone != 0)
            {
                if (WorldManager.ZoneId != (ushort)ReduceSettings.Instance.AEZone)
                {
                    return false;
                }

                await Coroutine.Sleep(5000);
            }

            await GeneralFunctions.StopBusy(false, true, true);

            while (InventoryManager.FilledSlots.Any(x => InventoryBagIds.Contains(x.BagId) && x.IsReducable)) // && WorldManager.ZoneId == (ushort) ReduceSettings.Instance.AEZone)
            {
                var item = InventoryManager.FilledSlots.FirstOrDefault(x => InventoryBagIds.Contains(x.BagId) && x.IsReducable);

                if (item == null)
                {
                    break;
                }

                Log.Information($"Reducing - Name: {item.Item.CurrentLocaleName}");
                item.Reduce();
                await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
                await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
            }

            var windowByName = RaptureAtkUnitManager.GetWindowByName("PurifyResult");
            if (windowByName != null)
            {
                windowByName.SendAction(1, 3uL, 4294967295uL);
            }

            return true;
        }

        public static async Task<bool> Desynth(IEnumerable<BagSlot> itemsToDesynth)
        {
            if (IsBusy)
            {
                await GeneralFunctions.StopBusy(leaveDuty: false);
                if (IsBusy)
                {
                    Log.Information("Can't desynth right now, we're busy.");
                    return false;
                }
            }

            var toDesynthList = itemsToDesynth.ToList();

            if (!toDesynthList.Any())
            {
                Log.Warning("No items to desynth.");
                return false;
            }

            Log.Warning($"# of slots to Desynth: {toDesynthList.Count()}");
            foreach (var bagSlot in toDesynthList)
            {
                bagSlot.Desynth();
                await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
                await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
            }

            if (SalvageResult.IsOpen)
            {
                SalvageResult.Close();
                await Coroutine.Wait(5000, () => !SalvageResult.IsOpen);
            }

            if (SalvageAutoDialog.Instance.IsOpen)
            {
                SalvageAutoDialog.Instance.Close();
                await Coroutine.Wait(5000, () => !SalvageAutoDialog.Instance.IsOpen);
            }

            return true;
        }

        public static async Task<bool> Desynth()
        {
            if (IsBusy)
            {
                await GeneralFunctions.StopBusy(leaveDuty: false);
                if (IsBusy)
                {
                    Log.Warning("Can't desynth right now, we're busy.");
                    return false;
                }
            }

            var toDesynthList = InventoryManager.GetBagsByInventoryBagId(BagsToCheck())
                                                           .SelectMany(bag => bag.FilledSlots
                                                                                 .FindAll(bs => bs.IsDesynthesizable && (ShouldDesynth(bs.Item.EnglishName) || ExtraCheck(bs)))).ToList();

            //if (MovementManager.IsOccupied) return false;
            //          if (!InventoryManager.GetBagsByInventoryBagId(BagsToCheck()).Any(bag => bag.FilledSlots.Any(bs => bs.IsDesynthesizable)))

            /*            var itemsToDesynth = InventoryManager.GetBagsByInventoryBagId(BagsToCheck())
                            .SelectMany(bag => bag.FilledSlots
                                .FindAll(bs => bs.IsDesynthesizable && (ShouldDesynth(bs.Item.EnglishName) || ExtraCheck(bs))));*/

            if (!toDesynthList.Any())
            {
                Log.Warning("No items to desynth.");
                return false;
            }

            Log.Information($"# of slots to Desynth: {toDesynthList.Count()}");

            foreach (var bagSlot in toDesynthList)
            {
                var itemId = bagSlot.RawItemId;
                while (bagSlot.IsValid && bagSlot.IsFilled && bagSlot.RawItemId == itemId)
                {
                    bagSlot.Desynth();
                    await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
                    await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
                }
            }

            if (SalvageResult.IsOpen)
            {
                SalvageResult.Close();
                await Coroutine.Wait(5000, () => !SalvageResult.IsOpen);
            }

            if (SalvageAutoDialog.Instance.IsOpen)
            {
                SalvageAutoDialog.Instance.Close();
                await Coroutine.Wait(5000, () => !SalvageAutoDialog.Instance.IsOpen);
            }

            return true;
        }

        public static async Task DesynthItem(BagSlot slot)
        {
            var agentSalvageInterface = AgentInterface<AgentSalvage>.Instance;
            var agentSalvage = Offsets.SalvageAgent;

            Log.Information($"Desynthesize Item - Name: {slot.Item.CurrentLocaleName}{(slot.Item.IsHighQuality ? " HQ" : string.Empty)}");
            var itemId = slot.RawItemId;
            while (slot.IsValid && slot.IsFilled && slot.RawItemId == itemId)
            {
                lock (Core.Memory.Executor.AssemblyLock)
                {
                    Core.Memory.CallInjected64<int>(agentSalvage, agentSalvageInterface.Pointer, slot.Pointer, 14, 0);
                }

                await Coroutine.Sleep(200);

                Log.Verbose($"Waiting for SalvageDialog.");
                await Coroutine.Wait(5000, () => SalvageDialog.IsOpen);

                if (SalvageDialog.IsOpen)
                {
                    Log.Verbose($"Sending Desynth action.");
                    RaptureAtkUnitManager.GetWindowByName("SalvageDialog").SendAction(1, 3, 0);
                    await Coroutine.Sleep(500);
                }

                Log.Verbose($"Wait for DesynthLock byte 1.");
                await Coroutine.Wait(5000, () => Core.Memory.NoCacheRead<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);

                Log.Verbose($"Wait for DesynthLock byte 0");
                await Coroutine.Wait(6000, () => Core.Memory.NoCacheRead<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
                await Coroutine.Sleep(100);
                await Coroutine.Wait(6000, () => SalvageResult.IsOpen || SalvageAutoDialog.Instance.IsOpen);

                if (SalvageAutoDialog.Instance.IsOpen)
                {
                    await Coroutine.Wait(-1, () => !slot.IsValid || !slot.IsFilled || Core.Memory.NoCacheRead<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
                    await Coroutine.Sleep(300);
                    SalvageAutoDialog.Instance.Close();
                    await Coroutine.Wait(5000, () => !SalvageAutoDialog.Instance.IsOpen);
                    continue;
                }

                if (IsBusy)
                {
                    break;
                }
            }
        }

        public static async Task Extract()
        {
            if (IsBusy)
            {
                await GeneralFunctions.SmallTalk();
                await GeneralFunctions.StopBusy(leaveDuty: false);
                if (IsBusy)
                {
                    return;
                }
            }

            var gear = InventoryManager.FilledInventoryAndArmory.Where(x => x.SpiritBond == 100f);
            if (gear.Any())
            {
                foreach (var slot in gear)
                {
                    Log.Information($"Extract Materia from: {slot}");
                    slot.ExtractMateria();
                    await Coroutine.Wait(5000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
                    await Coroutine.Wait(6000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
                    await Coroutine.Sleep(100);

                    if (IsBusy)
                    {
                        return;
                    }
                }
            }
        }

        private static bool ExtraCheck(BagSlot bs)
        {
            //return (bs.Item.EquipmentCatagory == ItemUiCategory.Seafood && bs.CanDesynthesize);
            return false;
        }

        private static InventoryBagId[] BagsToCheck()
        {
            return ReduceSettings.Instance.IncludeArmory ? InventoryBagIds.Concat(ArmoryBagIds).ToArray() : InventoryBagIds;

            //return inventoryBagIds;
        }
    }
}