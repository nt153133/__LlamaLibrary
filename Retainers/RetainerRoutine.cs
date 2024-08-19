using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaLibrary.Retainers
{
    public static class RetainerRoutine
    {
        private static readonly LLogger Log = new(nameof(RetainerRoutine), Colors.DodgerBlue);

        private static readonly InventoryBagId[] SaddlebagIds =
        {
            (InventoryBagId)0xFA0, (InventoryBagId)0xFA1//, (InventoryBagId) 0x1004,(InventoryBagId) 0x1005
        };

        private static byte _OccupiedSummoningBellCondition = 0;

        public static async Task<bool> ReadRetainers(Task retainerTask)
        {
            if (!await HelperFunctions.OpenRetainerList())
            {
                return false;
            }

            foreach (var retainer in RetainerList.Instance.OrderedRetainerList.Where(i => i.Active))
            {
                Log.Information($"Selecting {retainer.Name}");
                await SelectRetainer(retainer.Unique);

                await retainerTask;

                await DeSelectRetainer();
                Log.Information($"Done with {retainer.Name}");
            }

            if (!await HelperFunctions.CloseRetainerList())
            {
                Log.Error("Could not close retainer list");
                return false;
            }

            return true;
        }

        public static async Task<bool> ReadRetainers(Func<Task> retainerTask)
        {
            if (!await HelperFunctions.OpenRetainerList())
            {
                return false;
            }

            foreach (var retainer in RetainerList.Instance.OrderedRetainerList.Where(i => i.Active))
            {
                Log.Information($"Selecting {retainer.Name}");
                await SelectRetainer(retainer.Unique);

                await retainerTask();

                await DeSelectRetainer();
                Log.Information($"Done with {retainer.Name}");
            }

            if (!await HelperFunctions.CloseRetainerList())
            {
                Log.Error("Could not close retainer list");
                return false;
            }

            return true;
        }

        public static async Task<bool> ReadRetainers(Func<int, Task> retainerTask)
        {
            if (!await HelperFunctions.OpenRetainerList())
            {
                TreeRoot.Stop("Failed: Find a bell or some retainers");
                return false;
            }

            var ordered = RetainerList.Instance.OrderedRetainerList.ToArray();
            var numRetainers = await HelperFunctions.GetNumberOfRetainers();

            for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
            {
                if (!ordered[retainerIndex].Active)
                {
                    continue;
                }

                Log.Information($"Selecting {ordered[retainerIndex].Name}");
                await SelectRetainer(retainerIndex);

                await retainerTask(retainerIndex);

                await DeSelectRetainer();
                Log.Information($"Done with {ordered[retainerIndex].Name}");
            }

            if (!await HelperFunctions.CloseRetainerList())
            {
                Log.Error("Could not close retainer list");
                return false;
            }

            return true;
        }

        public static async Task<List<CompleteRetainer>> ReadRetainers(Func<RetainerInfo, int, Task<CompleteRetainer>> retainerTask)
        {
            var retainers = new List<CompleteRetainer>();
            if (!await HelperFunctions.OpenRetainerList())
            {
                return retainers;
            }

            var count = await HelperFunctions.GetNumberOfRetainers();

            var ordered = RetainerList.Instance.OrderedRetainerList.ToArray();
            var numRetainers = count;

            if (numRetainers <= 0)
            {
                Log.Error("Can't find number of retainers either you have none or not near a bell");
                RetainerList.Instance.Close();
                TreeRoot.Stop("Failed: Find a bell or some retainers");
                return retainers;
            }

            for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
            {
                if (!ordered[retainerIndex].Active)
                {
                    continue;
                }

                Log.Information($"Selecting {ordered[retainerIndex].Name}");
                await SelectRetainer(retainerIndex);

                retainers.Add(await retainerTask(ordered[retainerIndex], retainerIndex));

                await DeSelectRetainer();
                Log.Information($"Done with {ordered[retainerIndex].Name}");
            }

            if (!await HelperFunctions.CloseRetainerList())
            {
                Log.Error("Could not close retainer list");
            }

            return retainers;
        }

        public static async Task CloseRetainers()
        {
            if (RetainerList.Instance.IsOpen)
            {
                VerifyCondition();
                RetainerList.Instance.Close();
                if (!await Coroutine.Wait(15000, () => !RetainerList.Instance.IsOpen && !Core.Me.CheckCondition(_OccupiedSummoningBellCondition)))
                {
                    Log.Error("Could not close retainer list");
                    await DeSelectRetainer();
                    await GeneralFunctions.StopBusy(false);
                    if (RetainerList.Instance.IsOpen)
                    {
                        RetainerList.Instance.Close();
                    }

                    if (!await Coroutine.Wait(15000, () => !RetainerList.Instance.IsOpen && !Core.Me.CheckCondition(_OccupiedSummoningBellCondition)))
                    {
                        Log.Error("Really could not close retainer list");
                    }
                }
            }
        }

        public static async Task<bool> ReadRetainers(Func<RetainerInfo, Task> retainerTask)
        {
            if (!await HelperFunctions.OpenRetainerList())
            {
                //Move this to botbase based on return value
                TreeRoot.Stop("Failed: Find a bell or some retainers");
                return false;
            }

            foreach (var retainer in RetainerList.Instance.OrderedRetainerList.Where(i => i.Active))
            {
                Log.Information($"Selecting {retainer.Name}");
                await SelectRetainer(retainer.Unique);

                await retainerTask(retainer);

                await DeSelectRetainer();
                Log.Information($"Done with {retainer.Name}");
            }

            if (!await HelperFunctions.CloseRetainerList())
            {
                Log.Error("Could not close retainer list");
                return false;
            }

            return true;
        }

        public static async Task DumpItems(bool includeSaddle = false)
        {
            var playerItems = InventoryManager.GetBagsByInventoryBagId(HelperFunctions.PlayerInventoryBagIds).Select(i => i.FilledSlots).SelectMany(x => x).AsParallel()
                .Where(HelperFunctions.FilterStackable);

            var retItems = InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds).Select(i => i.FilledSlots).SelectMany(x => x).AsParallel()
                .Where(HelperFunctions.FilterStackable);

            var sameItems = playerItems.Intersect(retItems, new BagSlotComparer());
            foreach (var slot in sameItems)
            {
                Log.Information($"Want to move {slot}");
                slot.RetainerEntrustQuantity((int)slot.Count);
                await Coroutine.Sleep(100);
            }

            if (includeSaddle)
            {
                await RetrieveFromSaddleBagsRetainer();
            }
        }

        public static async Task RetrieveFromSaddleBagsRetainer()
        {
            if (await InventoryBuddy.Instance.Open())
            {
                Log.Debug("Saddlebags window open");

                var saddleInventory = InventoryManager.GetBagsByInventoryBagId(SaddlebagIds).SelectMany(i => i.FilledSlots);

                var overlap = saddleInventory.Where(i => InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds).SelectMany(k => k.FilledSlots).Any(j => j.TrueItemId == i.TrueItemId && j.Item.StackSize > 1 && j.Count < j.Item.StackSize));
                if (overlap.Any())
                {
                    foreach (var slot in overlap)
                    {
                        var haveSlot = InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds).SelectMany(k => k.FilledSlots).FirstOrDefault(j => j.TrueItemId == slot.TrueItemId && j.Item.StackSize > 1 && j.Count < j.Item.StackSize);

                        if (haveSlot == default(BagSlot))
                        {
                            break;
                        }

                        slot.RetainerEntrustQuantity(Math.Min(haveSlot.Item.StackSize - haveSlot.Count, slot.Count));

                        Log.Information($"(Saddlebag) Entrust {slot.Item.CurrentLocaleName}");

                        await Coroutine.Sleep(500);
                    }
                }

                InventoryBuddy.Instance.Close();
                await Coroutine.Wait(5000, () => !InventoryBuddy.Instance.IsOpen);
                Log.Debug("Saddlebags window closed");
            }
        }

        public static async Task<bool> SelectRetainer(int retainerIndex)
        {
            var list = await HelperFunctions.GetOrderedRetainerArray();

            return await SelectRetainer(list[retainerIndex].Unique);
        }

        public static async Task<bool> SelectRetainer(ulong retainerContentId)
        {
            Log.Information($"Selecting retainer {retainerContentId}");
            if (RetainerList.Instance.IsOpen)
            {
                return await RetainerList.Instance.SelectRetainer(retainerContentId);
            }

            if (RetainerTasks.IsOpen)
            {
                if (HelperFunctions.CurrentRetainer == retainerContentId)
                {
                    return true;
                }

                if (await DeSelectRetainer())
                {
                    return await RetainerList.Instance.SelectRetainer(retainerContentId);
                }
            }

            if (await HelperFunctions.OpenRetainerList())
            {
                return await RetainerList.Instance.SelectRetainer(retainerContentId);
            }

            return false;
        }

        private static void VerifyCondition()
        {
            if (_OccupiedSummoningBellCondition != 0)
            {
                return;
            }

            try
            {
                _OccupiedSummoningBellCondition = Lua.GetReturnVal<byte>("return _G['CmnDefRetainerBell']:GetConditionId();");
            }
            catch
            {
                Log.Error("Could not get occupied summoning bell condition, defaulting to 0x50");
                _OccupiedSummoningBellCondition = 0x50;
            }
        }

        public static async Task<bool> DeSelectRetainer()
        {
            if (!RetainerTasks.IsOpen)
            {
                return true;
            }

            VerifyCondition();

            RetainerTasks.CloseTasks();

            if (!await Coroutine.Wait(5000, () => !RetainerTasks.IsOpen ))
            {
                Log.Error("Could not close retainer task window");
            }

            if (!await Coroutine.Wait(13000, () => DialogOpen || SelectYesno.IsOpen))
            {
                Log.Error("Could not find dialog or select yesno");
            }

            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
                await Coroutine.Wait(13000, () => DialogOpen || RetainerList.Instance.IsOpen  || !Core.Me.CheckCondition(_OccupiedSummoningBellCondition));
            }

            while (!RetainerList.Instance.IsOpen)
            {
                if (!Core.Me.CheckCondition(_OccupiedSummoningBellCondition))
                {
                    break;
                }

                if (DialogOpen)
                {
                    Next();
                    await Coroutine.Sleep(100);
                }

                await Coroutine.Wait(13000, () => DialogOpen || RetainerList.Instance.IsOpen || !Core.Me.CheckCondition(_OccupiedSummoningBellCondition));
            }

            return RetainerList.Instance.IsOpen;
        }

        public static async Task<bool> RetainerVentureCheck(RetainerInfo retainer)
        {
            if (retainer.Job != ClassJobType.Adventurer)
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
                Log.Verbose("Venture Done");
                SelectString.ClickLineEquals(Translator.VentureCompleteText);

                await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen);

                if (!RetainerTaskResult.IsOpen)
                {
                    Log.Error("RetainerTaskResult didn't open");
                    return false;
                }

                var taskId = AgentRetainerVenture.Instance.RetainerTask;

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

        private class BagSlotComparer : IEqualityComparer<BagSlot>
        {
            public bool Equals(BagSlot x, BagSlot y)
            {
                return x.RawItemId == y.RawItemId && x.Count + y.Count < x.Item.StackSize;
            }

            public int GetHashCode(BagSlot obj)
            {
                return obj.Item.GetHashCode();
            }
        }
    }
}