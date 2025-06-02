using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RetainerItemFinder;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers;

public static class UIState
{
    private static readonly LLogger Log = new(nameof(UIState), Colors.Pink);

    private static class Offsets
    {
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? 48 8B 01 Add 3 TraceRelative")]
        internal static IntPtr Instance;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 66 85 D2 74 ?")]
        internal static IntPtr CardUnlocked;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 48 8B F9 0F B7 CA E8 ? ? ? ? 48 85 C0")]
        internal static IntPtr EmoteUnlocked;

        [Offset("Search 48 8D 0D ? ? ? ? 0F B6 04 08 84 D0 75 ? B8 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr MinionArray;

        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 66 83 B8 ? ? ? ? ? 0F 84 ? ? ? ? TraceCall")]
        internal static IntPtr ExdGetItem;

        [Offset("Search E8 ?? ?? ?? ?? 83 F8 01 75 03 TraceCall")]
        internal static IntPtr IsItemActionUnlocked;

        [Offset("Search 0F B7 8A ? ? ? ? E8 ? ? ? ? 48 8B F8 Add 3 Read32")]
        internal static int ItemActionOffset;
    }

    public static IntPtr Instance => Offsets.Instance;

    public static IntPtr IsItemActionUnlockedPtr => Offsets.IsItemActionUnlocked;

    public static IntPtr ExdGetItemPtr => Offsets.ExdGetItem;

    public static int ItemActionOffset => Offsets.ItemActionOffset;

    public static bool CardUnlocked(int id) => Core.Memory.CallInjectedWraper<bool>(Offsets.CardUnlocked, Offsets.Instance, id);

    public static bool EmoteUnlocked(int id) => Core.Memory.CallInjectedWraper<bool>(Offsets.EmoteUnlocked, Offsets.Instance, id);

    public static byte[] MinionArray => Core.Memory.ReadBytes(Offsets.MinionArray, 0x50);

    public static bool MinionUnlocked(int id) => ((1 << (id & 7)) & MinionArray[id >> 3]) > 0;

    public static IntPtr GetItemExdData(uint id) => Core.Memory.CallInjectedWraper<IntPtr>(Offsets.ExdGetItem, id);

    public static bool IsItemActionUnlocked(uint id)
    {
        var itemPtr = GetItemExdData(id);

        if (itemPtr == IntPtr.Zero)
        {
            Log.Information("Item not found in exd, trying again");
            Core.Memory.ClearCache();
            using (Core.Memory.AcquireFrame())
            {
                itemPtr = GetItemExdData(id);
            }
        }

        if (itemPtr == IntPtr.Zero)
        {
            Log.Information("Can't find item so making our own struct");
            var item = DataManager.GetItem(id);

            if (item == null)
            {
                Log.Error($"Item {id} not found in exd");
                return false;
            }

            using (var newStruct = Core.Memory.CreateAllocatedMemory(0x98))
            {
                newStruct.Write(Offsets.ItemActionOffset, item.ItemAction);
                return Core.Memory.CallInjectedWraper<int>(Offsets.IsItemActionUnlocked, Offsets.Instance, newStruct.Address) == 1;
            }
        }

        if (itemPtr == IntPtr.Zero)
        {
            throw new Exception("Item not found in exd");
        }

        return Core.Memory.CallInjectedWraper<int>(Offsets.IsItemActionUnlocked, Offsets.Instance, itemPtr) == 1;
    }

    public static bool CanUnlockItem(uint itemId)
    {
        var item = DataManager.GetItem(itemId);

        if (item == null)
        {
            Log.Error($"Item {itemId} not found in exd");
            return false;
        }

        if (item.ItemAction == 0)
        {
            return false;
        }

        return !IsItemActionUnlocked(itemId);
    }

    /// <summary>
    /// Checks to see if you have an item unlocked like a mount/hairstyle/emote/minion OR if you have the item in your inventory/retainer/saddlebags/glamour dresser. Ignores HQ/NQ. Force glamour dresser check if you want to make it open the glamour dresser if it's not loaded.
    /// </summary>
    /// <param name="itemId">Item Id (use raw id not true id).</param>
    /// <param name="forceGlamour">Force going to a glamour dresser if no items are cached for it</param>
    /// <returns>True if you have already acquired or used the item. False otherwise.</returns>
    public static async Task<bool> HasItem(uint itemId, bool forceGlamour = false)
    {
        var item = DataManager.GetItem(itemId);

        if (item == null)
        {
            Log.Error($"Item {itemId} not found in data manager");
            return false;
        }

        if (item.ItemAction != 0)
        {
            return IsItemActionUnlocked(itemId);
        }

        var inventoryItem = InventoryManager.FilledInventoryAndArmory.Any(i => i.RawItemId == itemId);
        var retainerItem = WorldHelper.IsOnHomeWorld && (await ItemFinder.SafelyGetCachedRetainerInventories()).Any(i => i.Value.Inventory.Any(j => j.Key % 1000000 == itemId));

        var saddlebagsItem = (await ItemFinder.SafelyGetCachedSaddlebagInventoryComplete()).Inventory.Any(i => i.Key % 1000000 == itemId);

        if (!QuestLogManager.IsQuestCompleted(68554))
        {
            return inventoryItem || retainerItem || saddlebagsItem;
        }

        var glamourDresserItem = forceGlamour ? (await ItemFinder.GetGlamourDressedUpdated()).Any(i => i % 1000000 == itemId) : ItemFinder.GetCachedGlamourDresserInventory().Any(i => i % 1000000 == itemId);

        //Log.Information($"Item {itemId} found in inventory: {inventoryItem}, retainer: {retainerItem}, saddlebags: {saddlebagsItem}, glamour dresser: {glamourDresserItem}");
        return inventoryItem || retainerItem || saddlebagsItem || glamourDresserItem;
    }

    public static bool HasItemSync(uint itemId)
    {
        var item = DataManager.GetItem(itemId);

        if (item == null)
        {
            Log.Error($"Item {itemId} not found in data manager");
            return false;
        }

        if (item.ItemAction != 0)
        {
            return IsItemActionUnlocked(itemId);
        }

        var inventoryItem = InventoryManager.FilledInventoryAndArmory.Any(i => i.RawItemId == itemId);
        var retainerItem = WorldHelper.IsOnHomeWorld && ItemFinder.GetCachedRetainerInventories().Any(i => i.Value.Inventory.Any(j => j.Key % 1000000 == itemId));

        var saddlebagsItem = ItemFinder.GetCachedSaddlebagInventoryComplete().Inventory.Any(i => i.Key % 1000000 == itemId);

        if (!QuestLogManager.IsQuestCompleted(68554))
        {
            return inventoryItem || retainerItem || saddlebagsItem;
        }

        var glamourDresserItem = ItemFinder.GetCachedGlamourDresserInventory().Any(i => i % 1000000 == itemId);

        //Log.Information($"Item {itemId} found in inventory: {inventoryItem}, retainer: {retainerItem}, saddlebags: {saddlebagsItem}, glamour dresser: {glamourDresserItem}");
        return inventoryItem || retainerItem || saddlebagsItem || glamourDresserItem;
    }
}