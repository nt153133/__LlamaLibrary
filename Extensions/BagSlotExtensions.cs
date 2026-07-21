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

        /// <summary>
        /// Gets the pointer to the game's event handler, retrieved from memory.
        /// </summary>
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

        /// <summary>
        /// Splits a specified amount from the item stack in this bag slot into a new stack.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the stack to split.</param>
        /// <param name="amount">The number of items to split into the new stack.</param>
        /// <returns><see langword="true"/> if the split operation was successful; otherwise <see langword="false"/>.</returns>
        public static bool Split(this BagSlot bagSlot, int amount)
        {
            if (bagSlot.Count > amount)
            {
                return Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ItemSplitFunc,
                                                            Offsets.g_InventoryManager,
                                                            (uint)bagSlot.BagId,
                                                            bagSlot.Slot,
                                                            amount) == 0;
            }

            return false;
        }

        /// <summary>
        /// Discards the item(s) contained in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot to discard.</param>
        public static void Discard(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ItemDiscardFunc,
                                                 Offsets.g_InventoryManager,
                                                 (uint)bagSlot.BagId,
                                                 bagSlot.Slot);
        }

        /// <summary>
        /// Initiates the desynthesis process for the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to desynthesize.</param>
        public static void Desynth(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RemoveMateriaFunc,
                                                 EventHandler,
                                                 BagSlotExtensionsOffsets.DesynthId,
                                                 bagSlot.BagId,
                                                 bagSlot.Slot,
                                                 2);
        }

        /// <summary>
        /// Lowers the quality of the item in this bag slot (e.g., from High Quality to Normal Quality).
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to lower in quality.</param>
        /// <returns><see langword="true"/> if the quality was successfully lowered; otherwise <see langword="false"/>.</returns>
        public static bool LowerQuality(this BagSlot bagSlot)
        {
            if (bagSlot.IsHighQuality || bagSlot.IsCollectable)
            {
                Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ItemLowerQualityFunc,
                                                     Offsets.g_InventoryManager,
                                                     (uint)bagSlot.BagId,
                                                     bagSlot.Slot);

                return !bagSlot.IsHighQuality;
            }

            return false;
        }

        /// <summary>
        /// Initiates aetherial reduction on the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to reduce.</param>
        public static void Reduce(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RemoveMateriaFunc,
                                                 EventHandler,
                                                 BagSlotExtensionsOffsets.ReduceId,
                                                 bagSlot.BagId,
                                                 bagSlot.Slot,
                                                 0);
        }

        /// <summary>
        /// Retrieves a specified quantity of items from a retainer's inventory into the player's inventory.
        /// </summary>
        /// <param name="bagSlot">The retainer bag slot to retrieve items from.</param>
        /// <param name="amount">The quantity of items to retrieve.</param>
        public static void RetainerRetrieveQuantity(this BagSlot bagSlot, int amount)
        {
            if (bagSlot.Count < amount)
            {
                amount = (int)bagSlot.Count;
            }

            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RetainerRetrieveQuantity,
                                                 Offsets.g_InventoryManager,
                                                 (uint)bagSlot.BagId,
                                                 bagSlot.Slot,
                                                 amount);
        }

        /// <summary>
        /// Entrusts a specified quantity of items from the player's inventory to a retainer.
        /// </summary>
        /// <param name="bagSlot">The player bag slot containing items to entrust.</param>
        /// <param name="amount">The quantity of items to entrust.</param>
        public static void RetainerEntrustQuantity(this BagSlot bagSlot, int amount)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.EntrustRetainerFunc,
                                                 AgentRetainerInventory.Instance.Pointer,
                                                 0,
                                                 (uint)bagSlot.BagId,
                                                 bagSlot.Slot,
                                                 amount);
        }

        /// <summary>
        /// Sells the item in this bag slot to a retainer.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to sell.</param>
        public static void RetainerSellItem(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.SellFunc,
                                                 AgentRetainerInventory.Instance.RetainerShopPointer,
                                                 bagSlot.Slot,
                                                 (uint)bagSlot.BagId,
                                                 0);
        }

        /// <summary>
        /// Sells the item in this bag slot to an NPC merchant.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to sell.</param>
        public static void NpcSellItem(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.SellFunc,
                                                 Shop.ActiveShopPtr,
                                                 bagSlot.Slot,
                                                 (uint)bagSlot.BagId,
                                                 0);
        }

        /// <summary>
        /// Removes all materia from the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to remove materia from.</param>
        public static void RemoveMateria(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.RemoveMateriaFunc,
                                                 EventHandler,
                                                 BagSlotExtensionsOffsets.RemoveMateriaId,
                                                 bagSlot.BagId,
                                                 bagSlot.Slot,
                                                 0);
        }

        /// <summary>
        /// Extracts materia from the item in this bag slot if it has reached 100% spiritbond.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to extract materia from.</param>
        public static void ExtractMateria(this BagSlot bagSlot)
        {
            if ((int)bagSlot.SpiritBond != 100)
            {
                return;
            }

            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.ExtractMateriaFunc,
                                                 BagSlotExtensionsOffsets.ExtractMateriaParam,
                                                 bagSlot.BagId,
                                                 bagSlot.Slot);
        }

        /// <summary>
        /// Affixes a piece of materia to a piece of equipment.
        /// </summary>
        /// <param name="Equipment">The equipment bag slot to receive the materia.</param>
        /// <param name="Materia">The bag slot containing the materia to be affixed.</param>
        /// <param name="BulkMeld">If <see langword="true"/>, attempts to perform a bulk meld operation.</param>
        public static void AffixMateria(this BagSlot Equipment, BagSlot Materia, bool BulkMeld)
        {
            var offset = Core.Memory.Read<int>(Core.Me.Pointer + BagSlotExtensionsOffsets.PlayerMeldOffset);
            Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.MeldItem,
                                                   (int)Equipment.BagId,
                                                   (short)Equipment.Slot,
                                                   (int)Materia.BagId,
                                                   (short)Materia.Slot,
                                                   offset,
                                                   BulkMeld ? 1 : 0);
        }

        /// <summary>
        /// Opens the materia melding interface for the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot to open the melding interface for.</param>
        public static void OpenMeldInterface(this BagSlot bagSlot)
        {
            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.MeldWindowFunc,
                                                 AgentMeld.Instance.Pointer,
                                                 bagSlot.Pointer);
        }

        /// <summary>
        /// Retrieves the suggested posting price for the item in this bag slot from the game's inventory manager.
        /// </summary>
        /// <param name="slot">The bag slot to check.</param>
        /// <returns>The posting price as a <see cref="uint"/>.</returns>
        public static uint PostingPrice(this BagSlot slot)
        {
            return Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.GetPostingPriceSlot,
                                                        Offsets.g_InventoryManager,
                                                        slot.Slot);
        }

        /// <summary>
        /// Checks if the item in this bag slot has any materia affixed to it.
        /// </summary>
        /// <param name="bagSlot">The bag slot to check.</param>
        /// <returns><see langword="true"/> if at least one materia is present; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Counts the number of materia pieces affixed to the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot to check.</param>
        /// <returns>The number of affixed materia pieces (up to 5).</returns>
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

        /// <summary>
        /// Retrieves a list of <see cref="MateriaItem"/> objects representing the materia affixed to the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot to retrieve materia information for.</param>
        /// <returns>A list of <see cref="MateriaItem"/> instances.</returns>
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

                if (!ResourceManager.MateriaList.Value.TryGetValue(materiaType[i], out var materiaByTier))
                {
                    continue;
                }

                var matchingMateria = materiaByTier.FirstOrDefault(j => j.Tier == materiaLevel[i]);
                if (matchingMateria != null)
                {
                    materia.Add(matchingMateria);
                }
            }

            return materia;
        }

        /// <summary>
        /// Adds the item in this bag slot to the current trade window.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to trade.</param>
        public static void TradeItem(this BagSlot bagSlot)
        {
            uint result;
            result = Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.TradeBagSlot,
                                                          Offsets.g_InventoryManager,
                                                          bagSlot.Slot,
                                                          (uint)bagSlot.BagId);

            if (result != 0)
            {
                Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.TradeBagSlot,
                                                     Offsets.g_InventoryManager,
                                                     bagSlot.Slot,
                                                     (uint)bagSlot.BagId);
            }
        }

        /// <summary>
        /// Determines whether the item in this bag slot is eligible for trading with other players.
        /// </summary>
        /// <param name="slot">The bag slot to check.</param>
        /// <returns>
        /// <see langword="true"/> if the item is not untradeable, not a collectable, and has no spiritbond;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool CanTrade(this BagSlot slot)
        {
            return !slot.Item.Untradeable && !slot.IsCollectable && !(slot.SpiritBond > 0);
        }

        /// <summary>
        /// Places an aetherial wheel from this bag slot into a nearby housing aetherial wheel stand.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the aetherial wheel.</param>
        public static void PlaceAetherWheel(this BagSlot bagSlot)
        {
            PlaceAetherWheel((uint)bagSlot.BagId, bagSlot.Slot);
        }

        /// <summary>
        /// Attempts to move a specified quantity of items from the player's inventory to the chocobo saddlebag.
        /// </summary>
        /// <param name="bagSlot">The player bag slot containing the items.</param>
        /// <param name="amount">The quantity to move.</param>
        /// <returns><see langword="true"/> if the operation was successful (resulted in a null pointer return from memory call); otherwise <see langword="false"/>.</returns>
        public static bool AddToSaddlebagQuantity(this BagSlot bagSlot, uint amount)
        {
            return AddToSaddleCall(Offsets.g_InventoryManager, (uint)bagSlot.BagId, bagSlot.Slot, amount) == IntPtr.Zero;
        }

        /// <summary>
        /// Attempts to move a specified quantity of items from the chocobo saddlebag to the player's inventory.
        /// </summary>
        /// <param name="bagSlot">The saddlebag slot containing the items.</param>
        /// <param name="amount">The quantity to move.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise <see langword="false"/>.</returns>
        public static bool RemoveFromSaddlebagQuantity(this BagSlot bagSlot, uint amount)
        {
            return RemoveFromSaddleCall(Offsets.g_InventoryManager, (uint)bagSlot.BagId, bagSlot.Slot, amount) == IntPtr.Zero;
        }

        /// <summary>
        /// Uses the item in this bag slot directly via the game's internal UseItem function.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to use.</param>
        public static void UseItemRaw(this BagSlot bagSlot)
        {
            BagSlotUseItemCall(Offsets.g_InventoryManager, bagSlot.TrueItemId, (uint)bagSlot.BagId, bagSlot.Slot);
        }

        internal static byte BagSlotUseItemCall(IntPtr InventoryManager, uint TrueItemId, uint inventoryContainer, int inventorySlot)
        {
            return Core.Memory.CallInjectedWraper<byte>(BagSlotExtensionsOffsets.BagSlotUseItem,
                                                        InventoryManager,
                                                        TrueItemId,
                                                        inventoryContainer,
                                                        inventorySlot);
        }

        internal static IntPtr RemoveFromSaddleCall(IntPtr InventoryManager, uint inventoryContainer, ushort inventorySlot, uint count)
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.RemoveFromSaddle,
                                                          InventoryManager,
                                                          inventoryContainer,
                                                          inventorySlot,
                                                          count);
        }

        internal static IntPtr AddToSaddleCall(IntPtr InventoryManager, uint inventoryContainer, ushort inventorySlot, uint count)
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.AddToSaddle,
                                                          InventoryManager,
                                                          inventoryContainer,
                                                          inventorySlot,
                                                          count);
        }

        internal static IntPtr PlaceAetherWheel(uint inventoryContainer, ushort inventorySlot)
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.PlaceAetherWheel,
                                                          AgentBagSlot.Instance.PointerForAether,
                                                          (int)inventorySlot,
                                                          inventoryContainer);
        }

        internal static IntPtr FcChestCall(uint sourceContainer, uint sourceSlot, uint destContainer, uint destSlot)
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(BagSlotExtensionsOffsets.FCChestMove,
                                                          AgentFreeCompanyChest.Instance.Pointer,
                                                          sourceContainer,
                                                          sourceSlot,
                                                          destContainer,
                                                          destSlot);
        }

        /// <summary>
        /// Moves items from a source bag slot to a destination bag slot within the Free Company chest.
        /// </summary>
        /// <param name="sourceBagSlot">The source bag slot.</param>
        /// <param name="destBagSlot">The destination bag slot.</param>
        public static void FcChestMove(this BagSlot sourceBagSlot, BagSlot destBagSlot)
        {
            FcChestCall((uint)sourceBagSlot.BagId, sourceBagSlot.Slot, (uint)destBagSlot.BagId, destBagSlot.Slot);
        }

        /// <summary>
        /// Returns a housing item from the storeroom to the player's inventory.
        /// </summary>
        /// <param name="bagSlot">The storeroom bag slot containing the item.</param>
        /// <returns><see langword="true"/> if the item was successfully moved; otherwise <see langword="false"/>.</returns>
        public static bool StoreroomReturnToInventory(this BagSlot bagSlot)
        {
            if (!bagSlot.IsFilled || InventoryManager.FreeSlots < 1)
            {
                return false;
            }

            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.StoreroomToInventory,
                                                 HousingHelper.PositionPointer,
                                                 bagSlot.Pointer);

            return true;
        }

        /// <summary>
        /// Moves a housing item from the player's inventory to the housing storeroom.
        /// </summary>
        /// <param name="bagSlot">The player bag slot containing the item.</param>
        /// <returns><see langword="true"/> if the item was successfully moved; otherwise <see langword="false"/>.</returns>
        public static bool InventoryToStoreroom(this BagSlot bagSlot)
        {
            if (!bagSlot.IsFilled)
            {
                return false;
            }

            Core.Memory.CallInjectedWraper<uint>(BagSlotExtensionsOffsets.InventoryToStoreroom,
                                                 HousingHelper.PositionPointer,
                                                 bagSlot.Pointer);

            return true;
        }

        /// <summary>
        /// Reads the stain (dye) identifier associated with the item in this bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot to check.</param>
        /// <returns>The stain ID as a <see cref="byte"/>.</returns>
        public static byte StainId(this BagSlot bagSlot)
        {
            return Core.Memory.Read<byte>(bagSlot.Pointer + BagSlotExtensionsOffsets.StainId);
        }

        /// <summary>
        /// Attempts to apply a dye to the specified item using another bag slot as the dye source.
        /// </summary>
        /// <param name="item">The item to be dyed.</param>
        /// <param name="dye">The bag slot containing the dye to use.</param>
        /// <returns>A status byte indicating the result of the dyeing operation.</returns>
        /// <remarks>
        /// This function was reported as broken in game version 7.5.
        /// </remarks>
        public static byte DyeItem(this BagSlot item, BagSlot dye)
        {
            //TODO Function broke in 7.5

            return 0;
            using var bagid = Core.Memory.CreateAllocatedMemory(8);
            using var bagslot = Core.Memory.CreateAllocatedMemory(4);
            bagid.AllocateOfChunk("bagid", 8);
            bagid.Write("bagid", 0x270F00000000 + (long)dye.BagId);
            bagslot.AllocateOfChunk("bagslot", 4);
            int test = -1 << 16;
            int test2 = test + dye.Slot;

            bagslot.Write("bagslot", test2);

            return Core.Memory.CallInjectedWraper<byte>(BagSlotExtensionsOffsets.DyeItem,
                                                        Offsets.g_InventoryManager,
                                                        item.BagId,
                                                        item.Slot,
                                                        bagid.Address,
                                                        bagslot.Address);
        }

        /// <summary>
        /// Entrusts a specified quantity (as <see cref="uint"/>) of items from the player's inventory to a retainer.
        /// </summary>
        /// <param name="bagSlot">The player bag slot containing items to entrust.</param>
        /// <param name="amount">The quantity of items to entrust.</param>
        public static void RetainerEntrustQuantity(this BagSlot bagSlot, uint amount)
        {
            bagSlot.RetainerEntrustQuantity((int)amount);
        }

        /// <summary>
        /// Retrieves a specified quantity (as <see cref="uint"/>) of items from a retainer's inventory.
        /// </summary>
        /// <param name="bagSlot">The retainer bag slot to retrieve items from.</param>
        /// <param name="amount">The quantity of items to retrieve.</param>
        public static void RetainerRetrieveQuantity(this BagSlot bagSlot, uint amount)
        {
            bagSlot.RetainerRetrieveQuantity((int)amount);
        }

        /// <summary>
        /// Gets the localized name of the item in this bag slot, appending "(HQ)" if the item is high quality.
        /// </summary>
        /// <param name="bagSlot">The bag slot to retrieve the name for.</param>
        /// <returns>The localized item name string.</returns>
        public static string ItemName(this BagSlot bagSlot)
        {
            return GetItemName(bagSlot.TrueItemId);
        }

        /// <summary>
        /// Gets the localized name for the specified item ID, appending "(HQ)" if the ID indicates a high-quality item.
        /// </summary>
        /// <param name="ItemId">The item ID to look up.</param>
        /// <returns>The localized item name string.</returns>
        public static string GetItemName(uint ItemId)
        {
            if (ItemId >= 1000000)
            {
                return $"{DataManager.GetItem(ItemId - 1000000).CurrentLocaleName} (HQ)";
            }

            return DataManager.GetItem(ItemId).CurrentLocaleName;
        }

        public const int DefaultBagSlotMoveWait = 600;

        /// <summary>
        /// Wait for a bag slot's contents to change (due to a move or stack operation) before proceeding.
        /// Adjusts the wait time based on current network ping.
        /// </summary>
        /// <param name="bagSlot">The bag slot to monitor.</param>
        /// <param name="curSlotCount">The initial item count in the slot.</param>
        /// <param name="waitMs">The maximum time to wait in milliseconds.</param>
        /// <returns><see langword="true"/> if the slot count changed or became invalid within the timeout; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Wait for a bag slot to become empty (unfilled) before proceeding.
        /// </summary>
        /// <param name="bagSlot">The bag slot to monitor.</param>
        /// <param name="waitMs">The maximum time to wait in milliseconds (doubled internally for safety).</param>
        /// <returns><see langword="true"/> if the slot became unfilled within the timeout; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Attempts to move a specified quantity of items to the saddlebag and waits for the operation to complete.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing items to move.</param>
        /// <param name="moveCount">The quantity to move.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> TryAddToSaddlebag(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.AddToSaddlebagQuantity(moveCount);
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        /// <summary>
        /// Attempts to retrieve a specified quantity of items from the saddlebag and waits for the operation to complete.
        /// </summary>
        /// <param name="bagSlot">The saddlebag slot containing items to retrieve.</param>
        /// <param name="moveCount">The quantity to retrieve.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> TryRemoveFromSaddlebag(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RemoveFromSaddlebagQuantity(moveCount);
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        /// <summary>
        /// Attempts to sell the item in this bag slot to a retainer and waits for the operation to complete.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing the item to sell.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the item was sold; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> TrySellItem(this BagSlot bagSlot, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RetainerSellItem();
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        /// <summary>
        /// Attempts to entrust a specified quantity of items to a retainer and waits for the operation to complete.
        /// If the direct entrust fails, it attempts to move the item to the first free retainer bag slot.
        /// </summary>
        /// <param name="bagSlot">The bag slot containing items to entrust.</param>
        /// <param name="moveCount">The quantity to entrust.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the items were successfully entrusted; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Attempts to retrieve a specified quantity of items from a retainer and waits for the operation to complete.
        /// </summary>
        /// <param name="bagSlot">The retainer bag slot containing items to retrieve.</param>
        /// <param name="moveCount">The quantity to retrieve.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the items were successfully retrieved; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> TryRetrieveFromRetainer(this BagSlot bagSlot, uint moveCount, int waitMs = DefaultBagSlotMoveWait)
        {
            var curSlotCount = bagSlot.Count;
            bagSlot.RetainerRetrieveQuantity(moveCount);
            return await BagSlotMoveWait(bagSlot, curSlotCount, waitMs);
        }

        /// <summary>
        /// Attempts to return a housing item from the storeroom to the player's inventory,
        /// handling the confirmation dialog if it appears, and waits for the operation to complete.
        /// </summary>
        /// <param name="bagSlot">The storeroom bag slot containing the item.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the item was successfully returned; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Attempts to move a housing item from the player's inventory to the storeroom and waits for the operation to complete.
        /// </summary>
        /// <param name="bagSlot">The player bag slot containing the item.</param>
        /// <param name="waitMs">Maximum wait time in milliseconds.</param>
        /// <returns><see langword="true"/> if the item was successfully moved; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> TryInventoryToStoreroom(this BagSlot bagSlot, int waitMs = DefaultBagSlotMoveWait)
        {
            if (bagSlot.InventoryToStoreroom())
            {
                return await Coroutine.Wait(5000, () => !bagSlot.IsFilled);
            }

            return false;
        }

        /// <summary>
        /// Attempts to dye an item using a specified dye and waits for the process (including the desynthesis/dye lock) to complete.
        /// </summary>
        /// <param name="item">The equipment item to dye.</param>
        /// <param name="dye">The bag slot containing the dye.</param>
        /// <returns><see langword="true"/> if the dyeing process completed successfully; otherwise <see langword="false"/>.</returns>
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
