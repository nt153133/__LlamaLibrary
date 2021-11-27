using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaBotBases.LlamaUtilities.Settings;
using LlamaLibrary.Logging;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using static ff14bot.RemoteWindows.Talk;
using static LlamaLibrary.Retainers.HelperFunctions;


namespace LlamaBotBases.LlamaUtilities
{
    public static class Retainers
    {
        private static readonly LLogger Log = new LLogger(BotName, Colors.MediumSlateBlue);

        private static readonly string BotName = "Retainers";

        public static async Task RetainerRun()
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

            var rets = await GetOrderedRetainerArray(true);

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

        public static async Task<bool> RetainerCheck(RetainerInfo retainer)
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
    }
}