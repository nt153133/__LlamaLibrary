using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Utilities;

// ReSharper disable PossibleMultipleEnumeration

namespace LlamaLibrary.Helpers
{
    public static class CompanyChestHelper
    {
        public const uint HousingCompanyChest = 196627;

        private static readonly LLogger Log = new LLogger("CompanyChestHelper", Colors.BurlyWood);

        public static readonly uint[] CrystalIds = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

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

        public static readonly IReadOnlyDictionary<int, InventoryBagId> ChestTabBags = new Dictionary<int, InventoryBagId>
        {
            { 0, InventoryBagId.GrandCompany_Page1 },
            { 1, InventoryBagId.GrandCompany_Page2 },
            { 2, InventoryBagId.GrandCompany_Page3 },
            { 3, InventoryBagId.GrandCompany_Page4 },
            { 4, InventoryBagId.GrandCompany_Page5 },
        };

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

        public static Npc ClosestCompanyChest
        {
            get
            {
                if (!HousingHelper.IsInsideHouse || GameObjectManager.GetObjectByNPCId(HousingCompanyChest) == default)
                {
                    return NpcHelper.GetClosestNpc(ChestLocations);
                }

                return new Npc(GameObjectManager.GetObjectByNPCId(HousingCompanyChest));
            }
        }

        public static bool CanDepositGil => FreeCompanyChest.Instance.GilPermission is CompanyChestPermission.DepositOnly or CompanyChestPermission.FullAccess;
        public static bool CanWithdrawGil => FreeCompanyChest.Instance.GilPermission is CompanyChestPermission.FullAccess;

        public static bool CanDepositCrystals => FreeCompanyChest.Instance.CrystalsPermission is CompanyChestPermission.DepositOnly or CompanyChestPermission.FullAccess;
        public static bool CanWithdrawCrystals => FreeCompanyChest.Instance.CrystalsPermission is CompanyChestPermission.FullAccess;

        public static IEnumerable<int> DepositBagIndexes
        {
            get { return FreeCompanyChest.Instance.ItemTabPermissions.Where(i => i.Value is CompanyChestPermission.DepositOnly or CompanyChestPermission.FullAccess).Select(i => i.Key); }
        }

        public static IEnumerable<InventoryBagId> DepositBagIds => DepositBagIndexes.Select(i => ChestTabBags[i]);

        public static IEnumerable<int> WithdrawBagIndexes
        {
            get { return FreeCompanyChest.Instance.ItemTabPermissions.Where(i => i.Value is CompanyChestPermission.FullAccess).Select(i => i.Key); }
        }

        public static IEnumerable<InventoryBagId> WithdrawBagIds => WithdrawBagIndexes.Select(i => ChestTabBags[i]);

        public static IEnumerable<int> AllBagIndexes
        {
            get { return FreeCompanyChest.Instance.ItemTabPermissions.Where(i => i.Value is not CompanyChestPermission.NoAccess).Select(i => i.Key); }
        }

        public static IEnumerable<InventoryBagId> AllBagIds => AllBagIndexes.Select(i => ChestTabBags[i]);

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

        public static async Task<bool> DepositItems(IEnumerable<uint> itemIds)
        {
            var slots = InventoryManager.FilledSlots.Where(i => itemIds.Contains(i.TrueItemId));

            if (slots.Any())
            {
                return await DepositItems(slots);
            }

            return true;
        }

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

        public static async Task<bool> WithdrawItems(IEnumerable<uint> itemIds)
        {
            if (!await MakeSureChestIsOpen())
            {
                return false;
            }

            var slots = InventoryManager.GetBagsByInventoryBagId(WithdrawBagIds.ToArray()).SelectMany(x => x.FilledSlots).Where(i => itemIds.Contains(i.TrueItemId));

            if (slots.Any())
            {
                return await WithdrawItems(slots);
            }

            return true;
        }

        public static async Task<bool> WithdrawItems(IEnumerable<BagSlot> bagSlots)
        {
            if (!await MakeSureChestIsOpen())
            {
                return false;
            }

            foreach (var slot in bagSlots)
            {
                if (!await WithdrawItem(slot, false))
                {
                    return false;
                }
            }

            return await CloseChest();
        }

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

        public static async Task<bool> WithdrawItem(BagSlot bagSlot, bool closeWindow = true)
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

            var result = await BagSlotMoveChest(bagSlot, slot);

            await Coroutine.Sleep(200);
            await Coroutine.Wait(5000, () => AgentFreeCompanyChest.Instance.FullyLoaded);

            if (closeWindow)
            {
                result = result && await CloseChest();
            }

            return result && !(bagSlot.IsValid && bagSlot.IsFilled);
        }

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

            uint currentGil = AgentFreeCompanyChest.Instance.GilBalance;

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

            uint currentGil = AgentFreeCompanyChest.Instance.GilBalance;

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

        public static BagSlot GetNextOrStackSlot(BagSlot bagSlot, TransactionType transactionType)
        {
            IEnumerable<InventoryBagId> bagList;

            switch (transactionType)
            {
                case TransactionType.Deposit:
                    bagList = DepositBagIds;
                    break;
                case TransactionType.Withdrawal:
                    bagList = Inventory.InventoryBagIds;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
            }

            var bags = InventoryManager.GetBagsByInventoryBagId(bagList.ToArray()).SelectMany(x => x.FilledSlots);
            var eqx = new BagSlotComparer();
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

        public static async Task<bool> BagSlotMoveChest(BagSlot source, BagSlot dest, uint count = 1)
        {
            Item itemToMove = source.Item;
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

            if (!await Navigation.GetToInteractNpc(ClosestCompanyChest, FreeCompanyChest.Instance))
            {
                return false;
            }

            await PingChecker.UpdatePing();
            await RefreshChestBags(gilOnly, crystalsOnly);

            return FreeCompanyChest.Instance.IsOpen;
        }

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

        private sealed class BagSlotComparer : IEqualityComparer<BagSlot>
        {
            public bool Equals(BagSlot x, BagSlot y)
            {
                return y != null && x != null && x.RawItemId == y.RawItemId && x.Count + y.Count <= x.Item.StackSize;
            }

            public int GetHashCode(BagSlot obj)
            {
                return obj.Item.GetHashCode();
            }
        }
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal
    }
}