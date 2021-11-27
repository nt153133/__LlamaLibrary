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
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaBotBases.LlamaUtilities.Settings;
using LlamaBotBases.LlamaUtilities.Tasks;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;
using Newtonsoft.Json;
using TreeSharp;

namespace LlamaBotBases.LlamaUtilities
{
    public class UtilitiesBase : BotBase
    {
        public static readonly string _name = "Llama Utilities";
        private static readonly LLogger Log = new LLogger(_name, Colors.Pink);
        public static BotTask BotTask = new BotTask();

        public override bool WantButton => true;
        public override string Name => _name;
        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;
        public override Composite Root => _root;

        private Composite _root;
        private Utilities settings;

        public static bool IsBusy => DutyManager.InInstance || DutyManager.InQueue || DutyManager.DutyReady || Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                                     MovementManager.IsOccupied;
        private static readonly List<string> DesynthList = new List<string>
        {
            "Warg",
            "Amaurotine",
            "Lakeland",
            "Voeburtite",
            "Fae",
            "Ravel",
            "Nabaath",
            "Anamnesis",
            "Shadowless",
            "Heirloom",
            "Paglth'an"
        };

        public UtilitiesBase()
        {
            OffsetManager.Init();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Start()
        {
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            Navigator.PlayerMover = new SlideMover();

            if (BotTask.Type == TaskType.None)
            {
                Log.Error("Use the settings window to run a task");
                return;
            }

            if (BotTask.Type == TaskType.AutoFollow)
            {
                //Just....no that botbase is shit
            }
            else
            {
                _root = new ActionRunCoroutine(r => Run());
            }
        }

        public override void Stop()
        {
            (Navigator.NavigationProvider as IDisposable)?.Dispose();
            Navigator.NavigationProvider = null;
        }

        public override void OnButtonPress()
        {
            if (settings == null || settings.IsDisposed)
            {
                settings = new Utilities();
            }

            try
            {
                settings.Show();
                settings.Activate();
            }
            catch
            {
                // ignored
            }
        }

        private async Task<bool> Run()
        {
            switch (BotTask.Type)
            {
                case TaskType.MateriaRemove:
                    var bagInfo = JsonConvert.DeserializeObject<(uint, ushort)>(BotTask.TaskInfo);
                    var slot = InventoryManager.GetBagByInventoryBagId((InventoryBagId)bagInfo.Item1).First(i => i.Slot == bagInfo.Item2);
                    await RemoveMateria(slot);
                    break;
                case TaskType.AutoFollow:
                    break;
                case TaskType.Reduce:
                    await Inventory.ReduceAll();
                    break;
                case TaskType.Desynth:
                    await Desynth();
                    break;
                case TaskType.None:
                    break;
                case TaskType.Hunts:
                    var huntTypes = new List<int>();
                    if (HuntsSettings.Instance.ARRHunts)
                    {
                        huntTypes.AddRange(HuntHelper.ARRHunts);
                    }

                    if (HuntsSettings.Instance.VerteranClanHunts)
                    {
                        huntTypes.AddRange(HuntHelper.VerteranClanHunts);
                    }

                    if (HuntsSettings.Instance.NutClanHunts)
                    {
                        huntTypes.AddRange(HuntHelper.NutClanHunts);
                    }

                    if (huntTypes.Count > 0)
                    {
                        await Hunts.DoHunts(huntTypes.ToArray());
                    }
                    else
                    {
                        Log.Error("Select some hunt types in settings");
                    }

                    break;
                case TaskType.Extract:
                    await Inventory.ExtractFromAllGear();
                    break;
                case TaskType.Coffers:
                    await Inventory.CofferTask();
                    break;
                case TaskType.Housing:
                    await Housing.CheckHousing();
                    break;
                case TaskType.CustomDeliveries:
                    await LlamaLibrary.Utilities.CustomDeliveries.RunCustomDeliveries();
                    break;
                case TaskType.GcTurnin:
                    await GCDailyTurnins.DoGCDailyTurnins();
                    break;
                case TaskType.Retainers:
                    await Retainers.RetainerRun();
                    break;
                case TaskType.FCWorkshop:
                    await FCWorkshop.HandInItems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (BotTask.Type == TaskType.Retainers && RetainerSettings.Instance.Loop)
            {
                return true;
            }

            BotTask.Type = TaskType.None;

            TreeRoot.Stop("Stop Requested");
            return true;
        }

        public static async Task<bool> RemoveMateria(BagSlot bagSlot)
        {
            if (bagSlot != null && bagSlot.IsValid)
            {
                Log.Information($"Want to remove Materia from {bagSlot}");
                var count = bagSlot.MateriaCount();
                for (var i = 0; i < count; i++)
                {
                    Log.Information($"Removing materia {count - i}");
                    bagSlot.RemoveMateria();
                    await Coroutine.Sleep(6000);
                }
            }

            Log.Information($"Item now has {bagSlot.MateriaCount()} materia affixed");

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

            if (!toDesynthList.Any())
            {
                Log.Warning("No items to desynth.");
                return false;
            }

            Log.Information($"# of slots to Desynth: {toDesynthList.Count()}");

            await Inventory.Desynth(toDesynthList);
            return true;
        }

        private static bool ExtraCheck(BagSlot bs)
        {
            if (ReduceSettings.Instance.IncludeFish)
            {
                return (bs.Item.EquipmentCatagory == ItemUiCategory.Seafood && bs.CanDesynthesize);
            }
            return false;
        }
        private static bool ShouldDesynth(string name)
        {
            return DesynthList.Any(name.Contains);
        }
        private static InventoryBagId[] BagsToCheck()
        {
            return ReduceSettings.Instance.IncludeArmory ? Inventory.InventoryBagIds.Concat(Inventory.ArmoryBagIds).ToArray() : Inventory.InventoryBagIds;

            //return inventoryBagIds;
        }

    }
}