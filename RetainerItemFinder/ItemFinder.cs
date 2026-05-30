using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.LocationTracking;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Retainers;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

#pragma warning disable 649
namespace LlamaLibrary.RetainerItemFinder
{
    /// <summary>
    /// Provides access to the game's internal item finder system, allowing retrieval of cached inventory data for retainers, saddlebags, and the glamour dresser.
    /// </summary>
    public static class ItemFinder
    {
        private static readonly LLogger Log = new(nameof(ItemFinder), Colors.Pink);

        /// <summary>
        /// The base pointer for the RaptureItemFinder module.
        /// </summary>
        public static IntPtr Pointer;

        /// <summary>
        /// Gets the root pointer of the internal binary tree used for storing retainer inventory data.
        /// </summary>
        internal static IntPtr TreeStart => Core.Memory.ReadArray<IntPtr>(ParentStart, 3)[1];

        /// <summary>
        /// Gets the pointer to the parent structure of the inventory tree.
        /// </summary>
        internal static IntPtr ParentStart => Core.Memory.Read<IntPtr>(Pointer + ItemFinderOffsets.TreeStartOff);

        private static readonly List<IntPtr> VisitedNodes = new();

        private static readonly Dictionary<ulong, StoredRetainerInventory> RetainerInventoryPointers = new();

        private static bool firstTimeSaddleRead = true;

        /// <summary>
        /// Pointer to the game's Framework instance.
        /// </summary>
        public static IntPtr Framework;

        private static bool _hasGoneToDresser;

        /// <summary>
        /// Initializes the <see cref="ItemFinder"/> static class by resolving the RaptureItemFinder pointer from the Framework.
        /// </summary>
        static ItemFinder()
        {
            Framework = Core.Memory.Read<IntPtr>(ItemFinderOffsets.GFramework2);
            var getUiModule = Core.Memory.CallInjectedWraper<IntPtr>(ItemFinderOffsets.GetUiModule, Framework);
            var getRaptureItemFinder = getUiModule + ItemFinderOffsets.RaptureItemFinder;

            Pointer = getRaptureItemFinder;
        }

        /// <summary>
        /// Safely retrieves cached inventory data for all retainers.
        /// Checks if the player has any retainers before attempting to traverse the inventory tree.
        /// </summary>
        /// <returns>A dictionary mapping retainer Content IDs to their <see cref="StoredRetainerInventory"/>.</returns>
        public static async Task<Dictionary<ulong, StoredRetainerInventory>> SafelyGetCachedRetainerInventories()
        {
            var retData = await HelperFunctions.GetOrderedRetainerArray(true);

            if (retData.Length == 0)
            {
                Log.Error("You don't have any retainers");
                return new Dictionary<ulong, StoredRetainerInventory>();
            }

            return GetCachedRetainerInventories();
        }

        /// <summary>
        /// Traverses the internal inventory tree and returns cached data for all retainers.
        /// </summary>
        /// <returns>A dictionary mapping retainer Content IDs to their <see cref="StoredRetainerInventory"/>.</returns>
        public static Dictionary<ulong, StoredRetainerInventory> GetCachedRetainerInventories()
        {
            VisitedNodes.Clear();
            RetainerInventoryPointers.Clear();

            VisitedNodes.Add(ParentStart);

            //Log.Information($"ParentStart {ParentStart.ToString("X")}");

            Visit(TreeStart);

            return RetainerInventoryPointers;
        }

        /// <summary>
        /// Retrieves the complete cached inventory data for the player's Chocobo Saddlebag.
        /// </summary>
        /// <returns>A <see cref="StoredSaddleBagInventory"/> containing the saddlebag items.</returns>
        public static StoredSaddleBagInventory GetCachedSaddlebagInventoryComplete()
        {
            var ids = Core.Memory.ReadArray<uint>(Pointer + ItemFinderOffsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + ItemFinderOffsets.SaddleBagItemQtys, 140);

            return new StoredSaddleBagInventory(ids, qtys);
        }

        /// <summary>
        /// Safely retrieves the complete cached saddlebag inventory, optionally triggering a UI flash to populate the cache if it's empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> resulting in a <see cref="StoredSaddleBagInventory"/>.</returns>
        public static async Task<StoredSaddleBagInventory> SafelyGetCachedSaddlebagInventoryComplete()
        {
            var ids = Core.Memory.ReadArray<uint>(Pointer + ItemFinderOffsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + ItemFinderOffsets.SaddleBagItemQtys, 140);

            if (firstTimeSaddleRead && ids.All(i => i == 0))
            {
                if (await FlashSaddlebags())
                {
                    ids = Core.Memory.ReadArray<uint>(Pointer + ItemFinderOffsets.SaddleBagItemIds, 140);
                    qtys = Core.Memory.ReadArray<ushort>(Pointer + ItemFinderOffsets.SaddleBagItemQtys, 140);
                }

                firstTimeSaddleRead = false;
            }

            return new StoredSaddleBagInventory(ids, qtys);
        }

        /// <summary>
        /// Briefly opens and closes the Chocobo Saddlebag window to force the game to populate the local item cache.
        /// </summary>
        /// <returns><see langword="true"/> if the window was opened successfully; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> FlashSaddlebags()
        {
            var couldOpen = await InventoryBuddy.Instance.Open();
            if (couldOpen)
            {
                await Coroutine.Sleep(200);
                InventoryBuddy.Instance.Close();
                await Coroutine.Wait(2000, () => !InventoryBuddy.Instance.IsOpen);
                await Coroutine.Sleep(300);
            }

            return couldOpen;
        }

        /// <summary>
        /// Retrieves a simplified dictionary of item IDs and quantities from the cached saddlebag inventory.
        /// Optionally triggers a UI flash to populate the cache if it's empty.
        /// </summary>
        /// <returns>A dictionary mapping item IDs to total quantities.</returns>
        public static async Task<Dictionary<uint, int>> GetCachedSaddlebagInventories()
        {
            var result = new Dictionary<uint, int>();

            var ids = Core.Memory.ReadArray<uint>(Pointer + ItemFinderOffsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + ItemFinderOffsets.SaddleBagItemQtys, 140);

            if (firstTimeSaddleRead && ids.All(i => i == 0))
            {
                if (await InventoryBuddy.Instance.Open())
                {
                    await Coroutine.Sleep(200);
                    InventoryBuddy.Instance.Close();
                    await Coroutine.Wait(2000, () => !InventoryBuddy.Instance.IsOpen);
                    await Coroutine.Sleep(300);
                    ids = Core.Memory.ReadArray<uint>(Pointer + ItemFinderOffsets.SaddleBagItemIds, 140);
                    qtys = Core.Memory.ReadArray<ushort>(Pointer + ItemFinderOffsets.SaddleBagItemQtys, 140);
                }

                firstTimeSaddleRead = false;
            }

            for (var i = 0; i < 140; i++)
            {
                if (ids[i] == 0)
                {
                    continue;
                }

                if (result.ContainsKey(ids[i]))
                {
                    result[ids[i]] += qtys[i];
                }
                else
                {
                    result.Add(ids[i], qtys[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the cached item IDs present in the player's glamour dresser.
        /// </summary>
        /// <returns>An array of 800 item IDs from the glamour dresser cache.</returns>
        public static int[] GetCachedGlamourDresserInventory()
        {
            var ids = Core.Memory.ReadArray<int>(Pointer + ItemFinderOffsets.GlamourDresserItemIds, 800);
            return ids;
        }

        /// <summary>
        /// Gets the updated glamour dresser inventory, visiting the dresser if necessary and if it hasn't been visited in this session.
        /// </summary>
        /// <returns>A <see cref="Task"/> resulting in an array of item IDs.</returns>
        public static async Task<int[]> GetGlamourDressedUpdated()
        {
            if (_hasGoneToDresser)
            {
                return GetCachedGlamourDresserInventory();
            }

            if (!ShouldVisitGlamourDresser())
            {
                return GetCachedGlamourDresserInventory();
            }

            await UpdateGlamourDresser();

            return GetCachedGlamourDresserInventory();
        }

        /// <summary>
        /// Navigates to the nearest glamour dresser and interacts with it to update the internal item cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UpdateGlamourDresser()
        {
            if (IsGlamourDresserCached)
            {
                _hasGoneToDresser = true;
                return;
            }

            if (!_hasGoneToDresser)
            {
                var tracker = new LocationTracker();
                if (await NavigationHelper.GoToGlamourDresser())
                {
                    RaptureAtkUnitManager.GetWindowByName("MiragePrismPrismBox").SendAction(1, 3uL, 0xFFFFFFFFuL);
                    await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("MiragePrismPrismBox") == null);
                    _hasGoneToDresser = true;
                }

                await tracker.GoBack();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the glamour dresser inventory is currently cached in memory.
        /// </summary>
        public static bool IsGlamourDresserCached => Core.Memory.Read<byte>(Pointer + ItemFinderOffsets.GlamourDresserCached) == 1;

        /// <summary>
        /// Determines if the glamour dresser should be visited based on whether the current cache is entirely empty.
        /// </summary>
        /// <returns><see langword="true"/> if all cached IDs are zero; otherwise <see langword="false"/>.</returns>
        public static bool ShouldVisitGlamourDresser()
        {
            var ids = Core.Memory.ReadArray<int>(Pointer + ItemFinderOffsets.GlamourDresserItemIds, 800);
            return ids.All(i => i == 0);
        }

        /// <summary>
        /// Recursively traverses the binary tree of retainer inventories, collecting data into <see cref="RetainerInventoryPointers"/>.
        /// </summary>
        /// <param name="nodePtr">The pointer to the current tree node to visit.</param>
        private static void Visit(IntPtr nodePtr)
        {
            if (VisitedNodes.Contains(nodePtr))
            {
                return;
            }

            var node = Core.Memory.Read<ItemFinderPtrNode>(nodePtr);

            if (!node.Filled)
            {
                return;
            }

            Log.Verbose("Adding node");
            RetainerInventoryPointers.Add(node.RetainerId, new StoredRetainerInventory(node.RetainerInventory));

            if (!VisitedNodes.Contains(node.Left))
            {
                Visit(node.Left);
            }

            if (!VisitedNodes.Contains(node.Right))
            {
                Visit(node.Right);
            }

            VisitedNodes.Add(nodePtr);
        }

        
    }
}
