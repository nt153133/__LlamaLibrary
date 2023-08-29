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

#pragma warning disable 649
namespace LlamaLibrary.RetainerItemFinder
{
    public static class ItemFinder
    {
        private static readonly LLogger Log = new(nameof(ItemFinder), Colors.Pink);

        public static IntPtr Pointer;

        internal static IntPtr TreeStart => Core.Memory.ReadArray<IntPtr>(ParentStart, 3)[1];

        internal static IntPtr ParentStart => Core.Memory.Read<IntPtr>(Pointer + Offsets.TreeStartOff);

        private static readonly List<IntPtr> VisitedNodes = new();

        private static readonly Dictionary<ulong, StoredRetainerInventory> RetainerInventoryPointers = new();

        private static bool firstTimeSaddleRead = true;

        public static IntPtr Framework;

        private static bool _hasGoneToDresser = false;

        static ItemFinder()
        {
            Framework = Core.Memory.Read<IntPtr>(Offsets.GFramework2);
            var getUiModule = Core.Memory.CallInjected64<IntPtr>(Offsets.GetUiModule, Framework);
            var getRaptureItemFinder = getUiModule + Offsets.RaptureItemFinder;

            Pointer = getRaptureItemFinder;
        }

        public static async Task<Dictionary<ulong, StoredRetainerInventory>> SafelyGetCachedRetainerInventories()
        {
            var retData = await HelperFunctions.GetOrderedRetainerArray(true);

            if (retData.Length == 0)
            {
                Log.Error($"You don't have any retainers");
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
            var ids = Core.Memory.ReadArray<uint>(Pointer + Offsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + Offsets.SaddleBagItemQtys, 140);

            return new StoredSaddleBagInventory(ids, qtys);
        }

        public static async Task<StoredSaddleBagInventory> SafelyGetCachedSaddlebagInventoryComplete()
        {
            var ids = Core.Memory.ReadArray<uint>(Pointer + Offsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + Offsets.SaddleBagItemQtys, 140);

            if (firstTimeSaddleRead && ids.All(i => i == 0))
            {
                if (await FlashSaddlebags())
                {
                    ids = Core.Memory.ReadArray<uint>(Pointer + Offsets.SaddleBagItemIds, 140);
                    qtys = Core.Memory.ReadArray<ushort>(Pointer + Offsets.SaddleBagItemQtys, 140);
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

            var ids = Core.Memory.ReadArray<uint>(Pointer + Offsets.SaddleBagItemIds, 140);
            var qtys = Core.Memory.ReadArray<ushort>(Pointer + Offsets.SaddleBagItemQtys, 140);

            if (firstTimeSaddleRead && ids.All(i => i == 0))
            {
                if (await InventoryBuddy.Instance.Open())
                {
                    await Coroutine.Sleep(200);
                    InventoryBuddy.Instance.Close();
                    await Coroutine.Wait(2000, () => !InventoryBuddy.Instance.IsOpen);
                    await Coroutine.Sleep(300);
                    ids = Core.Memory.ReadArray<uint>(Pointer + Offsets.SaddleBagItemIds, 140);
                    qtys = Core.Memory.ReadArray<ushort>(Pointer + Offsets.SaddleBagItemQtys, 140);
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
            var ids = Core.Memory.ReadArray<int>(Pointer + Offsets.GlamourDresserItemIds, 800);
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

        public static bool IsGlamourDresserCached => Core.Memory.Read<byte>(Pointer + Offsets.GlamourDresserCached) == 1;

        public static bool ShouldVisitGlamourDresser()
        {
            var ids = Core.Memory.ReadArray<int>(Pointer + Offsets.GlamourDresserItemIds, 800);
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
            else
            {
                Log.Verbose($"Adding node");
                RetainerInventoryPointers.Add(node.RetainerId, new StoredRetainerInventory(node.RetainerInventory));
            }

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

        private static class Offsets
        {
            [Offset("Search 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B C8 41 FF 90 ?? ?? ?? ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ?? 5B 49 FF 60 ?? 48 83 C4 ?? 5B C3 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 40 53 Add 3 TraceRelative")]
            internal static IntPtr GFramework2;

            [Offset("Search E8 ? ? ? ? 48 8B D8 48 85 C0 74 ? 66 85 FF TraceCall")]
            [OffsetCN("Search E8 ?? ?? ?? ?? 48 8B C8 48 85 C0 74 ?? 84 DB 74 ?? 38 1D ?? ?? ?? ?? TraceCall")]
            internal static IntPtr GetUiModule;

            //Broken pattern but it should be 0x88
            [Offset("Search 48 FF A0 ? ? ? ? 48 8B 02 48 8B CA 48 83 C4 ? 5B 48 FF A0 ? ? ? ? Add 3 Read32")]
            internal static int GetRaptureItemFinder;

            [Offset("Search 49 8D 8E ? ? ? ? 33 D2 FF 50 ? 41 80 BE ? ? ? ? ? Add 3 Read32")]
            internal static int RaptureItemFinder;

            [Offset("Search 4C 8B 85 ? ? ? ? 48 89 B4 24 ? ? ? ? Add 3 Read32")]
            internal static int TreeStartOff;

            [Offset("Search 48 8D 83 ? ? ? ? 48 89 74 24 ? 48 8D 8B ? ? ? ? Add 3 Read32")]
            internal static int SaddleBagItemIds;

            [Offset("Search 48 8D 8B ? ? ? ? 48 89 7C 24 ? 4C 89 64 24 ? Add 3 Read32")]
            internal static int SaddleBagItemQtys;

            [Offset("Search 4D 8D 85 ? ? ? ? 41 B9 ? ? ? ? 0F 1F 80 ? ? ? ? 41 8B 08 85 C9 74 ? B8 ? ? ? ? F7 E1 C1 EA ? 69 C2 ? ? ? ? 2B C8 8B C6 FF C6 41 89 0C 87 49 83 C0 ? 49 83 E9 ? 75 ? 0F B6 4C 24 ? Add 3 Read32")]
            internal static int GlamourDresserItemIds;

            [Offset("Search 80 B9 ? ? ? ? ? 48 8B D9 74 ? 48 83 C4 ? Add 2 Read32")]
            internal static int GlamourDresserCached;
        }
    }
}