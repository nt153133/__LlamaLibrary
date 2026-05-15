using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Utilities;

// ReSharper disable PossibleMultipleEnumeration

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helpers for interacting with the Free Company chest (deposit, withdrawal, crystal management, and gil transfers).
    /// Handles navigation to the nearest Company Chest NPC, permission checks, bag-slot moves, and window management.
    /// </summary>
    public static class CompanyChestHelper
    {
        /// <summary>NPC IDs for the housing-ward Company Chest objects (instanced and standard).</summary>
        public static readonly uint[] HousingCompanyChest = { 196627, 2000470 };

        private static readonly LLogger Log = new(nameof(CompanyChestHelper), Colors.BurlyWood);

        /// <summary>Item IDs for all 18 crystal types (shards, crystals, and clusters, elements 0–17).</summary>
        public static readonly uint[] CrystalIds = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

        /// <summary>
        /// Maps each crystal item ID to its fixed bag-slot index in the <see cref="InventoryBagId.GrandCompany_Crystals"/> bag.
        /// </summary>
        public static readonly IReadOnlyDictionary<uint, ushort> CrystalSlots = new Dictionary<uint, ushort>
        {
            { 2, 0 }, //Slot 0  Fire Shard
            { 3, 1 }, //Slot 1  Ice Shard
            { 4, 2 }, //Slot 2  Wind Shard
            { 5, 3 }, //Slot 3  Earth Shard
            { 6, 4 }, //Slot 4  Lightning Shard
            { 7, 5 }, //Slot 5  Water Shard
            { 8, 6 }, //Slot 6  Fire Crystal
            { 9, 7 }, //Slot 7  Ice Crystal
            { 10, 8 }, //Slot 8  Wind Crystal
            { 11, 9 }, //Slot 9  Earth Crystal
            { 12, 10 }, //Slot 10  Lightning Crystal
            { 13, 11 }, //Slot 11  Water Crystal
            { 14, 12 }, //Slot 12  Fire Cluster
            { 15, 13 }, //Slot 13  Ice Cluster
            { 16, 14 }, //Slot 14  Wind Cluster
            { 17, 15 }, //Slot 15  Earth Cluster
            { 18, 16 }, //Slot 16  Lightning Cluster
            { 19, 17 }, //Slot 17  Water Cluster
        };

        /// <summary>Maps chest tab index (0–4) to the corresponding <see cref="InventoryBagId"/> for that tab.</summary>
        public static readonly IReadOnlyDictionary<int, InventoryBagId> ChestTabBags = new Dictionary<int, InventoryBagId>
        {
            { 0, InventoryBagId.GrandCompany_Page1 },
            { 1, InventoryBagId.GrandCompany_Page2 },
            { 2, InventoryBagId.GrandCompany_Page3 },
            { 3, InventoryBagId.GrandCompany_Page4 },
            { 4, InventoryBagId.GrandCompany_Page5 },
        };

        /// <summary>Known city-state and residential district Company Chest NPC locations used when not in a housing zone.</summary>
        public static readonly IReadOnlyList<Npc> ChestLocations = new List<Npc>
        {
            new Npc(2000470, 132, new Vector3(-78.44666f, 0.5645142f, -0.04577637f)), //Company Chest New Gridania - Adders' Nest
            new Npc(2000470, 133, new Vector3(135.729f, 14.51129f, -87.48004f)), //Company Chest Old Gridania - (outside) Shaded Bowyer
            new Npc(2000470, 129, new Vector3(-200f, 17.04425f, 58.76245f)), //Company Chest Limsa Lominsa Lower Decks - Hawkers' Round
            new Npc(2000470, 128, new Vector3(90.34863f, 41.33667f, 60.71558f)), //Company Chest Limsa Lominsa Upper Decks - Maelstrom Command
            new Npc(2000470, 130, new Vector3(-149.3096f, 4.53186f, -91.38635f)), //Company Chest Ul'dah - Steps of Nald - Hall of Flames
            new Npc(2000470, 131, new Vector3(128.954f, 5.050659f, -40.94f)), //Company Chest Ul'dah - Steps of Thal - Sapphire Avenue Exchange
            new Npc(2000470, 635, new Vector3(-57.05353f, 2.212463f, 87.87671f)), //Company Chest Rhalgr's Reach - Southwest
            new Npc(2000470, 628, new Vector3(54.67297f, 5.203247f, 55.28345f)), //Company Chest Kugane - Kogane Dori
        };

        /// <summary>
        /// Returns the nearest usable Company Chest NPC. Prefers a housing-ward chest when one is
        /// visible nearby; otherwise returns the closest city-state chest from <see cref="ChestLocations"/>.
        /// </summary>
        public static Npc? ClosestCompanyChest
        {
            get
            {
                if (GameObjectManager.GetObjectsByNPCIds<GameObject>(HousingCompanyChest).Length == 0) //(!HousingHelper.IsInsideHouse && !HousingHelper.IsInsideWorkshop) ||
                {
                    return NpcHelper.GetClosestNpc(ChestLocations);
                }

                var firstOrDefault = GameObjectManager.GetObjectsByNPCIds<GameObject>(HousingCompanyChest).FirstOrDefault();
                return firstOrDefault == null ? null : new Npc(firstOrDefault);
            }
        }

        /// <summary><c>true</c> when the player has permission to deposit gil into the FC chest.</summary>
        public static bool CanDepositGil => FreeCompanyChest.Instance.GilPermission is CompanyChestPermission.DepositOnly or CompanyChestPermission.FullAccess;
        /// <summary><c>true</c> when the player has full-access permission to withdraw gil from the FC chest.</summary>
        public static bool CanWithdrawGil => FreeCompanyChest.Instance.GilPermission is CompanyChestPermission.FullAccess;

        /// <summary><c>true</c> when the player has permission to deposit crystals into the FC chest.</summary>
        public static bool CanDepositCrystals => FreeCompanyChest.Instance.CrystalsPermission is CompanyChestPermission.DepositOnly or CompanyChestPermission.FullAccess;
        /// <summary><c>true</c> when the player has full-access permission to withdraw crystals from the FC chest.</summary>
        public static bool CanWithdrawCrystals => FreeCompanyChest.Instance.CrystalsPermission is CompanyChestPermission.FullAccess;

        /// <summary>Tab indexes for which the player has at least deposit permission.</summary>
        public static IEnumerable<int> DepositBagIndexes => FreeCompanyChest.Instance.ItemTabPermissions.Where(i => i.Value is CompanyChestPermission.DepositOnly or CompanyChestPermission.FullAccess).Select(i => i.Key);

        /// <summary>Bag IDs corresponding to <see cref="DepositBagIndexes"/>.</summary>
        public static IEnumerable<InventoryBagId> DepositBagIds => DepositBagIndexes.Select(i => ChestTabBags[i]);

        /// <summary>Tab indexes for which the player has full withdraw permission.</summary>
        public static IEnumerable<int> WithdrawBagIndexes => FreeCompanyChest.Instance.ItemTabPermissions.Where(i => i.Value is CompanyChestPermission.FullAccess).Select(i => i.Key);

        /// <summary>Bag IDs corresponding to <see cref="WithdrawBagIndexes"/>.</summary>
        public static IEnumerable<InventoryBagId> WithdrawBagIds => WithdrawBagIndexes.Select(i => ChestTabBags[i]);

        /// <summary>Tab indexes for tabs the player can access (any permission except NoAccess).</summary>
        public static IEnumerable<int> AllBagIndexes => FreeCompanyChest.Instance.ItemTabPermissions.Where(i => i.Value is not CompanyChestPermission.NoAccess).Select(i => i.Key);

        /// <summary>Bag IDs corresponding to <see cref="AllBagIndexes"/>.</summary>
        public static IEnumerable<InventoryBagId> AllBagIds => AllBagIndexes.Select(i => ChestTabBags[i]);

        /// <summary>
        /// Deposits up to <paramref name="amount"/> of crystal <paramref name="crystalId"/> into the FC crystal tab.
        /// Skips if the crystal tab is already at the stack cap. Returns the actual quantity deposited.
        /// </summary>
        /// <param name="crystalId">Item ID of the crystal (must be in <see cref="CrystalIds"/>).</param>
        /// <param name="amount">Maximum quantity to deposit.</param>
        /// <returns>The number of crystals actually deposited, or 0 on failure/no permission.</returns>
        public static async Task<uint> DepositCrystals(uint crystalId, uint amount)
        {
            if (!await MakeSureChestIsOpen(false, true))
            {
                return 0;
            }

            if (FreeCompanyChest.Instance.CrystalsPermission is CompanyChestPermission.NoAccess or CompanyChestPermission.ViewOnly)
            {
                await CloseChest();
                return 0;
            }

            if (!CrystalIds.Contains(crystalId))
            {
                await CloseChest();
                return 0;
            }

            var fcSlot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.GrandCompany_Crystals)[CrystalSlots[crystalId]];
            var slot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.Crystals)[CrystalSlots[crystalId]];

            if (DataManager.GetItem(crystalId).StackSize == fcSlot.Count)
            {
                await CloseChest();
                return 0;
            }

            if (fcSlot.Count + amount > DataManager.GetItem(crystalId).StackSize)
            {
                amount = DataManager.GetItem(crystalId).StackSize - fcSlot.Count;
            }

            FreeCompanyChest.Instance.SelectCrystalTab();

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);
            var amt = Math.Min(slot.Count, amount);

            if (amt == 0 || !slot.IsFilled)
            {
                await CloseChest();
                return 0;
            }

            if (!await BagSlotMoveChest(slot, fcSlot, amt))
            {
                await CloseChest();
                return 0;
            }

            await Coroutine.Sleep(200);
            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            await CloseChest();

            return amt;
        }

        /// <summary>
        /// Withdraws up to <paramref name="amount"/> of crystal <paramref name="crystalId"/> from the FC crystal tab.
        /// Skips if the player's crystal bag is already at the stack cap. Returns the actual quantity withdrawn.
        /// </summary>
        /// <param name="crystalId">Item ID of the crystal (must be in <see cref="CrystalIds"/>).</param>
        /// <param name="amount">Maximum quantity to withdraw.</param>
        /// <returns>The number of crystals actually withdrawn, or 0 on failure/no permission.</returns>
        public static async Task<uint> WithdrawCrystals(uint crystalId, uint amount)
        {
            if (!await MakeSureChestIsOpen(false, true))
            {
                return 0;
            }

            if (FreeCompanyChest.Instance.CrystalsPermission is not CompanyChestPermission.FullAccess)
            {
                //Log.Error("No Permission");
                await CloseChest();
                return 0;
            }

            if (!CrystalIds.Contains(crystalId))
            {
                //Log.Error("Not a crystal");
                await CloseChest();
                return 0;
            }

            var fcSlot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.GrandCompany_Crystals)[CrystalSlots[crystalId]];
            var slot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.Crystals)[CrystalSlots[crystalId]];

            if (DataManager.GetItem(crystalId).StackSize == slot.Count)
            {
                //Log.Error("Already Maxed");
                await CloseChest();
                return 0;
            }

            if (slot.Count + amount > DataManager.GetItem(crystalId).StackSize)
            {
                amount = DataManager.GetItem(crystalId).StackSize - slot.Count;
            }

            FreeCompanyChest.Instance.SelectCrystalTab();

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);
            var amt = Math.Min(fcSlot.Count, amount);

            if (amt == 0 || !fcSlot.IsFilled)
            {
                await CloseChest();
                return 0;
            }

            if (!await BagSlotMoveChest(fcSlot, slot, amt))
            {
                //Log.Error("Bagslot failed");
                await CloseChest();
                return 0;
            }

            await Coroutine.Sleep(200);
            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            await CloseChest();

            return amt;
        }

        /// <summary>Deposits all inventory slots whose <see cref="BagSlot.TrueItemId"/> matches one of <paramref name="itemIds"/>.</summary>
        /// <returns><c>true</c> on success or when no matching slots exist.</returns>
        public static async Task<bool> DepositItems(IEnumerable<uint> itemIds)
        {
            var slots = InventoryManager.FilledSlots.Where(i => itemIds.Contains(i.TrueItemId));

            if (slots.Any())
            {
                return await DepositItems(slots);
            }

            return true;
        }

        /// <summary>
        /// Deposits all matching slots into the specified FC chest <paramref name="bagId"/>.
        /// </summary>
        /// <param name="itemIds">Item IDs to deposit.</param>
        /// <param name="bagId">Target FC chest bag.</param>
        /// <returns><c>true</c> on success or when no matching slots exist.</returns>
        public static async Task<bool> DepositItems(IEnumerable<uint> itemIds, InventoryBagId bagId)
        {
            var slots = InventoryManager.FilledSlots.Where(i => itemIds.Contains(i.TrueItemId));

            if (slots.Any())
            {
                return await DepositItems(slots, bagId);
            }

            return true;
        }

        /// <summary>Deposits each bag slot in <paramref name="bagSlots"/> and closes the chest when done.</summary>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> DepositItems(IEnumerable<BagSlot> bagSlots)
        {
            foreach (var slot in bagSlots)
            {
                if (!await DepositItem(slot, false))
                {
                    return false;
                }
            }

            return await CloseChest();
        }

        /// <summary>Deposits each bag slot in <paramref name="bagSlots"/> into <paramref name="bagId"/> and closes the chest when done.</summary>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> DepositItems(IEnumerable<BagSlot> bagSlots, InventoryBagId bagId)
        {
            foreach (var slot in bagSlots)
            {
                if (!await DepositItem(slot, bagId, false))
                {
                    return false;
                }
            }

            return await CloseChest();
        }

        /// <summary>
        /// Withdraws matching items from any accessible FC bag. Finds slots in <see cref="WithdrawBagIds"/>
        /// and delegates to the bag-slot overload.
        /// </summary>
        /// <param name="itemIds">Item IDs to withdraw.</param>
        /// <param name="amount">Maximum stack quantity per slot to transfer.</param>
        /// <returns><c>true</c> on success or when no matching slots exist.</returns>
        public static async Task<bool> WithdrawItems(IEnumerable<uint> itemIds, uint amount = 1)
        {
            if (!await MakeSureChestIsOpen())
            {
                return false;
            }

            var slots = InventoryManager.GetBagsByInventoryBagId(WithdrawBagIds.ToArray()).SelectMany(x => x.FilledSlots).Where(i => itemIds.Contains(i.TrueItemId));

            if (slots.Any())
            {
                return await WithdrawItems(slots, amount);
            }

            return true;
        }

        /// <summary>
        /// Withdraws matching items from a specific FC chest <paramref name="bagId"/>.
        /// Prefers stacking into existing slots before pulling from multiple slots.
        /// </summary>
        /// <param name="itemIds">Item IDs to withdraw.</param>
        /// <param name="bagId">The specific FC chest bag to withdraw from.</param>
        /// <param name="amount">Maximum total quantity to transfer.</param>
        /// <returns><c>true</c> on success or when no matching slots exist.</returns>
        public static async Task<bool> WithdrawItems(IEnumerable<uint> itemIds, InventoryBagId bagId, uint amount = 1)
        {
            if (!await MakeSureChestIsOpen(bagId))
            {
                return false;
            }

            if (!ChestTabBags.Values.Contains(bagId))
            {
                Log.Error($"Bag {bagId} is not a valid chest tab");
                return false;
            }

            if (!WithdrawBagIds.Contains(bagId))
            {
                Log.Error($"Bag {bagId} is not a valid withdraw chest tab");
                return false;
            }

            var slots = InventoryManager.GetBagsByInventoryBagId(bagId).SelectMany(x => x.FilledSlots).Where(i => itemIds.Contains(i.TrueItemId)).OrderBy(i => i.Count);

            if (slots.Any())
            {
                if (slots.First().Count > amount)
                {
                    return await WithdrawItems(new List<BagSlot> { slots.First() }, amount);
                }

                return await WithdrawItems(slots, amount);
            }

            return true;
        }

        /// <summary>Withdraws each bag slot in <paramref name="bagSlots"/> from the chest and closes it when done.</summary>
        /// <param name="bagSlots">FC chest bag slots to withdraw.</param>
        /// <param name="amount">Maximum stack quantity per slot to transfer.</param>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> WithdrawItems(IEnumerable<BagSlot> bagSlots, uint amount = 1)
        {
            if (!await MakeSureChestIsOpen())
            {
                return false;
            }

            foreach (var slot in bagSlots)
            {
                if (!await WithdrawItem(slot, amount, false))
                {
                    return false;
                }
            }

            return await CloseChest();
        }

        /// <summary>
        /// Deposits a single inventory <paramref name="bagSlot"/> into the first available FC chest tab.
        /// Optionally closes the chest after the transfer.
        /// </summary>
        /// <param name="bagSlot">The player inventory slot to deposit.</param>
        /// <param name="closeWindow">When <c>true</c>, closes the chest window after depositing.</param>
        /// <returns><c>true</c> when the slot is no longer filled after the transfer.</returns>
        public static async Task<bool> DepositItem(BagSlot bagSlot, bool closeWindow = true)
        {
            if (!await MakeSureChestIsOpen())
            {
                return false;
            }

            var slot = GetNextOrStackSlot(bagSlot, TransactionType.Deposit);
            if (slot == default)
            {
                Log.Information("Out of space in the destination");
                return false;
            }

            var index = ChestTabBags.First(i => i.Value == slot.BagId).Key;

            FreeCompanyChest.Instance.SelectItemTab(index);

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            var result = await BagSlotMoveChest(bagSlot, slot);

            await Coroutine.Sleep(200);
            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            if (closeWindow)
            {
                result = result && await CloseChest();
            }

            return result && !(bagSlot.IsValid && bagSlot.IsFilled);
        }

        /// <summary>
        /// Deposits a single inventory <paramref name="bagSlot"/> into the specific FC chest <paramref name="bagId"/>.
        /// </summary>
        /// <param name="bagSlot">The player inventory slot to deposit.</param>
        /// <param name="bagId">Target FC chest bag.</param>
        /// <param name="closeWindow">When <c>true</c>, closes the chest window after depositing.</param>
        /// <returns><c>true</c> when the slot is no longer filled after the transfer.</returns>
        public static async Task<bool> DepositItem(BagSlot bagSlot, InventoryBagId bagId, bool closeWindow = true)
        {
            if (!await MakeSureChestIsOpen(bagId))
            {
                return false;
            }

            var slot = GetNextOrStackSlot(bagSlot, bagId, TransactionType.Deposit);
            if (slot == default)
            {
                Log.Information("Out of space in the destination");
                return false;
            }

            var index = ChestTabBags.First(i => i.Value == slot.BagId).Key;

            FreeCompanyChest.Instance.SelectItemTab(index);

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            var result = await BagSlotMoveChest(bagSlot, slot);

            await Coroutine.Sleep(200);
            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            if (closeWindow)
            {
                result = result && await CloseChest();
            }

            return result && !(bagSlot.IsValid && bagSlot.IsFilled);
        }

        /// <summary>
        /// Withdraws <paramref name="amount"/> of the item in FC chest slot <paramref name="bagSlot"/> to the player's bags.
        /// For stacks larger than 1 the InputNumeric dialogue is used.
        /// Optionally closes the chest after the transfer.
        /// </summary>
        /// <param name="bagSlot">The FC chest bag slot to withdraw from.</param>
        /// <param name="amount">Quantity to withdraw.</param>
        /// <param name="closeWindow">When <c>true</c>, closes the chest window after withdrawing.</param>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> WithdrawItem(BagSlot bagSlot, uint amount = 1, bool closeWindow = true)
        {
            if (!await MakeSureChestIsOpen())
            {
                return false;
            }

            var slot = GetNextOrStackSlot(bagSlot, TransactionType.Withdrawal);
            if (slot == default)
            {
                Log.Information("Out of space in the destination");
                return false;
            }

            var index = ChestTabBags.First(i => i.Value == bagSlot.BagId).Key;

            FreeCompanyChest.Instance.SelectItemTab(index);

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            var result = await BagSlotMoveChest(bagSlot, slot, Math.Min(amount, bagSlot.Count));

            await Coroutine.Sleep(200);
            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            if (closeWindow)
            {
                result = result && await CloseChest();
            }

            if (amount > 1)
            {
                return result;
            }

            return result && !(bagSlot.IsValid && bagSlot.IsFilled);
        }

        /// <summary>
        /// Deposits <paramref name="amount"/> gil into the FC chest.
        /// Switches the gil bank window to Deposit mode if needed, waits for the balance to update, then closes the chest.
        /// </summary>
        /// <param name="amount">Gil amount to deposit.</param>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> DepositGil(uint amount)
        {
            if (!await MakeSureChestIsOpen(true))
            {
                return false;
            }

            if (FreeCompanyChest.Instance.GilPermission is CompanyChestPermission.NoAccess or CompanyChestPermission.ViewOnly)
            {
                return false;
            }

            var currentGil = AgentFreeCompanyChest.Instance.GilBalance;

            FreeCompanyChest.Instance.SelectGilTab();

            if (!await Coroutine.Wait(10000, () => Bank.Instance.IsOpen))
            {
                Log.Error("Could not open company bank");
                return false;
            }

            if (AgentFreeCompanyChest.Instance.GilWithdrawDeposit != 0)
            {
                Bank.Instance.ClickWithdrawalDeposit();
                await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.GilWithdrawDeposit == 0);
            }

            Bank.Instance.SetAmount((int)amount);

            await Coroutine.Sleep(PingChecker.CurrentPing);

            await Coroutine.Sleep(1000);

            Bank.Instance.Proceed();

            if (!await Coroutine.Wait(10000, () => AgentFreeCompanyChest.Instance.GilBalance > currentGil))
            {
                await AgentFreeCompanyChest.Instance.LoadBag(InventoryBagId.GrandCompany_Gil);
                Log.Error($"Could not verify amount changed: Old balance {currentGil} new balance {AgentFreeCompanyChest.Instance.GilBalance}");
                return false;
            }

            Log.Information("Updating gil");
            await AgentFreeCompanyChest.Instance.LoadBag(InventoryBagId.GrandCompany_Gil);

            return await CloseChest();
        }

        /// <summary>
        /// Withdraws <paramref name="amount"/> gil from the FC chest.
        /// Switches the gil bank window to Withdrawal mode if needed, waits for the balance to update, then closes the chest.
        /// </summary>
        /// <param name="amount">Gil amount to withdraw.</param>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> WithdrawGil(uint amount)
        {
            if (!await MakeSureChestIsOpen(true))
            {
                return false;
            }

            if (FreeCompanyChest.Instance.GilPermission is CompanyChestPermission.NoAccess or CompanyChestPermission.ViewOnly)
            {
                return false;
            }

            var currentGil = AgentFreeCompanyChest.Instance.GilBalance;

            FreeCompanyChest.Instance.SelectGilTab();

            if (!await Coroutine.Wait(10000, () => Bank.Instance.IsOpen))
            {
                Log.Error("Could not open company bank");
                return false;
            }

            if (AgentFreeCompanyChest.Instance.GilWithdrawDeposit != 1)
            {
                Bank.Instance.ClickWithdrawalDeposit();
                await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.GilWithdrawDeposit == 1);
            }

            Bank.Instance.SetAmount((int)amount);

            await Coroutine.Sleep(PingChecker.CurrentPing);

            await Coroutine.Sleep(1000);

            Bank.Instance.Proceed();

            if (!await Coroutine.Wait(10000, () => AgentFreeCompanyChest.Instance.GilBalance < currentGil))
            {
                await AgentFreeCompanyChest.Instance.LoadBag(InventoryBagId.GrandCompany_Gil);
                Log.Error($"Could not verify amount changed: Old balance {currentGil} new balance {AgentFreeCompanyChest.Instance.GilBalance}");
                return false;
            }

            Log.Information("Updating gil");
            await AgentFreeCompanyChest.Instance.LoadBag(InventoryBagId.GrandCompany_Gil);

            return await CloseChest();
        }

        /// <summary>
        /// Finds the best destination slot for a move of <paramref name="bagSlot"/>.
        /// For deposits, prefers an existing partial stack; falls back to the next free FC chest slot.
        /// For withdrawals, prefers a stackable slot in the player's bags; falls back to the next free inventory slot.
        /// </summary>
        /// <param name="bagSlot">The source slot whose item is being moved.</param>
        /// <param name="transactionType">Indicates deposit or withdrawal direction.</param>
        /// <returns>The target <see cref="BagSlot"/>, or <c>null</c> when no space is available.</returns>
        public static BagSlot? GetNextOrStackSlot(BagSlot bagSlot, TransactionType transactionType)
        {
            IEqualityComparer<BagSlot> eqx;
            IEnumerable<InventoryBagId>? bagList;
            switch (transactionType)
            {
                case TransactionType.Deposit:
                    bagList = DepositBagIds;
                    eqx = new BagSlotComparerDeposit();
                    break;
                case TransactionType.Withdrawal:
                    bagList = Inventory.InventoryBagIds;
                    eqx = new BagSlotComparer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
            }

            var bags = InventoryManager.GetBagsByInventoryBagId(bagList.ToArray()).SelectMany(x => x.FilledSlots);
            var match = bags.Where(i => eqx.Equals(i, bagSlot));

            if (match.Any())
            {
                Log.Information("Found item in chest so stacking");
                return match.OrderByDescending(i => i.Count).First();
            }

            //Find retainer code that checks for stacks
            //bags.free
            return bagList.OrderBy(i => (int)i).NextFreeBagSlot();
        }

        /// <summary>
        /// Overload of <see cref="GetNextOrStackSlot(BagSlot,TransactionType)"/> that constrains deposit
        /// operations to a specific <paramref name="bagId"/> rather than scanning all deposit-permitted tabs.
        /// </summary>
        /// <param name="bagSlot">The source slot.</param>
        /// <param name="bagId">The target FC chest bag for deposits.</param>
        /// <param name="transactionType">Indicates deposit or withdrawal direction.</param>
        /// <returns>The target <see cref="BagSlot"/>, or <c>null</c> when no space is available or the bag is invalid.</returns>
        public static BagSlot? GetNextOrStackSlot(BagSlot bagSlot, InventoryBagId bagId, TransactionType transactionType)
        {
            IEnumerable<InventoryBagId>? bagList;
            IEqualityComparer<BagSlot> eqx;
            switch (transactionType)
            {
                case TransactionType.Deposit:
                    if (DepositBagIds.Contains(bagId))
                    {
                        bagList = new List<InventoryBagId> { bagId };
                    }
                    else
                    {
                        Log.Error($"Bag {bagId} is not a valid chest tab");
                        return null;
                    }

                    eqx = new BagSlotComparerDeposit();

                    break;
                case TransactionType.Withdrawal:
                    bagList = Inventory.InventoryBagIds;
                    eqx = new BagSlotComparer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
            }

            var bags = InventoryManager.GetBagsByInventoryBagId(bagList.ToArray()).SelectMany(x => x.FilledSlots);
            var match = bags.Where(i => eqx.Equals(i, bagSlot));

            if (match.Any())
            {
                Log.Information("Found item in chest so stacking");
                return match.OrderByDescending(i => i.Count).First();
            }

            //Find retainer code that checks for stacks
            //bags.free
            return bagList.OrderBy(i => (int)i).NextFreeBagSlot();
        }

        /// <summary>
        /// Moves <paramref name="count"/> items from <paramref name="source"/> to <paramref name="dest"/> via the FC chest
        /// interface. If the source stack contains more than one item the InputNumeric dialogue is handled automatically.
        /// When <paramref name="count"/> is 1 and the source is a stack, the full stack is moved.
        /// </summary>
        /// <param name="source">Source bag slot (player bags or FC chest).</param>
        /// <param name="dest">Destination bag slot (FC chest or player bags).</param>
        /// <param name="count">Number of items to move; defaults to 1 (full stack for stacked items).</param>
        /// <returns><c>true</c> on success.</returns>
        public static async Task<bool> BagSlotMoveChest(BagSlot source, BagSlot dest, uint count = 1)
        {
            var itemToMove = source.Item;
            var stack = source.Count > 1;

            if (stack && count == 1)
            {
                count = source.Count;
            }

            Log.Information($"Moving {count} x {itemToMove.LocaleName()} from {source.BagId} to {dest.BagId}");
            source.FcChestMove(dest);

            if (stack)
            {
                //wait for inputnumeric
                if (!await Coroutine.Wait(10000, () => InputNumeric.IsOpen))
                {
                    Log.Error("Input Numeric didn't open");
                    return false;
                }

                InputNumeric.Ok(count);
            }

            if (await Coroutine.Wait(5000, () => InventoryHelpers.IsFCItemBusy))
            {
                await Coroutine.Wait(5000, () => !InventoryHelpers.IsFCItemBusy);
            }

            return true;
        }

        /// <summary>
        /// Closes the FC chest window and waits up to 5 seconds for it to close.
        /// Does nothing if the window is already closed.
        /// </summary>
        /// <returns><c>true</c> when the window is confirmed closed.</returns>
        public static async Task<bool> CloseChest()
        {
            if (FreeCompanyChest.Instance.IsOpen)
            {
                FreeCompanyChest.Instance.Close();
                await Coroutine.Wait(5000, () => !FreeCompanyChest.Instance.IsOpen);
            }

            await Coroutine.Sleep(1000);
            return !FreeCompanyChest.Instance.IsOpen;
        }

        /// <summary>
        /// Ensures the FC chest window is open. Travels to home world if needed, navigates to the
        /// <see cref="ClosestCompanyChest"/>, and refreshes bags via <see cref="RefreshChestBags"/>.
        /// </summary>
        /// <param name="gilOnly">When <c>true</c>, only the gil tab is refreshed.</param>
        /// <param name="crystalsOnly">When <c>true</c>, only the crystals tab is refreshed.</param>
        /// <returns><c>true</c> when the chest window is open.</returns>
        public static async Task<bool> MakeSureChestIsOpen(bool gilOnly = false, bool crystalsOnly = false)
        {
            if (FreeCompanyChest.Instance.IsOpen)
            {
                return true;
            }

            if (!WorldHelper.IsOnHomeWorld && !await WorldTravel.WorldTravel.MakeSureHome())
            {
                return false;
            }

            if (ClosestCompanyChest == null)
            {
                Log.Error("No company chest found");
                return false;
            }

            if (!await Navigation.GetToInteractNpc(ClosestCompanyChest, FreeCompanyChest.Instance))
            {
                return false;
            }

            await PingChecker.UpdatePing();
            await RefreshChestBags(gilOnly, crystalsOnly);

            return FreeCompanyChest.Instance.IsOpen;
        }

        /// <summary>
        /// Overload of <see cref="MakeSureChestIsOpen(bool,bool)"/> that opens the chest and refreshes
        /// only the specified <paramref name="bagid"/> tab via <see cref="RefreshChestBag"/>.
        /// </summary>
        /// <param name="bagid">The FC chest bag tab to load after opening.</param>
        /// <returns><c>true</c> when the chest window is open.</returns>
        public static async Task<bool> MakeSureChestIsOpen(InventoryBagId bagid)
        {
            if (FreeCompanyChest.Instance.IsOpen)
            {
                return true;
            }

            if (!WorldHelper.IsOnHomeWorld && !await WorldTravel.WorldTravel.MakeSureHome())
            {
                return false;
            }

            if (ClosestCompanyChest == null)
            {
                Log.Error("No company chest found");
                return false;
            }

            if (!await Navigation.GetToInteractNpc(ClosestCompanyChest, FreeCompanyChest.Instance))
            {
                return false;
            }

            await PingChecker.UpdatePing();
            await RefreshChestBag(bagid);

            return FreeCompanyChest.Instance.IsOpen;
        }

        /// <summary>
        /// Refreshes the FC chest bag data from the server for all accessible tabs.
        /// Only refreshes gil/crystals tabs when the player has the appropriate permission.
        /// Does nothing if the chest is not open.
        /// </summary>
        /// <param name="gilOnly">When <c>true</c>, refreshes only the gil tab.</param>
        /// <param name="crystalsOnly">When <c>true</c>, refreshes only the crystals tab.</param>
        public static async Task RefreshChestBags(bool gilOnly = false, bool crystalsOnly = false)
        {
            if (!FreeCompanyChest.Instance.IsOpen)
            {
                return;
            }

            if (FreeCompanyChest.Instance.GilPermission != CompanyChestPermission.NoAccess && !crystalsOnly)
            {
                Log.Information("Updating gil");
                await AgentFreeCompanyChest.Instance.LoadBag(InventoryBagId.GrandCompany_Gil);
            }

            if (gilOnly)
            {
                return;
            }

            if (FreeCompanyChest.Instance.CrystalsPermission != CompanyChestPermission.NoAccess)
            {
                Log.Information("Updating crystals");
                await AgentFreeCompanyChest.Instance.LoadBag(InventoryBagId.GrandCompany_Crystals);
            }

            if (crystalsOnly)
            {
                return;
            }

            foreach (var bag in AllBagIds)
            {
                await AgentFreeCompanyChest.Instance.LoadBag(bag);
            }
        }

        /// <summary>Refreshes data for a single <paramref name="bagId"/> tab. Does nothing if the chest is not open.</summary>
        /// <param name="bagId">The bag tab to reload from the server.</param>
        public static async Task RefreshChestBag(InventoryBagId bagId)
        {
            if (!FreeCompanyChest.Instance.IsOpen)
            {
                return;
            }

            await AgentFreeCompanyChest.Instance.LoadBag(bagId);
        }

        private sealed class BagSlotComparer : IEqualityComparer<BagSlot>
        {
            public bool Equals(BagSlot? x, BagSlot? y)
            {
                return y != null && x != null && x.RawItemId == y.RawItemId && x.Count + y.Count <= x.Item.StackSize;
            }

            public int GetHashCode(BagSlot obj)
            {
                return obj.Item.GetHashCode();
            }
        }

        private sealed class BagSlotComparerDeposit : IEqualityComparer<BagSlot>
        {
            public bool Equals(BagSlot? x, BagSlot? y)
            {
                return y != null && x != null && x.TrueItemId == y.TrueItemId && x.Count + y.Count <= x.Item.StackSize;
            }

            public int GetHashCode(BagSlot obj)
            {
                return obj.Item.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Specifies the direction of an FC chest transfer operation.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>Move items from the player's bags to the FC chest.</summary>
        Deposit,
        /// <summary>Move items from the FC chest to the player's bags.</summary>
        Withdrawal
    }
}