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
    public static class ItemFinder
    {
        private static readonly LLogger Log = new(nameof(ItemFinder), Colors.Pink);

        public static IntPtr Pointer;

        internal static IntPtr TreeStart => Core.Memory.ReadArray<IntPtr>(ParentStart, 3)[1];

        internal static IntPtr ParentStart => Core.Memory.Read<IntPtr>(Pointer + ItemFinderOffsets.TreeStartOff);

        private static readonly List<IntPtr> VisitedNodes = new();

        private static readonly Dictionary<ulong, StoredRetainerInventory> RetainerInventoryPointers = new();

        private static bool firstTimeSaddleRead = true;

        public static IntPtr Framework;

        private static bool _hasGoneToDresser;

        static ItemFinder()
        {
            Framework = Core.Memory.Read<IntPtr>(ItemFinderOffsets.GFramework2);
            var getUiModule = Core.Memory.CallInjectedWraper<IntPtr>(ItemFinderOffsets.GetUiModule, Framework);
            var getRaptureItemFinder = getUiModule + ItemFinderOffsets.RaptureItemFinder;

            Pointer = getRaptureItemFinder;
        }

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

        public static Dictionary<ulong, StoredRetainerInventory> GetCachedRetainerInventories()
        {
            VisitedNodes.Clear();
            RetainerInventoryPointers.Clear();

            VisitedNodes.Add(ParentStart);

            //Log.Information($"ParentStart {ParentStart.ToString("X")}");

            Visit(TreeStart);

            return RetainerInventoryPointers;
        }

        public static StoredSaddleBagInventory GetCachedSaddlebagInventoryComplete()
        {
            var ids = Core.Memory.ReadArray<uint>(Pointer + ItemFinderOffsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + ItemFinderOffsets.SaddleBagItemQtys, 140);

            return new StoredSaddleBagInventory(ids, qtys);
        }

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

        public static int[] GetCachedGlamourDresserInventory()
        {
            var ids = Core.Memory.ReadArray<int>(Pointer + ItemFinderOffsets.GlamourDresserItemIds, 800);
            return ids;
        }

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

        public static bool IsGlamourDresserCached => Core.Memory.Read<byte>(Pointer + ItemFinderOffsets.GlamourDresserCached) == 1;

        public static bool ShouldVisitGlamourDresser()
        {
            var ids = Core.Memory.ReadArray<int>(Pointer + ItemFinderOffsets.GlamourDresserItemIds, 800);
            return ids.All(i => i == 0);
        }

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