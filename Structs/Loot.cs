using System;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Structs;

public enum LootMode : uint
{
    NeedAndGreed = 1,
    GreedAll = 2,
    PassAll = 3
}

public enum RollOption : uint
{
    UnAwarded = 0,
    Need = 1,
    Greed = 2,
    Pass = 5,
    Awarded = 6,
    NotAvailable = 7
}

public enum RollState : uint
{
    UpToNeed,
    UpToGreed,
    UpToPass,
    Rolled = 17,
    NoLoot = 26
}
#if RB_TC
[StructLayout(LayoutKind.Explicit, Size = 0x40)]
#else
[StructLayout(LayoutKind.Explicit, Size = 0x44)]
#endif
public struct LootItem
{
    [FieldOffset(0x0)]
    public uint ObjectId;

    [FieldOffset(0x8)]
    public uint ItemId;

    [FieldOffset(0x20)]
    public RollState RollState;

    [FieldOffset(0x24)]
    public RollOption RolledState;

    [FieldOffset(0x2C)]
    public float LeftRollTime;

    [FieldOffset(0x20)]
    public float TotalRollTime;

    [FieldOffset(0x3C)]
    public uint Index;

    public readonly bool Rolled => RolledState > 0;

    public readonly bool Valid => ObjectId != GameObjectManager.EmptyGameObject && ObjectId != 0;

    public readonly Item? Item => DataManager.GetItem(ItemId);

    public bool CanRollNeedOrGreed()
    {
        if (Rolled)
        {
            return false;
        }

        var itemId = ItemId;

        if (Item == null)
        {
            return false;
        }

        if (Item.Unique && InventoryManager.FilledSlots.Any(i => i.RawItemId == itemId))
        {
            return false;
        }

        if (Item.Unique && Item.Untradeable && Item.ItemAction != 0 && !UIState.CanUnlockItem(itemId))
        {
            return false;
        }

        return true;
    }

    public bool Need()
    {
        return Roll(RollOption.Need);
    }

    public bool Greed()
    {
        return Roll(RollOption.Greed);
    }

    public bool Pass()
    {
        return Roll(RollOption.Pass);
    }

    private bool Roll(RollOption option)
    {
        bool result;
        var thisLootItem = this;
        var findIndex = Array.FindIndex(LootHelper.RawLootItems, item => item.Equals(thisLootItem));

        result = Core.Memory.CallInjectedWraper<byte>(Offsets.LootFunc, Offsets.LootsAddr, (ulong)option, findIndex) == 1;

        if (!result)
        {
            throw new Exception($"Failed rolling {option} for {Item?.EnglishName}. LootState: {RollState} Remaining time: {LeftRollTime:F2}");
        }

        return result;
    }

    public override readonly string? ToString()
    {
        return Item?.EnglishName;
    }
}
