using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using Newtonsoft.Json;

namespace LlamaLibrary.Utilities
{
    /// <summary>
    /// Provides automation for Grand Company daily supply and provisioning missions.
    /// Handles item identification, calling <see cref="Lisbeth"/> for crafting/gathering, and performing the hand-in.
    /// </summary>
    public static class GCDailyTurnins
    {
        /// <summary>
        /// Gets the static display name for this utility.
        /// </summary>
        public static string NameStatic => "GCDailyTurnins";

        private static readonly LLogger Log = new(NameStatic, Colors.Gold);

        /// <summary>
        /// Automatically performs all available Grand Company daily turn-ins.
        /// Identifies missing items, requests them from <see cref="Lisbeth"/>, and manages the hand-in interaction with the Personnel Officer.
        /// </summary>
        public static async Task DoGCDailyTurnins()
        {
            var items = Core.Memory.ReadArray<GCTurninItem>(Offsets.GCTurnin, Offsets.GCTurninCount);

            /*
            foreach (var item in items)
            {
                Log.Information(item.ItemID);
            }*/

            if (!items.Any(i => i.CanHandin))
            {
                Log.Information("All done.");
                return;
            }

            var lisbethOrder = await GetGCSupplyList();

            if (lisbethOrder == "")
            {
                Log.Information("Not Calling lisbeth.");
            }
            else
            {
                Log.Verbose(lisbethOrder);
                Log.Information("Calling lisbeth");
                if (!await Lisbeth.ExecuteOrders(lisbethOrder))
                {
                    Log.Error("Lisbeth order failed, Dumping order to GCSupply.json");
                    using var outputFile = new StreamWriter("GCSupply.json", false);
                    await outputFile.WriteAsync(lisbethOrder);
                }
                else
                {
                    Log.Information("Lisbeth order should be done");
                }
            }

            items = Core.Memory.ReadArray<GCTurninItem>(Offsets.GCTurnin, Offsets.GCTurninCount);

            if (!items.Any(i => i.CanHandin && InventoryManager.FilledSlots.Any(j => j.RawItemId == i.ItemID && !j.HasMateria() && j.Count >= i.ReqCount)))
            {
                Log.Information("No items available to hand in");
                return;
            }

            if (!GrandCompanySupplyList.Instance.IsOpen)
            {
                await GrandCompanyHelper.InteractWithNpc(GCNpc.Personnel_Officer);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
                if (!SelectString.IsOpen)
                {
                    Log.Error("Window is not open...maybe it didn't get to npc?");
                    return;
                }

                SelectString.ClickSlot(0);
                await Coroutine.Wait(5000, () => GrandCompanySupplyList.Instance.IsOpen);
                if (!GrandCompanySupplyList.Instance.IsOpen)
                {
                    Log.Error("Window is not open...maybe it didn't get to npc?");
                    return;
                }
            }

            if (GrandCompanySupplyList.Instance.IsOpen)
            {
                await GrandCompanySupplyList.Instance.SwitchToSupply();

                await HandleCurrentGCWindow();

                await GrandCompanySupplyList.Instance.SwitchToProvisioning();

                await HandleCurrentGCWindow();

                GrandCompanySupplyList.Instance.Close();
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
                if (SelectString.IsOpen)
                {
                    SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                }
            }
        }

        private static async Task HandleCurrentGCWindow()
        {
            var bools = GrandCompanySupplyList.Instance.GetTurninBools();
            var windowItemIds = GrandCompanySupplyList.Instance.GetTurninItemsIds();
            var required = GrandCompanySupplyList.Instance.GetTurninRequired();
            var maxSeals = Core.Me.MaxGCSeals();
            var items = Core.Memory.ReadArray<GCTurninItem>(Offsets.GCTurnin, Offsets.GCTurninCount);
            for (var index = 0; index < bools.Length; index++)
            {
                if (!bools[index])
                {
                    continue;
                }

                var item = items.FirstOrDefault(j => j.ItemID == windowItemIds[index]);
                var index1 = index;
                var handover = InventoryManager.FilledSlots.Where(k => k.RawItemId == item.ItemID && !k.HasMateria() && k.Count >= required[index1]).OrderByDescending(k => k.HqFlag).FirstOrDefault();
                if (handover == default(BagSlot))
                {
                    continue;
                }

                Log.Information($"{handover.Name} {handover.IsHighQuality}");
                if (handover.IsHighQuality)
                {
                    if (Core.Me.GCSeals() + (item.Seals * 2) < maxSeals)
                    {
                        GrandCompanySupplyList.Instance.ClickItem(index);
                        await Coroutine.Wait(5000, () => Request.IsOpen);
                        if (Request.IsOpen)
                        {
                            handover.Handover();
                            await Coroutine.Wait(5000, () => Request.HandOverButtonClickable);
                            Request.HandOver();
                            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                            if (SelectYesno.IsOpen)
                            {
                                SelectYesno.Yes();
                            }

                            await Coroutine.Wait(5000, () => GrandCompanySupplyReward.Instance.IsOpen);
                            GrandCompanySupplyReward.Instance.Confirm();
                            await Coroutine.Wait(5000, () => GrandCompanySupplyList.Instance.IsOpen);
                            await HandleCurrentGCWindow();
                            break;
                        }
                    }
                    else
                    {
                        Log.Warning($"Would get {item.Seals * 2} and we have {Core.Me.GCSeals()} out of {maxSeals}...too many");
                    }
                }
                else
                {
                    if (Core.Me.GCSeals() + item.Seals < maxSeals)
                    {
                        GrandCompanySupplyList.Instance.ClickItem(index);
                        await Coroutine.Wait(5000, () => Request.IsOpen);
                        if (Request.IsOpen)
                        {
                            handover.Handover();
                            await Coroutine.Wait(5000, () => Request.HandOverButtonClickable);
                            Request.HandOver();
                            await Coroutine.Wait(5000, () => GrandCompanySupplyReward.Instance.IsOpen);
                            GrandCompanySupplyReward.Instance.Confirm();
                            await Coroutine.Wait(5000, () => GrandCompanySupplyList.Instance.IsOpen);
                            await HandleCurrentGCWindow();
                            break;
                        }
                    }
                    else
                    {
                        Log.Warning($"Would get {item.Seals} and we have {Core.Me.GCSeals()} out of {maxSeals}...too many");
                    }
                }
            }
        }

        /// <summary>
        /// Generates a JSON list of required items for current Grand Company missions that the player does not currently possess.
        /// </summary>
        /// <returns>A JSON string representing a list of <see cref="LisbethOrder"/> objects, or an empty string if nothing is needed.</returns>
        public static async Task<string> GetGCSupplyList()
        {
            if (!ContentsInfoDetail.Instance.IsOpen)
            {
                Log.Verbose("Trying to open GC Supply window");

                if (!ContentsInfo.Instance.IsOpen)
                {
                    if (await ContentsInfo.Instance.Open())
                    {
                        ContentsInfo.Instance.OpenGCSupplyWindow();
                    }
                }

                await Coroutine.Wait(5000, () => ContentsInfoDetail.Instance.IsOpen);
            }

            if (!ContentsInfoDetail.Instance.IsOpen)
            {
                Log.Error("Nope failed opening GC Supply window");
                return string.Empty;
            }

            var outList = new List<LisbethOrder>();
            var id = 0;
            foreach (var item in ContentsInfoDetail.Instance.GetCraftingTurninItems().Where(item => !InventoryManager.FilledSlots.Any(i => i.RawItemId == item.Key.Id && !i.HasMateria() && i.Count >= item.Value.Key)))
            {
                Log.Information($"{item.Key} Qty: {item.Value.Key} Class: {item.Value.Value}");
                var order = new LisbethOrder(id, 1, (int)item.Key.Id, item.Value.Key, item.Value.Value);
                outList.Add(order);

                id++;
            }

            foreach (var item in ContentsInfoDetail.Instance.GetGatheringTurninItems().Where(item => !InventoryManager.FilledSlots.Any(i => i.RawItemId == item.Key.Id && i.Count >= item.Value.Key)))
            {
                Log.Information($"{item.Key} Qty: {item.Value.Key} Class: {item.Value.Value}");
                var type = "Gather";
                if (item.Value.Value.Equals("Fisher"))
                {
                    continue; //type = "Fisher";
                }

                var order = new LisbethOrder(id, 2, (int)item.Key.Id, item.Value.Key, type, hq: false);

                outList.Add(order);
                id++;
            }

            ContentsInfoDetail.Instance.Close();
            ContentsInfo.Instance.Close();

            /*foreach (var order in outList)
            {
                Log.Information($"{order}");
            }*/
            if (outList.Count == 0)
            {
                return "";
            }

            return JsonConvert.SerializeObject(outList, Formatting.None);
        }

        /// <summary>
        /// Logs the current Grand Company mission requirements to the console and writes them to a <c>GCSupply.json</c> file.
        /// </summary>
        /// <returns><see langword="true"/> if the list was successfully retrieved and logged; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> PrintGCSupplyList()
        {
            if (!ContentsInfoDetail.Instance.IsOpen)
            {
                Log.Verbose("Trying to open GC Supply window");

                if (!ContentsInfo.Instance.IsOpen)
                {
                    if (await ContentsInfo.Instance.Open())
                    {
                        ContentsInfo.Instance.OpenGCSupplyWindow();
                    }
                }

                await Coroutine.Wait(5000, () => ContentsInfoDetail.Instance.IsOpen);
            }

            if (!ContentsInfoDetail.Instance.IsOpen)
            {
                Log.Error("Nope failed opening GC Supply window");
                return false;
            }

            var outList = new List<LisbethOrder>();
            var id = 0;
            foreach (var item in ContentsInfoDetail.Instance.GetCraftingTurninItems())
            {
                Log.Information($"{item.Key} Qty: {item.Value.Key} Class: {item.Value.Value}");
                var order = new LisbethOrder(id, 1, (int)item.Key.Id, item.Value.Key, item.Value.Value);
                outList.Add(order);

                id++;
            }

            foreach (var item in ContentsInfoDetail.Instance.GetGatheringTurninItems())
            {
                Log.Information($"{item.Key} Qty: {item.Value.Key} Class: {item.Value.Value}");
                var type = "Gather";
                if (item.Value.Value.Equals("Fisher"))
                {
                    type = "Fisher";
                }

                var order = new LisbethOrder(id, 1, (int)item.Key.Id, item.Value.Key, type);

                outList.Add(order);
                id++;
            }

            ContentsInfoDetail.Instance.Close();
            ContentsInfo.Instance.Close();

            foreach (var order in outList)
            {
                Log.Information($"{order}");
            }

            using (var outputFile = new StreamWriter("GCSupply.json", false))
            {
                await outputFile.WriteAsync(JsonConvert.SerializeObject(outList, Formatting.None));
            }

            return true;
        }
    }
}