using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Extensions
{
    public static class BagSlotExtensions
    {
        private static readonly InventoryBagId[] RetainerBagIds =
        {
            InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2, InventoryBagId.Retainer_Page3,
            InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5, InventoryBagId.Retainer_Page6,
            InventoryBagId.Retainer_Page7
        };

        private static IntPtr _eventHandler = IntPtr.Zero;

        public static IntPtr EventHandler
        {
            get
            {
                if (_eventHandler == IntPtr.Zero)
                {
                    _eventHandler = Core.Memory.Read<IntPtr>(BagSlotExtensionsOffsets.EventHandlerOff);
                }

                return _eventHandler;
            }
        }

        public static bool Split(this BagSlot bagSlot, int amount)
        {
            if (bagSlot.Count > amount)
            {
                lock (Core.Memory.Executor.AssemblyLock)
                {
                    using (Core.Memory.TemporaryCacheState(false))
                    {
                        return Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ItemSplitFunc,
                                                                    Offsets.g_InventoryManager,
                                                                    (uint)bagSlot.BagId,
                                                                    bagSlot.Slot,
                                                                    amount) == 0;
                    }
                }
            }

            return false;
        }

        public static void Discard(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ItemDiscardFunc,
                                                     Offsets.g_InventoryManager,
                                                     (uint)bagSlot.BagId,
                                                     bagSlot.Slot);
            }
        }

        public static void Desynth(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RemoveMateriaFunc,
                                                     EventHandler,
                                                     BagSlotExtensionsOffsets.DesynthId,
                                                     bagSlot.BagId,
                                                     bagSlot.Slot,
                                                     2);
            }
        }

        public static bool LowerQuality(this BagSlot bagSlot)
        {
            if (bagSlot.IsHighQuality || bagSlot.IsCollectable)
            {
                lock (Core.Memory.Executor.AssemblyLock)
                {
                    using (Core.Memory.TemporaryCacheState(false))
                    {
                        Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ItemLowerQualityFunc,
                                                             Offsets.g_InventoryManager,
                                                             (uint)bagSlot.BagId,
                                                             bagSlot.Slot);
                    }
                }

                return !bagSlot.IsHighQuality;
            }

            return false;
        }

        public static void Reduce(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RemoveMateriaFunc,
                                                     EventHandler,
                                                     BagSlotExtensionsOffsets.ReduceId,
                                                     (uint)bagSlot.BagId,
                                                     bagSlot.Slot,
                                                     0);
            }
        }

        public static void RetainerRetrieveQuantity(this BagSlot bagSlot, int amount)
        {
            if (bagSlot.Count < amount)
            {
                amount = (int)bagSlot.Count;
            }

            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RetainerRetrieveQuantity,
                                                         Offsets.g_InventoryManager,
                                                         (uint)bagSlot.BagId,
                                                         bagSlot.Slot,
                                                         amount);
                }
            }
        }

        public static void RetainerEntrustQuantity(this BagSlot bagSlot, int amount)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.EntrustRetainerFunc,
                                                         AgentRetainerInventory.Instance.Pointer,
                                                         0,
                                                         (uint)bagSlot.BagId,
                                                         bagSlot.Slot,
                                                         amount);
                }
            }
        }

        public static void RetainerSellItem(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.SellFunc,
                                                         AgentRetainerInventory.Instance.RetainerShopPointer,
                                                         bagSlot.Slot,
                                                         (uint)bagSlot.BagId,
                                                         0);
                }
            }
        }

        public static void NpcSellItem(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.SellFunc,
                                                         Shop.ActiveShopPtr,
                                                         bagSlot.Slot,
                                                         (uint)bagSlot.BagId,
                                                         0);
                }
            }
        }

        public static void RemoveMateria(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RemoveMateriaFunc,
                                                     EventHandler,
                                                     BagSlotExtensionsOffsets.RemoveMateriaId,
                                                     (uint)bagSlot.BagId,
                                                     bagSlot.Slot,
                                                     0);
            }
        }

        public static void ExtractMateria(this BagSlot bagSlot)
        {
            if ((int)bagSlot.SpiritBond != 100)
            {
                return;
            }

            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ExtractMateriaFunc,
                                                         BagSlotExtensionsOffsets.ExtractMateriaParam,
                                                         (uint)bagSlot.BagId,
                                                         bagSlot.Slot);
                }
            }
        }

        public static void AffixMateria(this BagSlot Equipment, BagSlot Materia, bool BulkMeld)
        {

            if (BulkMeld)
            {
                throw new ArgumentException("Bulk melding no longer supported", "BulkMeld");
            }

            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.MeldItem,
                                                           IntPtr.Zero,//Really should be the MateriaRequestManager, but it isn't used in the function
                                                           (int)Equipment.BagId,
                                                           (short)Equipment.Slot,
                                                           (int)Materia.BagId,
                                                           (short)Materia.Slot);
                }
            }
        }

        public static void OpenMeldInterface(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.MeldWindowFunc,
                                                         AgentMeld.Instance.Pointer,
                                                         bagSlot.Pointer);
                }
            }
        }

        public static uint PostingPrice(this BagSlot slot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.GetPostingPriceSlot,
                                                                Offsets.g_InventoryManager,
                                                                slot.Slot);
                }
            }
        }

        public static bool HasMateria(this BagSlot bagSlot)
        {
            var materiaType = Core.Memory.ReadArray<ushort>(bagSlot.Pointer + BagSlotExtensionsOffsets.BagSlotMateriaType, 5);
            for (var i = 0; i < 5; i++)
            {
                if (materiaType[i] > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static int MateriaCount(this BagSlot bagSlot)
        {
            var materiaType = Core.Memory.ReadArray<ushort>(bagSlot.Pointer + BagSlotExtensionsOffsets.BagSlotMateriaType, 5);
            var count = 0;
            for (var i = 0; i < 5; i++)
            {
                if (materiaType[i] > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public static List<MateriaItem> Materia(this BagSlot bagSlot)
        {
            var materiaType = Core.Memory.ReadArray<ushort>(bagSlot.Pointer + BagSlotExtensionsOffsets.BagSlotMateriaType, 5);
            var materiaLevel = Core.Memory.ReadArray<byte>(bagSlot.Pointer + BagSlotExtensionsOffsets.BagSlotMateriaLevel, 5);
            var materia = new List<MateriaItem>();

            for (var i = 0; i < 5; i++)
            {
                if (materiaType[i] <= 0)
                {
                    continue;
                }

                try
                {
                    materia.Add(ResourceManager.MateriaList.Value[materiaType[i]].First(j => j.Tier == materiaLevel[i]));
                }
                catch (Exception e)
                {
                    ff14bot.Helpers.Logging.WriteDiagnostic(e.Message);
                    // ignored
                }
            }

            return materia;
        }

        public static void TradeItem(this BagSlot bagSlot)
        {
            uint result;
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    result = Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.TradeBagSlot,
                                                                  Offsets.g_InventoryManager,
                                                                  bagSlot.Slot,
                                                                  (uint)bagSlot.BagId);
                }
            }

            if (result != 0)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.TradeBagSlot,
                                                         Offsets.g_InventoryManager,
                                                         bagSlot.Slot,
                                                         (uint)bagSlot.BagId);
                }
            }
        }

        public static bool CanTrade(this BagSlot slot)
        {
            return !slot.Item.Untradeable && !slot.IsCollectable && !(slot.SpiritBond > 0);
        }

        public static void PlaceAetherWheel(this BagSlot bagSlot)
        {
            PlaceAetherWheel((uint)bagSlot.BagId, bagSlot.Slot);
        }

        public static bool AddToSaddlebagQuantity(this BagSlot bagSlot, uint amount)
        {
            return AddToSaddleCall(Offsets.g_InventoryManager, (uint)bagSlot.BagId, bagSlot.Slot, amount) == IntPtr.Zero;
        }

        public static bool RemoveFromSaddlebagQuantity(this BagSlot bagSlot, uint amount)
        {
            return RemoveFromSaddleCall(Offsets.g_InventoryManager, (uint)bagSlot.BagId, bagSlot.Slot, amount) == IntPtr.Zero;
        }

        public static void UseItemRaw(this BagSlot bagSlot)
        {
            BagSlotUseItemCall(Offsets.g_InventoryManager, bagSlot.TrueItemId, (uint)bagSlot.BagId, bagSlot.Slot);
        }

        internal static byte BagSlotUseItemCall(IntPtr InventoryManager, uint TrueItemId, uint inventoryContainer, int inventorySlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjectedWraper<byte>(BagSlotExtensionsOffsets.BagSlotUseItem,
                                                                InventoryManager,
                                                                TrueItemId,
                                                                inventoryContainer,
                                                                inventorySlot);
                }
            }
        }

        internal static IntPtr RemoveFromSaddleCall(IntPtr InventoryManager, uint inventoryContainer, ushort inventorySlot, uint count)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.RemoveFromSaddle,
                                                                  InventoryManager,
                                                                  inventoryContainer,
                                                                  inventorySlot,
                                                                  count);
                }
            }
        }

        internal static IntPtr AddToSaddleCall(IntPtr InventoryManager, uint inventoryContainer, ushort inventorySlot, uint count)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.AddToSaddle,
                                                                  InventoryManager,
                                                                  inventoryContainer,
                                                                  inventorySlot,
                                                                  count);
                }
            }
        }

        internal static IntPtr PlaceAetherWheel(uint inventoryContainer, ushort inventorySlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.PlaceAetherWheel,
                                                                  AgentBagSlot.Instance.PointerForAether,
                                                                  (int)inventorySlot,
                                                                  inventoryContainer);
                }
            }
        }

        internal static IntPtr FcChestCall(uint sourceContainer, uint sourceSlot, uint destContainer, uint destSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.FCChestMove,
                                                              AgentFreeCompanyChest.Instance.Pointer,
                                                              sourceContainer,
                                                              sourceSlot,
                                                              destContainer,
                                                              destSlot);
            }
        }

        public static void FcChestMove(this BagSlot sourceBagSlot, BagSlot destBagSlot)
        {
            FcChestCall((uint)sourceBagSlot.BagId, sourceBagSlot.Slot, (uint)destBagSlot.BagId, destBagSlot.Slot);
        }

        public static bool StoreroomReturnToInventory(this BagSlot bagSlot)
        {
            if (!bagSlot.IsFilled || InventoryManager.FreeSlots < 1)
            {
                return false;
            }

            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.StoreroomToInventory,
                                                         HousingHelper.PositionPointer,
                                                         bagSlot.Pointer);
                }
            }

            return true;
        }

        public static bool InventoryToStoreroom(this BagSlot bagSlot)
        {
            if (!bagSlot.IsFilled)
            {
                return false;
            }

            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.InventoryToStoreroom,
                                                         HousingHelper.PositionPointer,
                                                         bagSlot.Pointer);
                }
            }

            return true;
        }

        public static byte StainId(this BagSlot bagSlot)
        {
            return Core.Memory.Read<byte>(bagSlot.Pointer + BagSlotExtensionsOffsets.StainId);
        }

        public static byte DyeItem(this BagSlot item, BagSlot dye)
        {
            using var bagid = Core.Memory.CreateAllocatedMemory(8);
            using var bagslot = Core.Memory.CreateAllocatedMemory(4);
            bagid.AllocateOfChunk("bagid",8);
            bagid.Write("bagid",0x270F00000000+ (long)dye.BagId);
            bagslot.AllocateOfChunk("bagslot",4);
            int test = -1 << 16;
            int test2 = test + dye.Slot;

            bagslot.Write("bagslot", test2);

            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjectedWraper<byte>(BagSlotExtensionsOffsets.DyeItem,
                                                                Offsets.g_InventoryManager,
                                                                item.BagId,
                                                                item.Slot,
                                                                bagid.Address,
                                                                bagslot.Address);
                }
            }
        }

        public static void RetainerEntrustQuantity(this BagSlot bagSlot, uint amount)
        {
            bagSlot.RetainerEntrustQuantity((int)amount);
        }

        public static void RetainerRetrieveQuantity(this BagSlot bagSlot, uint amount)
        {
            bagSlot.RetainerRetrieveQuantity((int)amount);
        }

        public static string ItemName(this BagSlot bagSlot)
        {
            return GetItemName(bagSlot.TrueItemId);
        }

        public static string GetItemName(uint ItemId)
        {
            if (ItemId >= 1000000)
            {
                return $"{DataManager.GetItem(ItemId - 1000000).CurrentLocaleName} (HQ)";
            }

            return DataManager.GetItem(ItemId).CurrentLocaleName;
        }

        public const int DefaultBagSlotMoveWait = 600;

        public static async Task<bool> BagSlotMoveWait(BagSlot bagSlot, uint curSlotCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var sw = Stopwatch.StartNew();
            if (await Coroutine.Wait(waitMs, () => !bagSlot.IsValid || !bagSlot.IsFilled || bagSlot.Count < curSlotCount))
            {
                sw.Stop();
                int remainingMs;
                remainingMs = Math.Min(Math.Min((PingChecker.CurrentPing * 3) - (int)sw.ElapsedMilliseconds, waitMs - (int)sw.ElapsedMilliseconds), DefaultBagSlotMoveWait - (int)sw.ElapsedMilliseconds);

                if (remainingMs > 0)
                {
                    //ff14bot.Helpers.Logging.WriteDiagnostic($"BagSlotMoveWait: Ping {PingChecker.CurrentPing}ms, remaining {remainingMs}ms, elapsed {sw.ElapsedMilliseconds}ms");
                    await Coroutine.Sleep(remainingMs);
                    return true;
                }

                //ff14bot.Helpers.Logging.WriteDiagnostic($"BagSlotMoveWait: Ping {PingChecker.CurrentPing}ms, remaining {remainingMs}ms, elapsed {sw.ElapsedMilliseconds}ms");

                return true;
            }

            sw.Stop();
            //ff14bot.Helpers.Logging.WriteDiagnostic("BagSlotMoveWait: Timeout waiting for bag slot to move.");
            return false;
        }

        public static async Task<bool> BagSlotNotFilledWait(BagSlot bagSlot, int waitMs = DefaultBagSlotMoveWait)
        {
            var sw = new Stopwatch();
            sw.Start();
            if (await Coroutine.Wait(waitMs * 2, () => !bagSlot.IsFilled))
            {
                sw.Stop();
                var remainingMs = waitMs - (int)sw.ElapsedMilliseconds;
                if (remainingMs > 0)
                {
                    await Coroutine.Sleep(remainingMs);
                }

                return true;
            }

            sw.Stop();
            return false;
        }

        public static async Task<bool> TryAddToSaddlebag(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.AddToSaddlebagQuantity(moveCount);
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        public static async Task<bool> TryRemoveFromSaddlebag(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RemoveFromSaddlebagQuantity(moveCount);
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        public static async Task<bool> TrySellItem(this BagSlot bagSlot, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RetainerSellItem();
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        public static async Task<bool> TryEntrustToRetainer(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RetainerEntrustQuantity(moveCount);
            if (!await BagSlotMoveWait(bagSlot, curSlotCount, waitMs))
            {
                if (InventoryManager.GetBagsByInventoryBagId(RetainerBagIds).Any(i => i.FreeSlots > 0))
                {
                    bagSlot.Move(InventoryManager.GetBagsByInventoryBagId(RetainerBagIds).First(i => i.FreeSlots >= 1).GetFirstFreeSlot());
                    return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
                }

                return false;
            }

            return true;
        }

        public static async Task<bool> TryRetrieveFromRetainer(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RetainerRetrieveQuantity(moveCount);
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        public static async Task<bool> TryStoreroomReturnToInventory(this BagSlot bagSlot, int waitMs = DefaultBagSlotMoveWait)
        {
            if (bagSlot.StoreroomReturnToInventory())
            {
                if (await Coroutine.Wait(5000, () => !bagSlot.IsFilled || SelectYesno.IsOpen))
                {
                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                        return await BagSlotNotFilledWait(bagSlot, 2000);
                    }
                }

                return true;
            }

            return false;
        }

        public static async Task<bool> TryInventoryToStoreroom(this BagSlot bagSlot, int waitMs = DefaultBagSlotMoveWait)
        {
            if (bagSlot.InventoryToStoreroom())
            {
                return await Coroutine.Wait(5000, () => !bagSlot.IsFilled);
            }

            return false;
        }

        public static async Task<bool> TryDyeItem(this BagSlot item, BagSlot dye)
        {
            var result = item.DyeItem(dye);
            if (result != 1)
            {
                return false;
            }

            if (await Coroutine.Wait(10000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) != 0))
            {
                return await Coroutine.Wait(10000, () => Core.Memory.Read<uint>(Offsets.Conditions + Offsets.DesynthLock) == 0);
            }

            return false;
        }
    }
}
