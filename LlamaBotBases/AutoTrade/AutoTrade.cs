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
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using TreeSharp;
using Action = TreeSharp.Action;

namespace LlamaBotBases.AutoTrade
{
    public class AutoTrade : BotBase
    {
        private static readonly LLogger Log = new LLogger(typeof(AutoTrade).Name, Colors.Orange);

        private static readonly Queue<QueuedTradeItem> TradeQueue = new Queue<QueuedTradeItem>();

        private Composite _root;
        public override string Name => "AutoTrade";
        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;
        public override Composite Root => _root;
        public override bool WantButton { get; } = true;
        private AutoTradeSettings _settings;

        public override void Initialize()
        {
            OffsetManager.Init();

            // var pmc = new PluginContainer(); pmc.Plugin = new AutoRepair.AutoRepair();
            // pmc.Enabled = true;

            // PluginManager.Plugins.Add(pmc);
        }

        public override void OnButtonPress()
        {
            if (_settings == null || _settings.IsDisposed)
            {
                _settings = new AutoTradeSettings();
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

        private Composite TradeAcceptBehavior => new PrioritySelector(
                    new Decorator(
                        r => Trade.IsOpen,
                        new PrioritySelector(
                            new Decorator(
                                r => SelectYesno.IsOpen && Trade.TradeStage == 5,
                                new Sequence(
                                    new Action(r => Log.Verbose("At Select Yes/No")),
                                    new Sleep(200),
                                    new Action(r => SelectYesno.ClickYes())
                                )
                            ),
                            new Decorator(
                                r => Trade.IsOpen && Trade.TradeStage == 3,
                                new Sequence(
                                    new Action(r => Log.Information($"Trade window open with {Trade.Trader}")),
                                    new Sleep(500),
                                    new Action(r => RaptureAtkUnitManager.GetWindowByName("Trade").SendAction(1, 3uL, 0))
                                )
                            )
                        )
                    )
                );

        public override void Start()
        {
            if (AutoTradeSettings.AcceptTrades)
            {
                Log.Information("Accepting trades...");
                _root = TradeAcceptBehavior;
            }
            else
            {
                _root = new ActionRunCoroutine(r => Run());
            }
        }

        private async Task<bool> Run()
        {
            TradeQueue.Clear();
            var target = GameObjectManager.Target as BattleCharacter;

            if (target == null)
            {
                Log.Warning("No target found to trade to.");
                return false;
            }

            if (target.IsMe)
            {
                Log.Warning("We can't trade with ourselves.");
                return false;
            }

            if (target.Type != GameObjectType.Pc)
            {
                Log.Warning("We can't trade with an NPC.");
                return false;
            }

            if (!target.IsWithinInteractRange)
            {
                Log.Warning("Target is too far away to interact with.");
                return false;
            }

            Log.Information($"Starting to trade with {target.Name}.");

            await QueueTradeItems(AutoTradeSettings.ItemsToTrade);

            await TradeItems(TradeQueue, target);

            if (failedTradeCount >= 5)
            {
                Log.Error("Too many failed trades, exiting.");
            }
            else if (!TradeQueue.Any())
            {
                Log.Information("Done trading!");
            }
            else
            {
                Log.Error("We're done trading, but didn't trip FailedTradeCount and have items still in the queue?");
            }

            if (InputNumeric.IsOpen)
            {
                InputNumeric.Close();
                await Coroutine.Wait(5000, () => !InputNumeric.IsOpen);
            }

            if (Trade.IsOpen)
            {
                Trade.Close();
                await Coroutine.Wait(5000, () => !Trade.IsOpen);
            }

            TradeQueue.TrimExcess();

            TreeRoot.Stop("Finished trading.");
            return true;
        }

        private static int failedTradeCount;
        private static readonly List<WatchedBagSlot> WatchedBagSlots = new List<WatchedBagSlot>();

        private static async Task TradeItems(Queue<QueuedTradeItem> tradeQueue, BattleCharacter target)
        {
            failedTradeCount = 0;
            var gilToTrade = AutoTradeSettings.GilToTrade;
            if (gilToTrade > 0)
            {
                Log.Information($"We want to trade a total of {gilToTrade:N0} gil.");
            }

            Log.Information("---Starting Trades---");
            while ((TradeQueue.Any() || gilToTrade > 0) && failedTradeCount < 3)
            {
                if (!target.IsWithinInteractRange)
                {
                    Log.Error("Our trading partner ran away from us!");
                    break;
                }

                var result = target.OpenTradeWindow();

                if (result != 0)
                {
                    failedTradeCount++;
                    Log.Warning("Couldn't open trade window. Pausing to retry...");
                    await Coroutine.Sleep(3000);
                    continue;
                }

                await Coroutine.Wait(5000, () => Trade.IsOpen);
                if (!Trade.IsOpen)
                {
                    failedTradeCount++;
                    Log.Warning("Trade window never opened. Pausing to retry...");
                    await Coroutine.Sleep(3000);
                    continue;
                }

                Log.Information("Trading window opened.");

                var gilAmount = 0;
                var currentGil = AutoTradeSettings.CurrentGil;

                if (gilToTrade > 0)
                {
                    gilAmount = Math.Min(gilToTrade, 1000000);
                    Log.Information($"Adding {gilAmount:N0} gil.");
                    RaptureAtkUnitManager.GetWindowByName("Trade").SendAction(1, 3, 2);
                    await WaitForInputNumeric((uint)gilAmount);
                    await Coroutine.Sleep(250);
                }

                if (tradeQueue.Any())
                {
                    WatchedBagSlots.Clear();
                    var markedBagSlots = AutoTradeSettings.MainBagsFilledSlots
                        .Where(x => x.CanTrade() && AutoTradeSettings.ItemsToTrade.Any(y => y.TrueItemId == x.TrueItemId))
                        .Select(x => new MarkedBagSlot(x))
                        .ToList();

                    for (var i = 0; i < 5 && tradeQueue.Any(); i++)
                    {
                        var itemToTrade = tradeQueue.Dequeue();

                        var itemSlot = markedBagSlots
                            .Where(x => x.BagSlot.TrueItemId == itemToTrade.TrueItemId && !x.BeingTraded)
                            .OrderByDescending(x => x.BagSlot.Count)
                            .FirstOrDefault();
                        if (itemSlot?.BagSlot == null || !itemSlot.BagSlot.IsFilled)
                        {
                            Log.Error($"Couldn't find any ID: {itemToTrade.TrueItemId} in our inventory.");
                            i--;
                            continue;
                        }

                        itemSlot.BeingTraded = true;
                        WatchedBagSlots.Add(new WatchedBagSlot(itemSlot.BagSlot));
                        Log.Information($"Adding x{itemToTrade.QtyToTrade} {itemToTrade.ItemName} to Slot {i + 1}.");
                        itemSlot.BagSlot.TradeItem();
                        if (itemToTrade.QtyToTrade > 1 && itemToTrade.StackSize > 1)
                        {
                            await WaitForInputNumeric(itemToTrade.QtyToTrade);
                        }

                        await Coroutine.Sleep(250);
                    }
                }

                RaptureAtkUnitManager.GetWindowByName("Trade").SendAction(1, 3uL, 0);
                await Coroutine.Wait(30000, () => Trade.TradeStage == 5);
                if (Trade.TradeStage != 5)
                {
                    Log.Error("Our target still hasn't accepted the trade... aborting.");
                    break;
                }

                await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                if (SelectYesno.IsOpen)
                {
                    SelectYesno.Yes();
                }

                await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
                await Coroutine.Wait(30000, () => Trade.TradeStage == 1);
                if (Trade.TradeStage == 6)
                {
                    Log.Error("Our target still hasn't accepted the trade... aborting.");
                    break;
                }

                Log.Information("Trade completed.");

                // Arbitrary wait to let values update after trade is complete.
                await Coroutine.Sleep(1500);

                if (gilToTrade > 0 && currentGil == AutoTradeSettings.CurrentGil)
                {
                    Log.Error("Trading gil didn't go through.");
                    failedTradeCount++;
                    continue;
                }

                gilToTrade -= gilAmount;
                if (WatchedSlotsUnchanged(WatchedBagSlots))
                {
                    Log.Error("Some items were unchanged, even though they should have been traded.");
                    failedTradeCount++;
                    continue;
                }
            }
        }

        private static async Task WaitForInputNumeric(uint qty)
        {
            await Coroutine.Wait(5000, () => InputNumeric.IsOpen);
            await Coroutine.Sleep(200);
            if (InputNumeric.IsOpen)
            {
                InputNumeric.Ok(qty);
            }
            else
            {
                Log.Error("InputNumeric never opened!");
            }
        }

        private static bool WatchedSlotsUnchanged(IReadOnlyCollection<WatchedBagSlot> watchedSlots)
        {
            if (!watchedSlots.Any())
            {
                return false;
            }

            using (Core.Memory.TemporaryCacheState(false))
            {
                foreach (var toCheck in watchedSlots.Where(toCheck => toCheck.BagSlot != null && toCheck.BagSlot.IsFilled && toCheck.TrueItemId == toCheck.BagSlot.TrueItemId))
                {
                    if (toCheck.Count == toCheck.BagSlot.Count)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static async Task QueueTradeItems(IEnumerable<ItemToTrade> itemsToQueue)
        {
            foreach (var item in itemsToQueue)
            {
                var inventoryItems = AutoTradeSettings.MainBagsFilledSlots.Where(i => i.TrueItemId == item.TrueItemId && i.CanTrade()).ToList();
                if (!inventoryItems.Any())
                {
                    continue;
                }

                if (inventoryItems.Count > inventoryItems.Count(x => x.Count >= x.Item.StackSize))
                {
                    await CombineStacks(item.TrueItemId);
                    await CombineStacks(item.TrueItemId);
                }

                var quotient = Math.DivRem(item.QtyToTrade, item.StackSize, out var remainder);
                if (quotient > 0)
                {
                    for (var i = 0; i < quotient; i++)
                    {
                        TradeQueue.Enqueue(new QueuedTradeItem(item, item.StackSize));
                    }
                }

                if (remainder > 0)
                {
                    TradeQueue.Enqueue(new QueuedTradeItem(item, remainder));
                }

                Log.Information($"Queued x{item.QtyToTrade} {item.ItemName} to trade.");
            }
        }

        public static async Task CombineStacks(uint itemId)
        {
            var slots = AutoTradeSettings.MainBagsFilledSlots.Where(slot => slot.TrueItemId == itemId && slot.Count < slot.Item.StackSize)
                .OrderByDescending(i => i.Count).ToList();

            if (slots.Count > 1)
            {
                var firstSlot = slots.First();
                var restSlots = slots.Skip(1).ToList();
                foreach (var slot in restSlots)
                {
                    var prevCount = firstSlot.Count;
                    var result = slot.Move(firstSlot);
                    if (result)
                    {
                        await Coroutine.Wait(5000, () => prevCount != firstSlot.Count);
                    }

                    await Coroutine.Yield();
                }
            }
        }

        public override void Stop()
        {
            _root = null;
        }

        private class MarkedBagSlot
        {
            internal readonly BagSlot BagSlot;
            internal bool BeingTraded;

            public MarkedBagSlot(BagSlot slot)
            {
                BagSlot = slot;
                BeingTraded = false;
            }
        }

        private struct QueuedTradeItem
        {
            internal readonly uint TrueItemId;
            internal readonly uint QtyToTrade;
            internal readonly uint StackSize;
            internal readonly string ItemName;

            public QueuedTradeItem(ItemToTrade item, int qty)
            {
                TrueItemId = item.TrueItemId;
                QtyToTrade = (uint)qty;
                StackSize = (uint)item.StackSize;
                ItemName = item.ItemName;
            }
        }

        private struct WatchedBagSlot
        {
            internal readonly BagSlot BagSlot;
            internal readonly Item Item;
            internal readonly uint Count;
            internal readonly uint TrueItemId;

            public WatchedBagSlot(BagSlot slot)
            {
                BagSlot = slot;
                Item = slot.Item;
                Count = slot.Count;
                TrueItemId = slot.TrueItemId;
            }
        }
    }
}