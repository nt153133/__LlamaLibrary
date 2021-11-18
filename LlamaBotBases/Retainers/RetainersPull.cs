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
using LlamaLibrary;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using Newtonsoft.Json;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;
using static LlamaLibrary.Retainers.HelperFunctions;

namespace LlamaBotBases.Retainers
{
    public class RetainersPull : BotBase
    {
        private static readonly LLogger Log = new LLogger(BotName, Colors.MediumSlateBlue);

        private static readonly string BotName = "Retainers";

        private Composite _root;

        private SettingsForm _settings;

        public RetainersPull()
        {
            Init();
        }

        public override string Name =>
#if RB_CN
                return "雇员拉";
#else
                "Retainers";
#endif

        public override bool WantButton => true;

        public override string EnglishName => "Retainers";

        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        public override void Initialize()
        {
        }

        public override void OnButtonPress()
        {
            if (_settings == null || _settings.IsDisposed)
            {
                _settings = new SettingsForm();
            }

            try
            {
                _settings.Show();
                _settings.Activate();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        private void Init()
        {
            OffsetManager.Init();

            Log.Information("Load venture.json");

            Log.Information("Loaded venture.json");
        }

        private static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            _root = new ActionRunCoroutine(r => RetainerTest());
        }

        private async Task<bool> RetainerTest()
        {
            Log.Information("====================Retainers=====================");

            await RetainerRun();

            return true;
        }

        public static async Task CheckVentureTask()
        {
            var verified = await VerifiedRetainerData();
            if (!verified)
            {
                return;
            }

            var count = await HelperFunctions.GetNumberOfRetainers();
            var rets = Core.Memory.ReadArray<RetainerInfo>(Offsets.RetainerData, count);
            var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            if (rets.Any(i => i.Active && i.VentureTask != 0 && (i.VentureEndTimestamp - now) <= 0 && SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) > 2))
            {
                await GeneralFunctions.StopBusy(dismount: false);

                if (DutyManager.InInstance || CraftingLog.IsOpen || FishingManager.State != FishingState.None || MovementManager.IsOccupied || CraftingManager.IsCrafting)
                {
                    Log.Error("Something went wrong: character is busy or in duty");
                    return;
                }

                var bell = await GoToSummoningBell();

                if (bell == false)
                {
                    Log.Error("No summoning bell near by");
                    return;
                }

                await RetainerRoutine.ReadRetainers(RetainerCheckOnlyVenture);
            }
            else
            {
                Log.Information("No Ventures Complete");
            }
        }

        public async Task RetainerRun()
        {
            var bell = await GoToSummoningBell();

            if (bell == false)
            {
                Log.Error("No summoning bell near by");
                TreeRoot.Stop("Done playing with retainers");
                return;
            }

            await RetainerRoutine.ReadRetainers(RetainerCheck);
            await Coroutine.Sleep(1000);
            if (!RetainerSettings.Instance.Loop || !RetainerSettings.Instance.ReassignVentures)
            {
                Log.Information($"Loop Setting {RetainerSettings.Instance.Loop} ReassignVentures {RetainerSettings.Instance.ReassignVentures}");
                TreeRoot.Stop("Done playing with retainers");
            }

            if (RetainerSettings.Instance.Loop && InventoryManager.FreeSlots < 2)
            {
                Log.Error($"I am overburdened....free up some space you hoarder");
                TreeRoot.Stop("Done playing with retainers");
            }

            var count = await GetNumberOfRetainers();
            var rets = Core.Memory.ReadArray<RetainerInfo>(Offsets.RetainerData, count);

            if (!rets.Any(i => i.VentureTask != 0 && i.Active))
            {
                Log.Warning($"No ventures assigned or completed");
                TreeRoot.Stop("Done playing with retainers");
            }

            var nextVenture = rets.Where(i => i.VentureTask != 0 && i.Active).OrderBy(i => i.VentureEndTimestamp).First();
            if (nextVenture.VentureEndTimestamp == 0)
            {
                Log.Warning($"No ventures running");
                TreeRoot.Stop("Done playing with retainers");
            }

            if (SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) <= 2)
            {
                Log.Error($"Get more venture tokens...bum");
                TreeRoot.Stop("Done playing with retainers");
            }

            var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var timeLeft = nextVenture.VentureEndTimestamp - now;

            Log.Information($"Waiting till {RetainerInfo.UnixTimeStampToDateTime(nextVenture.VentureEndTimestamp)}");
            await Coroutine.Sleep(timeLeft * 1000);
            await Coroutine.Sleep(30000);
            Log.Information($"{nextVenture.Name} Venture should be done");
        }

        public async Task<bool> RetainerCheck(RetainerInfo retainer)
        {
            if (RetainerSettings.Instance.ReassignVentures && retainer.Active && retainer.Job != ClassJobType.Adventurer)
            {
                if (retainer.VentureTask != 0)
                {
                    var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    var timeLeft = retainer.VentureEndTimestamp - now;

                    if (timeLeft <= 0 && SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) > 2)
                    {
                        await RetainerHandleVentures();
                    }
                    else
                    {
                        Log.Information($"Venture will be done at {RetainerInfo.UnixTimeStampToDateTime(retainer.VentureEndTimestamp)}");
                    }
                }
            }

            if (RetainerSettings.Instance.DepositFromPlayer)
            {
                await RetainerRoutine.DumpItems(RetainerSettings.Instance.DepositFromSaddleBags);
            }

            if (RetainerSettings.Instance.GetGil)
            {
                GetRetainerGil();
            }

            return true;
        }

        public static async Task<bool> RetainerCheckOnlyVenture(RetainerInfo retainer)
        {
            if (retainer.VentureTask != 0)
            {
                var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var timeLeft = retainer.VentureEndTimestamp - now;

                if (timeLeft <= 0 && SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) > 2)
                {
                    await RetainerHandleVentures();
                }
                else
                {
                    Log.Information($"Venture will be done at {RetainerInfo.UnixTimeStampToDateTime(retainer.VentureEndTimestamp)}");
                }
            }

            return true;
        }

        public static async Task<bool> RetainerHandleVentures()
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

                var task = ResourceManager.VentureData.Value.FirstOrDefault(i => i.Id == taskId);

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
                Log.Information("Venture Not Done");
            }

            return true;
        }
    }
}