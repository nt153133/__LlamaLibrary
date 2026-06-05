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
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers;

public static class UIState
{
    private static readonly LLogger Log = new(nameof(UIState), Colors.Pink);

    

    /// <summary>Gets the memory address of the game's UIState instance.</summary>
    public static IntPtr Instance => UIStateOffsets.Instance;

    /// <summary>Gets the pointer to the game's internal function that checks if an item action is unlocked.</summary>
    public static IntPtr IsItemActionUnlockedPtr => UIStateOffsets.IsItemActionUnlocked;

    /// <summary>Gets the pointer to the game's internal function for retrieving item EXD data.</summary>
    public static IntPtr ExdGetItemPtr => UIStateOffsets.ExdGetItem;

    /// <summary>Gets the byte offset within an item structure where the item action identifier is stored.</summary>
    public static int ItemActionOffset => UIStateOffsets.ItemActionOffset;

    /// <summary>Determines if a Triple Triad card is unlocked for the current player.</summary>
    /// <param name="id">The numeric identifier of the card.</param>
    /// <returns><see langword="true"/> if the card is unlocked; otherwise <see langword="false"/>.</returns>
    public static bool CardUnlocked(int id) => Core.Memory.CallInjectedWraper<bool>(UIStateOffsets.CardUnlocked, UIStateOffsets.Instance, id);

    /// <summary>Determines if an emote is unlocked for the current player.</summary>
    /// <param name="id">The numeric identifier of the emote.</param>
    /// <returns><see langword="true"/> if the emote is unlocked; otherwise <see langword="false"/>.</returns>
    public static bool EmoteUnlocked(int id) => Core.Memory.CallInjectedWraper<bool>(UIStateOffsets.EmoteUnlocked, UIStateOffsets.Instance, id);

    /// <summary>Gets the raw bit-array from memory representing the player's unlocked minions.</summary>
    public static byte[] MinionArray => Core.Memory.ReadBytes(UIStateOffsets.MinionArray, 0x50);

    /// <summary>Determines if a minion is unlocked for the current player by checking the bit-field in <see cref="MinionArray"/>.</summary>
    /// <param name="id">The numeric identifier of the minion.</param>
    /// <returns><see langword="true"/> if the minion is unlocked; otherwise <see langword="false"/>.</returns>
    public static bool MinionUnlocked(int id) => ((1 << (id & 7)) & MinionArray[id >> 3]) > 0;

    /// <summary>Retrieves the memory pointer to the EXD data record for the specified item ID.</summary>
    /// <param name="id">The numeric identifier of the item.</param>
    /// <returns>A pointer to the item's EXD record in game memory.</returns>
    public static IntPtr GetItemExdData(uint id) => Core.Memory.CallInjectedWraper<IntPtr>(UIStateOffsets.ExdGetItem, id);

    /// <summary>
    /// Determines if the specific item action associated with an item (e.g., teaching a mount or minion)
    /// has already been unlocked or consumed by the player.
    /// </summary>
    /// <param name="id">The raw numeric ID of the item to check.</param>
    /// <returns><see langword="true"/> if the item's action is unlocked; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// If the item's EXD record cannot be resolved directly, a temporary structure is allocated
    /// in memory to perform the check.
    /// </remarks>
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
                newStruct.Write(UIStateOffsets.ItemActionOffset, item.ItemAction);
                return Core.Memory.CallInjectedWraper<int>(UIStateOffsets.IsItemActionUnlocked, UIStateOffsets.Instance, newStruct.Address) == 1;
            }
        }

        if (itemPtr == IntPtr.Zero)
        {
            throw new Exception("Item not found in exd");
        }

        return Core.Memory.CallInjectedWraper<int>(UIStateOffsets.IsItemActionUnlocked, UIStateOffsets.Instance, itemPtr) == 1;
    }

    /// <summary>
    /// Determines if an item can currently be "unlocked" (used to acquire a mount, minion, or emote).
    /// Returns <see langword="true"/> if the item has an unlock action and the player hasn't used it yet.
    /// </summary>
    /// <param name="itemId">The numeric identifier of the item.</param>
    /// <returns><see langword="true"/> if the item is unlockable and currently not unlocked; otherwise <see langword="false"/>.</returns>
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
    /// Checks if the player has already acquired or currently possesses a specific item.
    /// </summary>
    /// <param name="itemId">The raw numeric ID of the item (ignores HQ/NQ modifiers).</param>
    /// <param name="forceGlamour">When <see langword="true"/>, forces navigation to a glamour dresser if its cache is empty.</param>
    /// <returns>
    /// <see langword="true"/> if the item's action is unlocked (for consumables like mounts) OR
    /// if the physical item exists in inventory, armory, retainer storage, saddlebags, or the glamour dresser.
    /// </returns>
    /// <remarks>
    /// Retainer inventory checks are only performed when the player is on their home world.
    /// Glamour dresser check is skipped if the glamour dresser quest (68554) is not completed.
    /// </remarks>
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

    /// <summary>
    /// Synchronously checks if the player has already acquired or currently possesses a specific item.
    /// </summary>
    /// <param name="itemId">The raw numeric ID of the item.</param>
    /// <returns>
    /// <see langword="true"/> if the item is acquired or possessed across all supported storage systems
    /// (inventory, armory, retainer, saddlebags, dresser).
    /// </returns>
    /// <remarks>
    /// This method uses the existing memory cache for retainers and saddlebags and does not trigger
    /// any asynchronous UI operations.
    /// </remarks>
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