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
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using Newtonsoft.Json;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaBotBases.Retainers
{
    public class Retainers : BotBase
    {
        private static readonly LLogger Log = new LLogger(BotName, Colors.Green);

        private static readonly string BotName = "Retainers Organize";

        private static readonly InventoryBagId[] InventoryBagId_0 = new InventoryBagId[6]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4,
            InventoryBagId.Bag5,
            InventoryBagId.Bag6
        };

        private static bool done;

        private Composite _root;

        private bool debug;

        private SettingsForm settings;

        public Retainers()
        {
            OffsetManager.Init();

            VenturesInit();
            Log.Information("INIT DONE");
        }

        public override string Name =>
#if RB_CN
                return "雇员整理";
#else
                BotName;
#endif

        public override bool WantButton => true;

        public override string EnglishName => "Retainers Organize";

        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        internal static Lazy<List<RetainerTaskData>> VentureData;
        private int ventures;

        internal void VenturesInit()
        {
            Log.Information("Load venture.json");
            VentureData = new Lazy<List<RetainerTaskData>>(() => LoadResource<List<RetainerTaskData>>(LlamaLibrary.Properties.Resources.Ventures));
            Log.Information("Loaded venture.json");
        }

        private static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public override void Initialize()
        {
        }

        public override void OnButtonPress()
        {
            if (settings == null || settings.IsDisposed)
            {
                settings = new SettingsForm();
            }

            try
            {
                settings.Show();
                settings.Activate();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            _root = new ActionRunCoroutine(r => RetainerTest());
            done = false;
        }

        /*The await sleeps shouldn't be necessary but if they aren't there the game crashes some times since
        it tries to send commands to a window that isn't open even though it reports it as open (guess it didn't load yet)*/

        private async Task<bool> RetainerTest()
        {
            if (done)
            {
                return true;
            }

            Log.Information(" ");
            Log.Information("==================================================");
            Log.Information("====================Retainers=====================");
            Log.Information("==================================================");
            Log.Information(" ");

            var count = await HelperFunctions.GetNumberOfRetainers();
            var rets = Core.Memory.ReadArray<RetainerInfo>(Offsets.RetainerData, count);

            //var retainerIndex = 0;

            //Settings variables
            debug = RetainerSettings.Instance.DebugLogging;
            var bell = await HelperFunctions.GoToSummoningBell();

            if (bell == false)
            {
                Log.Error("No summoning bell near by");
                TreeRoot.Stop("Done playing with retainers");
                return false;
            }

            await HelperFunctions.UseSummoningBell();
            await Coroutine.Wait(5000, () => RetainerList.Instance.IsOpen);

            if (!RetainerList.Instance.IsOpen)
            {
                Log.Error("Can't Open Bell");
                TreeRoot.Stop("Done playing with retainers");
                return false;
            }

            if (SelectString.IsOpen)
            {
                await RetainerRoutine.DeSelectRetainer();
            }

            var ordered = RetainerList.Instance.OrderedRetainerList.ToArray();
            var numRetainers = ordered.Where(i => i.Active).Count(); //GetNumberOfRetainers();

            var retList = new List<RetainerInventory>();
            var moveToOrder = new List<KeyValuePair<uint, int>>();
            var masterInventory = new Dictionary<uint, List<KeyValuePair<int, uint>>>();

            var retainerNames = new Dictionary<int, string>();

            if (numRetainers <= 0)
            {
                Log.Error("Can't find number of retainers either you have none or not near a bell");
                RetainerList.Instance.Close();

                TreeRoot.Stop("Failed: Find a bell or some retainers");
                return true;
            }

            //Moves
            var moveFrom = new List<uint>[numRetainers];

            for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
            {
                moveFrom[retainerIndex] = new List<uint>();
            }

            ventures = RetainerList.Instance.NumberOfVentures;

            for (var retainerIndex = 0; retainerIndex < ordered.Length; retainerIndex++)
            {
                if (!ordered[retainerIndex].Active)
                {
                    continue;
                }

                if (!retainerNames.ContainsKey(retainerIndex))
                {
                    retainerNames.Add(retainerIndex, RetainerList.Instance.RetainerName(retainerIndex));
                }

                var hasJob = RetainerList.Instance.RetainerHasJob(retainerIndex);
                Log.Information($"Selecting {RetainerList.Instance.RetainerName(retainerIndex)}");
                await RetainerRoutine.SelectRetainer(retainerIndex);

                var inventory = new RetainerInventory();

                if (RetainerSettings.Instance.GetGil)
                {
                    HelperFunctions.GetRetainerGil();
                }

                Log.Verbose("Inventory open");
                foreach (var item in InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds).Select(i => i.FilledSlots).SelectMany(x => x).Where(HelperFunctions.FilterStackable))
                {
                    try
                    {
                        inventory.AddItem(item);
                        if (masterInventory.ContainsKey(item.TrueItemId))
                        {
                            masterInventory[item.TrueItemId]
                                .Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                        }
                        else
                        {
                            masterInventory.Add(item.TrueItemId, new List<KeyValuePair<int, uint>>());
                            masterInventory[item.TrueItemId]
                                .Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("SHIT:" + e);
                        throw;
                    }
                }

                Log.Verbose("Inventory done");

                Log.Information($"Checking retainer[{retainerNames[retainerIndex]}] against player inventory");

                if (RetainerSettings.Instance.DepositFromPlayer)
                {
                    await RetainerRoutine.DumpItems();
                }

                Log.Information("Done checking against player inventory");

                if (RetainerSettings.Instance.ReassignVentures && (ordered[retainerIndex].Job != ClassJobType.Adventurer) && ventures > 2 && (ordered[retainerIndex].VentureEndTimestamp - HelperFunctions.UnixTimestamp) <= 0)
                {
                    Log.Information("Checking Ventures");
                    await RetainerHandleVentures(); //CheckVentures();
                }
                else if ((ordered[retainerIndex].VentureEndTimestamp - HelperFunctions.UnixTimestamp) > 0)
                {
                    Log.Information($"Venture will be done in {(ordered[retainerIndex].VentureEndTimestamp - HelperFunctions.UnixTimestamp) / 60} minutes");
                }
                else
                {
                    Log.Warning("Retainer has no job");
                }

                await RetainerRoutine.DeSelectRetainer();

                Log.Verbose("Should be back at retainer list by now");

                // await Coroutine.Sleep(200);
                // }

                retList.Add(inventory);
            }

            //await Coroutine.Sleep(1000);

            if (RetainerSettings.Instance.DontOrganizeRetainers || !RetainerSettings.Instance.DepositFromPlayer)
            {
                RetainerList.Instance.Close();

                TreeRoot.Stop("Done playing with retainers (Don't organize or don't deposit items.)");
                return true;
            }

            if (debug)
            {
                foreach (var itemId in masterInventory)
                {
                    var retainers = "";

                    foreach (var retainerId in itemId.Value)
                    {
                        retainers += $"Retainer[{retainerNames[retainerId.Key]}] has {retainerId.Value} ";
                    }

                    Log.Information($"Item {itemId.Key}: {retainers}");
                }
            }

            Log.Error("Duplicate items Found:");

            if (debug)
            {
                foreach (var itemId in masterInventory.Where(r => r.Value.Count > 1))
                {
                    var retainers = "";
                    var retListInv = new List<KeyValuePair<int, uint>>(itemId.Value.OrderByDescending(r => r.Value));

                    foreach (var retainerId in retListInv)
                    {
                        retainers += $"Retainer[{retainerNames[retainerId.Key]}] has {retainerId.Value} ";
                    }

                    Log.Debug($"Item {itemId.Key}: {retainers}");
                }
            }

            /*
                 * Same as above but before the second foreach save retainer/count
                 * remove that one since it's where we're going to move stuff to
                 */
            var numOfMoves = 0;

            foreach (var itemId in masterInventory.Where(r => r.Value.Count > 1))
            {
                var retListInv = new List<KeyValuePair<int, uint>>(itemId.Value.OrderByDescending(r => r.Value));

                var retainerTemp = retListInv[0].Key;
                var countTemp = retListInv[0].Value;

                var retainers = "";

                retListInv.RemoveAt(0);

                foreach (var retainerId in retListInv)
                {
                    retainers += $"Retainer[{retainerNames[retainerId.Key]}] has {retainerId.Value} ";
                    countTemp += retainerId.Value;
                }

                Log.Information($"Item: {DataManager.GetItem(HelperFunctions.NormalRawId(itemId.Key))} ({itemId.Key}) Total:{countTemp} should be in {retainerNames[retainerTemp]} and {retainers}");

                if (countTemp > 999)
                {
                    Log.Error($"This item will have a stack size over 999: {itemId.Key}");
                }
                else if (numOfMoves < InventoryManager.FreeSlots - 1)
                {
                    numOfMoves++;
                    foreach (var retainerIdTemp in retListInv)
                    {
                        moveFrom[retainerIdTemp.Key].Add(itemId.Key);
                    }
                }
            }

            Log.Information($"Looks like we need to do {numOfMoves} moves");

            if (numOfMoves < InventoryManager.FreeSlots && numOfMoves > 0)
            {
                Log.Information($"Looks like we have {InventoryManager.FreeSlots} free spaces in inventory so we can just dump into player inventory");

                //First loop
                for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                {
                    var inventory = new RetainerInventory();

                    if (!RetainerList.Instance.IsOpen)
                    {
                        await HelperFunctions.UseSummoningBell();
                        await Coroutine.Wait(5000, () => RetainerList.Instance.IsOpen);

                        //await Coroutine.Sleep(1000);
                    }

                    if (!RetainerList.Instance.IsOpen)
                    {
                        Log.Error("Failed opening retainer list");
                    }

                    Log.Verbose("Open:" + RetainerList.Instance.IsOpen);

                    await RetainerList.Instance.SelectRetainer(retainerIndex);

                    Log.Information($"Selected Retainer: {retainerNames[retainerIndex]}");

                    await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                    RetainerTasks.OpenInventory();

                    await Coroutine.Wait(5000, RetainerTasks.IsInventoryOpen);

                    if (!RetainerTasks.IsInventoryOpen())
                    {
                        continue;
                    }

                    await Coroutine.Sleep(500);

                    Log.Information($"Checking retainer[{retainerNames[retainerIndex]}] against move list");

                    foreach (var item in moveFrom[retainerIndex])
                    {
                        if (!InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds).Select(i => i.FilledSlots).SelectMany(x => x).Any(i => i.TrueItemId == item))
                        {
                            continue;
                        }

                        Log.Information("Moved: " + InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds).Select(i => i.FilledSlots).SelectMany(x => x).First(i => i.TrueItemId == item)
                                .Move(InventoryManager.GetBagsByInventoryBagId(InventoryBagId_0).First(bag => bag.FreeSlots > 0).GetFirstFreeSlot()));
                        await Coroutine.Sleep(200);
                    }

                    Log.Information("Done checking against player inventory");

                    RetainerTasks.CloseInventory();

                    await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);

                    RetainerTasks.CloseTasks();

                    await Coroutine.Wait(3000, () => DialogOpen);

                    if (DialogOpen)
                    {
                        Next();
                    }

                    await Coroutine.Wait(3000, () => RetainerList.Instance.IsOpen);

                    Log.Verbose("Should be back at retainer list by now");
                }
            }
            else
            {
                if (numOfMoves <= 0)
                {
                    Log.Information("No duplicate stacks found so no moved needed.");
                    RetainerList.Instance.Close();

                    TreeRoot.Stop("Done playing with retainers");
                    return true;
                }

                Log.Error("Crap, we don't have enough player inventory to dump it all here");
                RetainerList.Instance.Close();

                TreeRoot.Stop("Done playing with retainers");
                return false;
            }

            for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
            {
                Log.Information($"Selecting {RetainerList.Instance.RetainerName(retainerIndex)}");
                await RetainerRoutine.SelectRetainer(retainerIndex);

                await RetainerRoutine.DumpItems();

                await RetainerRoutine.DeSelectRetainer();
                Log.Information($"Done with {RetainerList.Instance.RetainerName(retainerIndex)}");
            }

            //await RetainerRoutine.ReadRetainers(RetainerRoutine.DumpItems());

            Log.Verbose("Closing Retainer List");

            RetainerList.Instance.Close();

            TreeRoot.Stop("Done playing with retainers");

            done = true;

            return true;
        }

        public async Task<bool> CheckVentures()
        {
            if (!SelectString.IsOpen)
            {
                return false;
            }

            if (SelectString.LineCount > 9)
            {
                if (SelectString.Lines().Contains(Translator.VentureCompleteText))
                {
                    Log.Information("Venture Done");
                    SelectString.ClickLineEquals(Translator.VentureCompleteText);

                    await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen);

                    if (!RetainerTaskResult.IsOpen)
                    {
                        Log.Error("RetainerTaskResult didn't open");
                        return false;
                    }

                    var taskId = AgentRetainerVenture.Instance.RetainerTask;

                    var task = VentureData.Value.FirstOrDefault(i => i.Id == taskId);

                    if (task != default(RetainerTaskData))
                    {
                        Log.Information($"Finished Venture {task.Name}");
                        Log.Information($"Reassigning Venture {task.Name}");
                    }
                    else
                    {
                        Log.Information($"Finished Venture");
                        Log.Information($"Reassigning Venture");
                    }

                    RetainerTaskResult.Reassign();

                    await Coroutine.Wait(5000, () => RetainerTaskAsk.IsOpen);
                    if (!RetainerTaskAsk.IsOpen)
                    {
                        Log.Error("RetainerTaskAsk didn't open");
                        return false;
                    }

                    await Coroutine.Wait(2000, RetainerTaskAskExtensions.CanAssign);
                    if (RetainerTaskAskExtensions.CanAssign())
                    {
                        RetainerTaskAsk.Confirm();
                        ventures -= task.VentureCost;
                        Log.Information($"Should be down to {ventures} venture tokens");
                    }
                    else
                    {
                        Log.Error($"RetainerTaskAsk Error: {RetainerTaskAskExtensions.GetErrorReason()}");
                        RetainerTaskAsk.Close();
                    }

                    await Coroutine.Wait(1500, () => DialogOpen);
                    await Coroutine.Sleep(200);
                    if (DialogOpen)
                    {
                        Next();
                    }

                    await Coroutine.Sleep(200);
                    await Coroutine.Wait(5000, () => SelectString.IsOpen);
                }
                else
                {
                    Log.Warning("Venture Not Done");
                }
            }
            else
            {
                Log.Warning("Venture Not Done");
            }

            return true;
        }

        public async Task<bool> RetainerHandleVentures()
        {
            if (!SelectString.IsOpen)
            {
                return false;
            }

            if (SelectString.Lines().Contains(Translator.VentureCompleteText))
            {
                //Log.Information("Venture Done");
                SelectString.ClickLineEquals(Translator.VentureCompleteText);

                await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen);

                if (!RetainerTaskResult.IsOpen)
                {
                    Log.Error("RetainerTaskResult didn't open");
                    return false;
                }

                var taskId = AgentRetainerVenture.Instance.RetainerTask;

                var task = VentureData.Value.FirstOrDefault(i => i.Id == taskId);

                if (task != default(RetainerTaskData))
                {
                    Log.Information($"Finished Venture {task.Name}");
                    Log.Information($"Reassigning Venture {task.Name}");
                }
                else
                {
                    Log.Information($"Finished Venture");
                    Log.Information($"Reassigning Venture");
                }

                RetainerTaskResult.Reassign();

                await Coroutine.Wait(5000, () => RetainerTaskAsk.IsOpen);
                if (!RetainerTaskAsk.IsOpen)
                {
                    Log.Error("RetainerTaskAsk didn't open");
                    return false;
                }

                await Coroutine.Wait(2000, RetainerTaskAskExtensions.CanAssign);
                if (RetainerTaskAskExtensions.CanAssign())
                {
                    RetainerTaskAsk.Confirm();
                }
                else
                {
                    Log.Error($"RetainerTaskAsk Error: {RetainerTaskAskExtensions.GetErrorReason()}");
                    RetainerTaskAsk.Close();
                }

                await Coroutine.Wait(1500, () => DialogOpen || SelectString.IsOpen);
                await Coroutine.Sleep(200);
                if (DialogOpen)
                {
                    Next();
                }

                await Coroutine.Sleep(200);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }
            else
            {
                Log.Warning("Venture Not Done");
            }

            return true;
        }
    }
}