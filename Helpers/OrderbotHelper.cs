using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Managers;
using ff14bot.NeoProfile;
using ff14bot.NeoProfiles;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    public class OrderbotHelper
    {
        private static readonly LLogger Log = new(nameof(OrderbotHelper), Colors.MediumPurple);

        public static bool loaded;
        public static bool tempbool;
        public static bool StopBot = false;
        public static Button? RbStartButton => typeof(ff14bot.Forms.ugh.MainWpf).GetField("btnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(ff14bot.Forms.ugh.MainWpf.current) as Button;

        public static Task<bool> CallOrderbot(string profile)
        {
            StopBot = false;
            var a = new Thread(() => MyThread(profile));
            a.Start();
            return Task.FromResult(true);
        }

        private static async void MyThread(string profile)
        {
            Log.Information("Thread Started");
            tempbool = false;
            if (RbStartButton == null)
            {
                Log.Error("RbStartButton is null");
                return;
            }

            RbStartButton.Click += OnClick;

            //var profile = NeoProfileManager.CurrentProfile.Path;

            var lastBot = BotManager.Current;
            Log.Information($"Current Bot: {lastBot.Name} {TreeRoot.IsRunning}");
            TreeRoot.Stop("Orderbot helper called stop");

            //BotManager.Current.Stop();
            //await TreeRoot.StopGently();
            await Task.Delay(2000);
            //Thread.Sleep(2000);
            try
            {
                await WaitUntil(() => !TreeRoot.IsRunning, timeout: 20000);
            }
            catch
            {
            }

            if (!TreeRoot.IsRunning)
            {
                Log.Information($"Bot Stopped: {lastBot.Name}");
                BotEvents.NeoProfile.OnNewProfileLoaded += OnNewProfileLoaded;
                BotManager.SetCurrent(BotManager.Bots.First(i => i.EnglishName.Contains("Order Bot")));

                //NeoProfileManager.Load(profile);
                //NeoProfileManager.UpdateCurrentProfileBehavior();

                //await WaitUntil(() => loaded, timeout: 20000);
                loaded = false;
                Log.Information($"Botbase set to orderbot");
                BotManager.Current.Initialize();
                Log.Information($"Initialize");
                NeoProfileManager.Load(profile);
                Log.Information($"Profile loaded");

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
                        await Task.Delay(2000);
                        //Thread.Sleep(1000);

                        tempbool = false;

                        TreeRoot.OnStart += OnBotStart;
                        BotManager.Current.Initialize();

                        if (StopBot)
                        {
                            Log.Information("Since we stopped the bot with the button, we will not restart it");
                            return;
                        }

                        BotManager.Current.Start();

                        //NeoProfileManager.Load(profile);

                        TreeRoot.Start();
                        try
                        {
                            await WaitUntil(() => tempbool, timeout: 20000);
                        }
                        catch
                        {
                        }

                        TreeRoot.OnStart -= OnBotStart;
                    }
                    else
                    {
                        //Log.Information($"LastBot Null: Starting Orderbot");
                        //BotManager.SetCurrent(new OrderBot());
                        //BotManager.Current.Start();
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

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            Log.Information($"Someone hit the stop button, catching so we don't restart");
            StopBot = true;
            if (RbStartButton != null)
            {
                RbStartButton.Click -= OnClick;
            }
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