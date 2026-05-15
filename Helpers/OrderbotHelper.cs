using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Forms.ugh;
using ff14bot.Managers;
using ff14bot.NeoProfile;
using ff14bot.NeoProfiles;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helpers for switching the active botbase to the Order Bot, loading a profile,
    /// waiting for it to run to completion, and then restoring the original botbase.
    /// </summary>
    public class OrderbotHelper
    {
        private static readonly LLogger Log = new(nameof(OrderbotHelper), Colors.MediumPurple);

        /// <summary>Tracks whether the Order Bot profile has been loaded (used by the background thread).</summary>
        public static bool loaded;

        /// <summary>Temporary flag set by the botbase-start event handler to signal that the restored botbase has started.</summary>
        public static bool tempbool;

        /// <summary>
        /// Set to <c>true</c> when the user clicks the RebornBuddy stop button, preventing the original botbase
        /// from being restarted after the Order Bot finishes.
        /// </summary>
        public static bool StopBot;

        /// <summary>
        /// Retrieves the RebornBuddy main-window Start/Stop button via reflection.
        /// Returns <c>null</c> when the field cannot be found.
        /// </summary>
        public static Button? RbStartButton => typeof(MainWpf).GetField("btnStart", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(MainWpf.current) as Button;

        /// <summary>
        /// Starts a background thread that stops the current botbase, switches to the Order Bot,
        /// loads <paramref name="profile"/>, waits for it to finish, and then restores the previous botbase.
        /// </summary>
        /// <param name="profile">Path to the NeoProfile to load in the Order Bot.</param>
        /// <returns>A completed task with value <c>true</c> once the thread has been started.</returns>
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
                // ignored
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
                Log.Information("Botbase set to orderbot");

                NeoProfileManager.Load(profile);
                Log.Information("Profile loaded");

                //NeoProfileManager.UpdateCurrentProfileBehavior();

                //BotManager.Current.Start();
                TreeRoot.Start();
            }
            else
            {
                Log.Information("Failed Stopping bot");
            }

            await WaitUntil(() => TreeRoot.IsRunning, timeout: 20000);
            if (TreeRoot.IsRunning)
            {
                Log.Information("Orderbot Started");

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

                        if (StopBot)
                        {
                            Log.Information("Since we stopped the bot with the button, we will not restart it");
                            return;
                        }


                        //NeoProfileManager.Load(profile);

                        TreeRoot.Start();
                        try
                        {
                            await WaitUntil(() => tempbool, timeout: 20000);
                        }
                        catch
                        {
                            // ignored
                        }

                        TreeRoot.OnStart -= OnBotStart;
                    }
                    //Log.Information($"LastBot Null: Starting Orderbot");
                    //BotManager.SetCurrent(new OrderBot());
                    //BotManager.Current.Start();
                }
            }
            else
            {
                Log.Information("Failed To Start Orderbot");
            }

            Log.Information("Thread Stopped");

            //_isDone = true;
        }

        private static void OnNewProfileLoaded(BotEvents.NeoProfile.NewProfileLoadedEventArgs args)
        {
            loaded = true;
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            Log.Information("Someone hit the stop button, catching so we don't restart");
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