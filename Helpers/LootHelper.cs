using System.Collections.Generic;
using System.Linq;
using ff14bot;
using LlamaLibrary.Memory;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers;

public static class LootHelper
{
    public static bool HasLoot => AvailableLoots().Count != 0;
    public static bool HasLootRequiringAction => LootRequiringAction().Count != 0;
    public static LootItem[] RawLootItems => Core.Memory.ReadArray<LootItem>(Offsets.LootsAddr + 0x10, 16);
    public static List<LootItem> LootRequiringAction() => RawLootItems.Where(li => li is { Valid: true, RolledState: < RollOption.Pass, Rolled: false, }).ToList();
    public static List<LootItem> AvailableLoots() => RawLootItems.Where(li => li.Valid).ToList();
}