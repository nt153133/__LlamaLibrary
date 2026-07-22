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
    /// Provides static utility methods and properties for inventory management and automation.
    /// </summary>
    public static class Inventory
    {
        private static readonly LLogger Log = new("InventoryUtilities", Colors.Green);

        /// <summary>
        /// Gets a value indicating whether the player is currently busy with an action that prevents inventory interaction.
        /// </summary>
        /// <remarks>
        /// Busy states include being in an instance, in a duty queue, casting, mounted, in combat, having a talk dialog open, or moving.
        /// </remarks>
        public static bool IsBusy => DutyManager.InInstance || DutyManager.InQueue || DutyManager.DutyReady || Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                                     MovementManager.IsOccupied;

        /// <summary>
        /// An array of <see cref="InventoryBagId"/> representing the four main inventory bags.
        /// </summary>
        public static readonly InventoryBagId[] InventoryBagIds = new InventoryBagId[4]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4
        };

        /// <summary>
        /// An array of <see cref="InventoryBagId"/> representing the various categories of the Armoury Chest.
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
        /// Asynchronously identifies and opens container items (coffers) in the player's inventory.
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
        /// Asynchronously uses various unlockable items found in the inventory, such as minions, orchestrion rolls, and cards.
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
        /// Desynthesizes a collection of items, checking for job and level requirements.
        /// </summary>
        /// <param name="itemsToDesynth">The collection of bag slots to desynthesize.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <see langword="true"/> if the process completed.</returns>
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
        /// Performs aetherial reduction on all reducable items in the main inventory bags.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <see langword="true"/> when finished.</returns>
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
        /// Extracts materia from all equipped and armory gear that has reached 100% spiritbond, excluding specific relic items.
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
