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
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Utilities
{
    /// <summary>
    /// Provides a suite of utility methods for managing the player's inventory,
    /// including automated coffer opening, desynthesis, item reduction, and materia extraction.
    /// </summary>
    public static class Inventory
    {
        private static readonly LLogger Log = new("InventoryUtilities", Colors.Green);

        /// <summary>
        /// Gets a value indicating whether the player is currently busy or occupied with an action.
        /// Checks for active casting, mounting, combat, movement, open dialogs, and duty-related states.
        /// </summary>
        public static bool IsBusy => DutyManager.InInstance || DutyManager.InQueue || DutyManager.DutyReady || Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                                     MovementManager.IsOccupied;

        /// <summary>
        /// Gets the standard set of inventory bag identifiers for player-carried items.
        /// </summary>
        public static readonly InventoryBagId[] InventoryBagIds = new InventoryBagId[4]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4
        };

        /// <summary>
        /// Gets the standard set of armory chest bag identifiers for equipment.
        /// </summary>
        public static readonly InventoryBagId[] ArmoryBagIds = new InventoryBagId[12]
        {
            InventoryBagId.Armory_MainHand,
            InventoryBagId.Armory_OffHand,
            InventoryBagId.Armory_Helmet,
            InventoryBagId.Armory_Chest,
            InventoryBagId.Armory_Glove,
            InventoryBagId.Armory_Belt,
            InventoryBagId.Armory_Pants,
            InventoryBagId.Armory_Boots,
            InventoryBagId.Armory_Earrings,
            InventoryBagId.Armory_Necklace,
            InventoryBagId.Armory_Writs,
            InventoryBagId.Armory_Rings
        };

        /// <summary>
        /// Automatically identifies and opens coffer-style items (Item Action 388) in the player's inventory.
        /// This includes gear containers and other items that require a casting bar to open.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> CofferTask()
        {
            foreach (var bagslot in InventoryManager.FilledSlots.Where(bagslot => bagslot.Item.ItemAction == 388))
            {
                var count = bagslot.Count;
                for (var i = 0; i < count; i++)
                {
                    Log.Information($"Opening Coffer {bagslot.Name} #{i + 1}");
                    bagslot.UseItem();
                    await Coroutine.Wait(5000, () => Core.Me.IsCasting);
                    await Coroutine.Wait(5000, () => !Core.Me.IsCasting);
                    await Coroutine.Sleep(5000);
                }
            }

            return true;
        }

        /// <summary>
        /// Scans the player's inventory for items that unlock game content (minions, orchestrion rolls, cards, books, tomes)
        /// and attempts to use each one.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UseUnlockablesAsync()
        {
            var backingActionIds = new HashSet<uint>
            {
                853, // Minions
                25183, // Orchestrion Rolls
                3357, // Triple Triad Cards
                2136, // Master Crafting Books
                4107, // Folklore Gathering Tomes
            };

            var heldUnlockables = InventoryManager.FilledSlots
                .Where(bs => bs.CanUse() && bs.Item.BackingAction != null)
                .Where(bs => backingActionIds.Contains(bs.Item.BackingAction.Id));

            foreach (var unlockable in heldUnlockables)
            {
                Log.Information($"Using ({unlockable.RawItemId}) {unlockable.Name}");
                unlockable.UseItem();

                if (await Coroutine.Wait(5000, () => Core.Me.IsCasting))
                {
                    await Coroutine.Wait(5000, () => !Core.Me.IsCasting);
                    await Coroutine.Sleep(2500);
                }
            }
        }

        /// <summary>
        /// Desynths a single bagslot, single item or whole stack. *Should* not result in a desynthesis result window.
        /// </summary>
        /// <param name="bagSlot">The item(s) to desynth.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Desynth(BagSlot bagSlot)
        {
            await GeneralFunctions.StopBusy(leaveDuty: false);
            if (IsBusy)
            {
                Log.Error("Can't desynth right now, we're busy.");
                return;
            }

            if (!bagSlot.IsValid)
            {
                Log.Error("No items to desynth.");
                return;
            }

            bagSlot.Desynth();
            await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
            if (Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0)
            {
                await Coroutine.Wait(-1, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
            }

            Log.Verbose("Done desynth");
        }

        /// <summary>
        /// Performs batch desynthesis on a collection of <see cref="BagSlot"/> objects.
        /// Skips items where the player's level for the required repair class is below 30.
        /// </summary>
        /// <param name="itemsToDesynth">The collection of bag slots containing items to desynthesize.</param>
        /// <returns><see langword="true"/> if the process completed; otherwise <see langword="false"/> if the player is busy.</returns>
        public static async Task<bool> Desynth(IEnumerable<BagSlot> itemsToDesynth)
        {
            await GeneralFunctions.StopBusy(leaveDuty: false);
            if (IsBusy)
            {
                Log.Information("Can't desynth right now, we're busy.");
                return false;
            }

            var toDesynthList = itemsToDesynth.ToList();

            if (toDesynthList.Count == 0)
            {
                Log.Warning("No items to desynth.");
                return false;
            }

            Log.Warning($"# of slots to Desynth: {toDesynthList.Count}");
            foreach (var bagSlot in toDesynthList)
            {
                if (bagSlot.Item.RepairClass == 0)
                {
                    Log.Information($"{bagSlot.Name} repair class is 0.");
                    continue;
                }

                var job = (ClassJobType)bagSlot.Item.RepairClass;
                if (Core.Me.Levels[job] < 30)
                {
                    Log.Information($"{bagSlot.Name} requires {job} level 30 and we're level {Core.Me.Levels[job]}");
                    continue;
                }

                await Desynth(bagSlot);
            }

            if (SalvageResult.IsOpen)
            {
                SalvageResult.Close();
                await Coroutine.Wait(5000, () => !SalvageResult.IsOpen);
            }

            if (SalvageAutoDialog.Instance.IsOpen)
            {
                SalvageAutoDialog.Instance.Close();
                await Coroutine.Wait(5000, () => !SalvageAutoDialog.Instance.IsOpen);
            }

            return true;
        }

        /// <summary>
        /// Automatically performs Aetherial Reduction on all reducible items found in the standard player inventory bags.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> ReduceAll()
        {
            await GeneralFunctions.StopBusy(false);

            while (InventoryManager.FilledSlots.Any(x => InventoryBagIds.Contains(x.BagId) && x.IsReducable))
            {
                var item = InventoryManager.FilledSlots.FirstOrDefault(x => InventoryBagIds.Contains(x.BagId) && x.IsReducable);

                if (item == null)
                {
                    break;
                }

                Log.Information($"Reducing - Name: {item.Item.CurrentLocaleName}");
                item.Reduce();
                await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
                await Coroutine.Wait(20000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
            }

            await Coroutine.Sleep(1000);

            var windowByName = RaptureAtkUnitManager.GetWindowByName("PurifyResult");
            if (windowByName != null)
            {
                windowByName.SendAction(1, 3uL, 4294967295uL);
            }

            return true;
        }

        /// <summary>
        /// Scans inventory and armory for gear with 100% spiritbond and attempts to extract Materia from each.
        /// Excludes Relic Sphere Scrolls (item IDs 7873-7882, 9255) to prevent accidental loss or interruption.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ExtractFromAllGear()
        {
            await GeneralFunctions.StopBusy(leaveDuty: false);
            if (Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                MovementManager.IsOccupied)
            {
                return;
            }
            List<uint> RelicSphereScrolls = new() { 7873, 7874, 7875, 7876, 7877, 7878, 7879, 7880, 7881, 7882, 9255 };

            var gear = InventoryManager.FilledInventoryAndArmory.Where(x => x.SpiritBond == 100f  && RelicSphereScrolls.Contains(x.RawItemId) == false);
            if (gear.Any())
            {
                foreach (var slot in gear)
                {
                    Log.Information($"Extract Materia from: {slot}");
                    slot.ExtractMateria();
                    await Coroutine.Wait(5000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0);
                    await Coroutine.Wait(6000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
                    await Coroutine.Sleep(100);

                    if (Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                        MovementManager.IsOccupied)
                    {
                        return;
                    }
                }
            }
        }
    }
}
