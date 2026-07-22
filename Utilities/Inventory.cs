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
    /// Provides utility methods and automation tasks for managing player inventory in Final Fantasy XIV.
    /// This includes automated container opening, unlocking collectible items, batch desynthesis,
    /// aetherial reduction, and extracting materia from spiritbound gear.
    /// </summary>
    public static class Inventory
    {
        private static readonly LLogger Log = new("InventoryUtilities", Colors.Green);

        /// <summary>
        /// Gets a value indicating whether the player's character is currently busy or in a state where
        /// certain inventory and item operations cannot be performed safely.
        /// This checks if the player is in an active duty instance, queued for a duty, has a duty pop ready,
        /// is casting a spell, is mounted, is in combat, has a NPC Talk dialog open, is moving, or is flagged as occupied.
        /// </summary>
        public static bool IsBusy => DutyManager.InInstance || DutyManager.InQueue || DutyManager.DutyReady || Core.Me.IsCasting || Core.Me.IsMounted || Core.Me.InCombat || Talk.DialogOpen || MovementManager.IsMoving ||
                                     MovementManager.IsOccupied;

        /// <summary>
        /// Gets the standard character inventory bag identifiers (Bag 1, Bag 2, Bag 3, Bag 4).
        /// These represent the four main tabs of the player's primary inventory.
        /// </summary>
        public static readonly InventoryBagId[] InventoryBagIds = new InventoryBagId[4]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4
        };

        /// <summary>
        /// Gets the identifiers for the 12 primary equipment categories inside the player's Armoury Chest.
        /// These are used to locate equipment pieces (weapons, armor, accessories) in memory or slots.
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
        /// Scans the player's primary inventory for any useable coffer or container items and attempts to open them sequentially.
        /// Coffers are identified by having an <c>ItemAction</c> ID of <c>388</c>.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> indicating whether the scan and opening sequence completed.</returns>
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
        /// Scans the player's primary inventory for useable items that unlock collectible game content and uses them sequentially.
        /// Collectibles are identified by checking the backing action ID:
        /// <list type="bullet">
        /// <item><description><c>853</c>: Minions</description></item>
        /// <item><description><c>25183</c>: Orchestrion Rolls</description></item>
        /// <item><description><c>3357</c>: Triple Triad Cards</description></item>
        /// <item><description><c>2136</c>: Master Crafting Books</description></item>
        /// <item><description><c>4107</c>: Folklore Gathering Tomes</description></item>
        /// </list>
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
        /// Attempts to desynthesize a collection of items sequentially.
        /// For each item, it verifies that the item has a valid repair class and that the player's level for that
        /// class is at least 30. It automatically closes any salvage-related UI windows upon completion.
        /// </summary>
        /// <param name="itemsToDesynth">The collection of bag slots representing items to desynthesize.</param>
        /// <returns>A <see cref="Task{TResult}"/> indicating whether the desynthesis sequence completed successfully (returns <see langword="false"/> if the character is busy or no items are provided).</returns>
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
        /// Automatically performs Aetherial Reduction on all reducable items in the character's primary inventory bags.
        /// Continues sequentially until no more reducable items are found, and automatically dismisses the PurifyResult window.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous outcome.</returns>
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
        /// Extracts materia from all equipped or armory gear that has achieved 100% spiritbond.
        /// Explicitly excludes Novus Relic Sphere Scrolls (item IDs 7873 to 7882, and 9255) to prevent accidental loss or corruption.
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