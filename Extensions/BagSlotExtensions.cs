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
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteAgents;

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

        public static class Offsets
        {
            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 56 48 83 EC ? 8B DA 41 0F B7 E8")]
            public static IntPtr ItemDiscardFunc;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 48 89 7C 24 ? 41 56 48 83 EC ? 8B FA 33 DB")]
            public static IntPtr ItemLowerQualityFunc;

            [Offset("Search 40 55 53 56 41 56 41 57 48 8D 6C 24 ? 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 45 ? 8D 82 ? ? ? ?")]
            public static IntPtr ItemSplitFunc;

            //6.3
            [Offset("Search 48 89 5C 24 ? 56 48 83 EC ? 80 3D ? ? ? ? ? 48 8B F2")]
            //pre 6.3 [OffsetCN("Search 48 89 91 ? ? ? ? 33 D2 C7 81 ? ? ? ? ? ? ? ?")]
            public static IntPtr MeldWindowFunc;

            [Offset("Search 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 41 0F B7 F8 8B DA")]
            public static IntPtr ExtractMateriaFunc;

            [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 83 7E ? ? 75 ? 48 8B 06 Add 3 TraceRelative")]
            public static IntPtr ExtractMateriaParam;

            //This client function does desynth, remove materia and reduce depending on the 2nd param
            [Offset("Search 40 57 41 54 41 55 41 56 41 57 48 83 EC ? 45 0F B7 F1")]
            public static IntPtr RemoveMateriaFunc;

            //6.51
            [Offset("Search BA ? ? ? ? E8 ? ? ? ? 33 D2 48 8B CD E8 ? ? ? ? 48 8B 7C 24 ? Add 1 Read32")]
            [OffsetCN("Search BA ? ? ? ? E8 ? ? ? ? 48 8B CD E8 ? ? ? ? 48 8B 7C 24 ? Add 1 Read32")]
            public static int DesynthId;

            //6.5
            //BA ? ? ? ? E8 ? ? ? ? 48 8B 7C 24 ? 48 8B 5C 24 ? 84 C0 Add 1 Read32
            [Offset("Search BA ? ? ? ? E8 ? ? ? ? 48 8B 7C 24 ? 48 8B 5C 24 ? 84 C0 Add 1 Read32")]
            [OffsetCN("Search BA ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 48 8B 4E ? 48 8B 01 Add 1 Read32")]
            public static int ReduceId;

            [Offset("Search 44 0F B7 7C 73 ? Add 5 Read8")]
            public static int BagSlotMateriaType;

            [Offset("Search 0F B6 74 18 ? 66 45 85 FF Add 4 Read8")]
            public static int BagSlotMateriaLevel;

            [Offset("Search BA ? ? ? ? E8 ? ? ? ? EB ? 44 0F B7 41 ? Add 1 Read32")]
            public static int RemoveMateriaId;

            [Offset("Search 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 83 B9 ? ? ? ? ? 41 8B F0 8B EA 48 8B F9 0F 85 ? ? ? ?")]
            public static IntPtr TradeBagSlot;

            [Offset("Search 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 8B CA 41 8B F1")]
            public static IntPtr BagSlotUseItem;

            [Offset("Search 48 89 6C 24 ? 56 41 56 41 57 48 83 EC ? 45 8B F9 45 0F B7 F0")]
            public static IntPtr RemoveFromSaddle;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 45 33 DB 41 8B F9 45 8B D3 41 0F B7 F0 8B EA 48 8B D9 48 8B C1 0F 1F 80 ? ? ? ? 80 38 ? 75 ? 41 FF C3 49 FF C2 48 83 C0 ? 49 81 FA ? ? ? ? 7C ? EB ? 49 63 C3 48 6B D0 ? 48 03 D3 C6 02 ? 74 ? C7 42 ? ? ? ? ? 44 8B C7 89 6A ? 66 89 72 ? 89 7A ? 8B 81 ? ? ? ? 89 42 ? 0F B7 D6 44 8B 89 ? ? ? ? 8B CD E8 ? ? ? ? 8B 8B ? ? ? ? B8 ? ? ? ? FF C1 F7 E1 8B C1 2B C2 ? ? 03 C2 C1 E8 ? 69 C0 ? ? ? ? 2B C8 0F BA E9 ? 89 8B ? ? ? ? 48 8B 5C 24 ? 48 8B 6C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? 66 83 FA ?")]
            public static IntPtr RetainerRetrieveQuantity;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 0F B6 DA 48 8B F9")]
            public static IntPtr EntrustRetainerFunc;

            [Offset("Search 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 80 B9 ? ? ? ? ? 41 8B F0")]
            public static IntPtr SellFunc;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 41 56 48 83 EC ? 45 8B F1")]
            public static IntPtr AddToSaddle;

            [Offset("Search 40 53 56 57 41 56 41 57 48 83 EC ? 45 33 FF")]
            public static IntPtr FCChestMove;

            [Offset("Search 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 63 F2 48 8B F9")]
            public static IntPtr PlaceAetherWheel;

            [Offset("Search 48 8B 05 ? ? ? ? 48 85 C0 74 ? 83 B8 ? ? ? ? ? 75 ? E8 ? ? ? ? Add 3 TraceRelative")]
            public static IntPtr EventHandlerOff;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 8B E9 41 0F B7 D9 48 8B 0D ? ? ? ? 41 8B F8 0F B7 F2 E8 ? ? ? ? 48 8B C8 48 85 C0 74 ? 80 BC 24 ? ? ? ? ?")]
            internal static IntPtr MeldItem;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 48 89 7C 24 ? 41 56 48 83 EC ? 45 33 F6 41 8B F1")]
            internal static IntPtr DyeItem;

            [Offset("Search 0F B6 43 ? 88 47 ? 80 7B ? ? 74 ? 48 8B CB E8 ? ? ? ? 48 85 C0 74 ? 48 8B CB E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? EB ? 8B 43 ? Add 3 Read8")]
            public static int StainId;

            [Offset("Search 8B 48 ? 40 88 6C 24 ? Add 2 Read8")]
            internal static int PlayerMeldOffset;

            [Offset("Search 41 56 41 57 48 81 EC ? ? ? ? 83 B9 ? ? ? ? ? 4C 8B F2")]
            public static IntPtr StoreroomToInventory;

            [Offset("Search 40 53 41 55 48 83 EC ? 48 8B DA")]
            public static IntPtr InventoryToStoreroom;

            [Offset("Search E8 ? ? ? ? 89 83 ? ? ? ? C7 44 24 ? ? ? ? ? TraceCall")]
            internal static IntPtr GetPostingPriceSlot;

            private static IntPtr _eventHandler = IntPtr.Zero;

            public static IntPtr EventHandler
            {
                get
                {
                    if (_eventHandler == IntPtr.Zero)
                    {
                        _eventHandler = Core.Memory.Read<IntPtr>(EventHandlerOff);
                    }

                    return _eventHandler;
                }
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
                        return Core.Memory.CallInjected64<uint>(Offsets.ItemSplitFunc,
                                                                Memory.Offsets.g_InventoryManager,
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
                Core.Memory.CallInjected64<uint>(Offsets.ItemDiscardFunc,
                                                 Memory.Offsets.g_InventoryManager,
                                                 (uint)bagSlot.BagId,
                                                 bagSlot.Slot);
            }
        }

        public static void Desynth(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjected64<uint>(Offsets.RemoveMateriaFunc,
                                                 Offsets.EventHandler,
                                                 Offsets.DesynthId,
                                                 (uint)bagSlot.BagId,
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
                        Core.Memory.CallInjected64<uint>(Offsets.ItemLowerQualityFunc,
                                                         Memory.Offsets.g_InventoryManager,
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
                Core.Memory.CallInjected64<uint>(Offsets.RemoveMateriaFunc,
                                                 Offsets.EventHandler,
                                                 Offsets.ReduceId,
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
                    Core.Memory.CallInjected64<uint>(Offsets.RetainerRetrieveQuantity,
                                                     Memory.Offsets.g_InventoryManager,
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
                    Core.Memory.CallInjected64<uint>(Offsets.EntrustRetainerFunc,
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
                    Core.Memory.CallInjected64<uint>(Offsets.SellFunc,
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
                    Core.Memory.CallInjected64<uint>(Offsets.SellFunc,
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
                Core.Memory.CallInjected64<uint>(Offsets.RemoveMateriaFunc,
                                                 Offsets.EventHandler,
                                                 Offsets.RemoveMateriaId,
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
                    Core.Memory.CallInjected64<uint>(Offsets.ExtractMateriaFunc,
                                                     Offsets.ExtractMateriaParam,
                                                     (uint)bagSlot.BagId,
                                                     bagSlot.Slot);
                }
            }
        }

        public static void AffixMateria(this BagSlot Equipment, BagSlot Materia, bool BulkMeld)
        {
            var offset = Core.Memory.Read<int>(Core.Me.Pointer + Offsets.PlayerMeldOffset);
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjected64<IntPtr>(Offsets.MeldItem,
                                                       (int)Equipment.BagId,
                                                       (short)Equipment.Slot,
                                                       (int)Materia.BagId,
                                                       (short)Materia.Slot,
                                                       offset,
                                                       (byte)(BulkMeld ? 1 : 0));
                }
            }
        }

        public static void OpenMeldInterface(this BagSlot bagSlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjected64<uint>(Offsets.MeldWindowFunc,
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
                    return Core.Memory.CallInjected64<uint>(Offsets.GetPostingPriceSlot,
                                                            Memory.Offsets.g_InventoryManager,
                                                            slot.Slot);
                }
            }
        }

        public static bool HasMateria(this BagSlot bagSlot)
        {
            var materiaType = Core.Memory.ReadArray<ushort>(bagSlot.Pointer + Offsets.BagSlotMateriaType, 5);
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
            var materiaType = Core.Memory.ReadArray<ushort>(bagSlot.Pointer + Offsets.BagSlotMateriaType, 5);
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
            var materiaType = Core.Memory.ReadArray<ushort>(bagSlot.Pointer + Offsets.BagSlotMateriaType, 5);
            var materiaLevel = Core.Memory.ReadArray<byte>(bagSlot.Pointer + Offsets.BagSlotMateriaLevel, 5);
            var materia = new List<MateriaItem>();

            for (var i = 0; i < 5; i++)
            {
                if (materiaType[i] > 0)
                {
                    materia.Add(ResourceManager.MateriaList.Value[materiaType[i]].First(j => j.Tier == materiaLevel[i]));
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
                    result = Core.Memory.CallInjected64<uint>(Offsets.TradeBagSlot,
                                                              Memory.Offsets.g_InventoryManager,
                                                              bagSlot.Slot,
                                                              (uint)bagSlot.BagId);
                }
            }

            if (result != 0)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    Core.Memory.CallInjected64<uint>(Offsets.TradeBagSlot,
                                                     Memory.Offsets.g_InventoryManager,
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
            return AddToSaddleCall(Memory.Offsets.g_InventoryManager, (uint)bagSlot.BagId, bagSlot.Slot, amount) == IntPtr.Zero;
        }

        public static bool RemoveFromSaddlebagQuantity(this BagSlot bagSlot, uint amount)
        {
            return RemoveFromSaddleCall(Memory.Offsets.g_InventoryManager, (uint)bagSlot.BagId, bagSlot.Slot, amount) == IntPtr.Zero;
        }

        public static void UseItemRaw(this BagSlot bagSlot)
        {
            BagSlotUseItemCall(Memory.Offsets.g_InventoryManager, bagSlot.TrueItemId, (uint)bagSlot.BagId, bagSlot.Slot);
        }

        internal static byte BagSlotUseItemCall(IntPtr InventoryManager, uint TrueItemId, uint inventoryContainer, int inventorySlot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjected64<byte>(Offsets.BagSlotUseItem,
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
                    return Core.Memory.CallInjected64<IntPtr>(Offsets.RemoveFromSaddle,
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
                    return Core.Memory.CallInjected64<IntPtr>(Offsets.AddToSaddle,
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
                    return Core.Memory.CallInjected64<IntPtr>(Offsets.PlaceAetherWheel,
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
                return Core.Memory.CallInjected64<IntPtr>(Offsets.FCChestMove,
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
                    Core.Memory.CallInjected64<uint>(Offsets.StoreroomToInventory,
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
                    Core.Memory.CallInjected64<uint>(Offsets.InventoryToStoreroom,
                                                     HousingHelper.PositionPointer,
                                                     bagSlot.Pointer);
                }
            }

            return true;
        }

        public static byte StainId(this BagSlot bagSlot)
        {
            return Core.Memory.Read<byte>(bagSlot.Pointer + Offsets.StainId);
        }

        public static byte DyeItem(this BagSlot item, BagSlot dye)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using (Core.Memory.TemporaryCacheState(false))
                {
                    return Core.Memory.CallInjected64<byte>(Offsets.DyeItem,
                                                            Memory.Offsets.g_InventoryManager,
                                                            item.BagId,
                                                            item.Slot,
                                                            dye.BagId,
                                                            dye.Slot);
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

            if (await Coroutine.Wait(10000, () => Core.Memory.Read<uint>(LlamaLibrary.Memory.Offsets.Conditions + LlamaLibrary.Memory.Offsets.DesynthLock) != 0))
            {
                return await Coroutine.Wait(10000, () => Core.Memory.Read<uint>(LlamaLibrary.Memory.Offsets.Conditions + LlamaLibrary.Memory.Offsets.DesynthLock) == 0);
            }

            return false;
        }
    }
}