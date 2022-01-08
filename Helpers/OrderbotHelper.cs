using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.BotBases;
using ff14bot.Managers;
using ff14bot.NeoProfile;
using ff14bot.NeoProfiles;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    public class OrderbotHelper
    {
        private static readonly LLogger Log = new LLogger("OrderbotHelper", Colors.MediumPurple);
        public static bool loaded;
        public static bool tempbool;

        public static Task<bool> CallOrderbot(string profile)
        {
            var a = new Thread(() => MyThread(profile));
            a.Start();
            return Task.FromResult(true);
        }

        private static async void MyThread(string profile)
        {
            Log.Information("Thread Started");
            tempbool = false;
            //var profile = NeoProfileManager.CurrentProfile.Path;

            var lastBot = BotManager.Current;
            BotManager.Current.Stop();
            await TreeRoot.StopGently();

            await WaitUntil(() => !TreeRoot.IsRunning, timeout: 20000);

            if (!TreeRoot.IsRunning)
            {
                Log.Information($"Bot Stopped: {lastBot.Name}");
                BotEvents.NeoProfile.OnNewProfileLoaded += OnNewProfileLoaded;
                BotManager.SetCurrent(BotManager.Bots.First(i => i.EnglishName.Contains("Order Bot")));
                //NeoProfileManager.Load(profile);
                //NeoProfileManager.UpdateCurrentProfileBehavior();

                //await WaitUntil(() => loaded, timeout: 20000);
                loaded = false;
                BotManager.Current.Initialize();
                NeoProfileManager.Load(profile);
                //NeoProfileManager.UpdateCurrentProfileBehavior();

                //BotManager.Current.Start();
                TreeRoot.Start();

            }
            else
            {
                Log.Information($"Failed Stopping bot");
            }

            await WaitUntil(() => TreeRoot.IsRunning, timeout: 20000);
            if (TreeRoot.IsRunning)
            {
                Log.Information($"Orderbot Started");

                await WaitWhile(() => TreeRoot.IsRunning, 500);

                if (!TreeRoot.IsRunning)
                {
                    if (lastBot != null)
                    {
                        Log.Information($"Restarting {lastBot.Name}");
                        BotManager.SetCurrent(lastBot);
                        Thread.Sleep(1000);

                        tempbool = false;

                        TreeRoot.OnStart += OnBotStart;
                        BotManager.Current.Initialize();
                        BotManager.Current.Start();
                        //NeoProfileManager.Load(profile);

                        TreeRoot.Start();
                        await WaitUntil(() => tempbool, timeout: 20000);
                        TreeRoot.OnStart -= OnBotStart;
                    }
                    else
                    {
                        Log.Information($"LastBot Null: Starting Orderbot");
                        BotManager.SetCurrent(new OrderBot());
                        BotManager.Current.Start();
                    }
                }
            }
            else
            {
                Log.Information($"Failed To Start Orderbot");
            }

            Log.Information($"Thread Stopped");

            //_isDone = true;
        }

        private static void OnNewProfileLoaded(BotEvents.NeoProfile.NewProfileLoadedEventArgs args)
        {
            loaded = true;
        }

        private static void OnBotStart(BotBase bot)
        {
            Log.Information($"{bot.Name} Started");
            tempbool = true;
        }

        /// <summary>
        ///     Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        ///     Blocks while condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="frequency">The frequency at which the condition will be check, in milliseconds.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }
    }
}