using System.Collections.Generic;
using System.Linq;
using ff14bot;
using LlamaLibrary.Memory;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Provides helpers for inspecting the current loot table (up to 16 slots) read directly from game memory.
/// </summary>
public static class LootHelper
{
    /// <summary><c>true</c> when at least one valid loot item is available in the current loot window.</summary>
    public static bool HasLoot => AvailableLoots().Count != 0;

    /// <summary><c>true</c> when at least one valid loot item has not yet been passed or rolled on.</summary>
    public static bool HasLootRequiringAction => LootRequiringAction().Count != 0;

    /// <summary>Reads all 16 raw loot slots from game memory, including invalid/empty entries.</summary>
    public static LootItem[] RawLootItems => Core.Memory.ReadArray<LootItem>(Offsets.LootsAddr + 0x10, 16);

    /// <summary>
    /// Returns the subset of <see cref="RawLootItems"/> that are valid, have not yet been rolled on,
    /// and whose <see cref="LootItem.RolledState"/> has not yet reached <see cref="RollOption.Pass"/>.
    /// </summary>
    /// <returns>Loot items still awaiting a roll decision.</returns>
    public static List<LootItem> LootRequiringAction() => RawLootItems.Where(li => li is { Valid: true, RolledState: < RollOption.Pass, Rolled: false, }).ToList();

    /// <summary>
    /// Returns all valid (non-empty) loot items from <see cref="RawLootItems"/>.
    /// </summary>
    /// <returns>All currently available loot items.</returns>
    public static List<LootItem> AvailableLoots() => RawLootItems.Where(li => li.Valid).ToList();
}