using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;

namespace LlamaBotBases.LlamaUtilities
{
    public static class FCWorkshop
    {
        public static List<Item> itemList = new List<Item>();
        private static uint[] npcids = new uint[] { 2005236, 2005238, 2005240, 2007821 };

        public const string Name = "FCWorkshop";
        private static readonly LLogger Log = new LLogger(Name, Colors.White);

        public static async Task<bool> HandInItems()
        {
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            Navigator.PlayerMover = new SlideMover();

            if (!SubmarinePartsMenu.Instance.IsOpen)
            {
                Log.Verbose("Trying to open FC Workshop window.");

                if (!await OpenFCCraftingStation())
                {
                    Log.Error("Failed to open FC Workshop window.");
                    return false;
                }
            }

            if (!SubmarinePartsMenu.Instance.IsOpen)
            {
                Log.Error("Failed to open FC Workshop window.");
                return false;
            }

            //    List<LisbethOrder> outList = new List<LisbethOrder>();
            var id = 0;
            var counts = SubmarinePartsMenu.Instance.GetItemAvailCount();
            var done = SubmarinePartsMenu.Instance.GetTurninsDone();
            foreach (var item in SubmarinePartsMenu.Instance.GetCraftingTurninItems())
            {
                var needed = (item.Qty * item.TurnInsRequired) - (item.Qty * done[id]);
                var itemCount = (int)DataManager.GetItem((uint)item.ItemId).ItemCount();

                var turnInsAvail = itemCount / item.Qty;

                Log.Information($"{item}");
                Log.Information($"Player has {itemCount} and {needed} are still needed and can do {turnInsAvail} turnins");
                var turnInsNeeded = item.TurnInsRequired - done[id];

                if (turnInsNeeded >= 1)
                {
                    if (turnInsAvail >= 1)
                    {
                        for (var i = 0; i < Math.Min(turnInsAvail, turnInsNeeded); i++)
                        {
                            BagSlot bagSlot = null;

                            if (HqItemCount(item.ItemId) >= item.Qty)
                            {
                                bagSlot = InventoryManager.FilledSlots.First(slot => slot.RawItemId == item.ItemId && slot.IsHighQuality && slot.Count >= item.Qty);
                                Log.Information($"Have HQ {bagSlot.Name}");

                                // continue;
                            }
                            else if (ItemCount(item.ItemId) >= item.Qty)
                            {
                                bagSlot = InventoryManager.FilledSlots.FirstOrDefault(slot => slot.RawItemId == item.ItemId && !slot.IsHighQuality && slot.Count >= item.Qty);

                                if (bagSlot == null)
                                {
                                    await CloseFCCraftingStation();

                                    await InventoryHelpers.LowerQualityAndCombine(item.ItemId);

                                    // var nqSlot = InventoryManager.FilledSlots.FirstOrDefault(slot => slot.RawItemId == item.ItemId && slot.IsHighQuality && slot.Count < item.Qty);

                                    await OpenFCCraftingStation();
                                    bagSlot = InventoryManager.FilledSlots.FirstOrDefault(slot => slot.RawItemId == item.ItemId && !slot.IsHighQuality && slot.Count >= item.Qty);
                                    Log.Information($"Need To Lower Quality {bagSlot.Name}");
                                }
                                else
                                {
                                    Log.Information($"Have NQ {bagSlot.Name}");
                                }
                            }
                            else
                            {
                                Log.Warning($"Something went wrong {ItemCount(item.ItemId)}");
                            }

                            if (bagSlot != null)
                            {
                                Log.Information($"Turn in {bagSlot.Name} HQ({bagSlot.IsHighQuality})");
                                await Coroutine.Sleep(500);
                                SubmarinePartsMenu.Instance.ClickItem(id);

                                await Coroutine.Wait(5000, () => Request.IsOpen);
                                var isHQ = bagSlot.IsHighQuality;
                                bagSlot.Handover();

                                await Coroutine.Wait(5000, () => Request.HandOverButtonClickable);

                                if (Request.HandOverButtonClickable)
                                {
                                    Request.HandOver();
                                    await Coroutine.Sleep(500);
                                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);

                                    if (SelectYesno.IsOpen)
                                    {
                                        SelectYesno.Yes();
                                    }

                                    await Coroutine.Sleep(700);

                                    if (!isHQ)
                                    {
                                        continue;
                                    }

                                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);

                                    if (SelectYesno.IsOpen)
                                    {
                                        SelectYesno.Yes();
                                    }

                                    await Coroutine.Sleep(700);
                                }
                                else
                                {
                                    Log.Error("HandOver Stuck");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Error("Bagslot is null");
                            }
                        }
                    }
                    else
                    {
                        Log.Information($"No Turn ins available {turnInsAvail}");
                    }
                }
                else
                {
                    Log.Information($"turnInsNeeded {turnInsNeeded}");
                }

                Log.Information("--------------");
                id++;
            }

            await CloseFCCraftingStation();

            return true;
        }

        public static async Task<bool> OpenFCCraftingStation()
        {
            if (!GameObjectManager.GetObjectsByNPCIds<GameObject>(npcids).Any())
            {
                Log.Error("Can't find Fabrication Station");
                return false;
            }

            var station = GameObjectManager.GetObjectsByNPCIds<GameObject>(npcids).First();

            if (!station.IsWithinInteractRange)
            {
                var _target = station.Location;
                Navigator.PlayerMover.MoveTowards(_target);
                while (_target.Distance2D(Core.Me.Location) >= 4)
                {
                    Navigator.PlayerMover.MoveTowards(_target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            station.Interact();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            SelectString.ClickSlot(0);

            await Coroutine.Wait(5000, () => SubmarinePartsMenu.Instance.IsOpen);

            return SubmarinePartsMenu.Instance.IsOpen;
        }

        public static async Task<bool> CloseFCCraftingStation()
        {
            if (!SubmarinePartsMenu.Instance.IsOpen)
            {
                return true;
            }

            SubmarinePartsMenu.Instance.Close();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            SelectString.ClickSlot(3);

            await Coroutine.Sleep(500);

            return SelectString.IsOpen;
        }

        public static int HqItemCount(int itemId)
        {
            return Lua.GetReturnVal<int>($"return _G['{Core.Player.LuaString}']:GetNumOfHqItems({itemId});");
        }

        public static int ItemCount(int itemId)
        {
            return Lua.GetReturnVal<int>($"return _G['{Core.Player.LuaString}']:GetNumOfItems({itemId});");
        }
    }
}