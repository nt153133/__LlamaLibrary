---
title: "Commerce and Utility Helpers"
description: "API reference for helper modules that orchestrate Grand Company shopping, busy-state cleanup, and related gameplay automation."
---

Import path: `using LlamaLibrary.Helpers;`

The helper layer is very large, but several modules are especially important because they demonstrate how the rest of the library is meant to be consumed. This page focuses on `GeneralFunctions` and `GrandCompanyShop`, which combine many of the lower-level APIs documented elsewhere.

## `GeneralFunctions`

Source: `Helpers/GeneralFunctions.cs`

Representative public surface:

```csharp
public static class GeneralFunctions
{
    public static readonly InventoryBagId[] MainBags;
    public static readonly InventoryBagId[] SaddlebagIds;
    public static IEnumerable<BagSlot> MainBagsFilledSlots();
    public static ClassJobType QuestClass(int questId);
    public static bool IsJumping { get; }
    public static Task StopBusy(bool leaveDuty = true, bool stopFishing = true, bool dismount = true);
    public static Task SmallTalk(int waitTime = 500);
    public static Task InventoryEquipBest(bool updateGearSet = true, bool useRecommendEquip = true);
    public static Task<bool> UpdateGearSet();
    public static IEnumerable<BagSlot> NonGearSetItems();
    public static Task RetainerSellItems(IEnumerable<BagSlot> items);
    public static Task<bool> ExitRetainer(bool exitList = false);
    public static Task RepairAll();
    public static Task GoHome();
    public static Task OpenChests();
    public static IEnumerable<Treasure> GetTreasureChests();
    public static bool IsDutyComplete(uint dutyId);
    public static bool IsDutyUnlocked(uint dutyId);
    public static Task PassOnAllLoot();
    public static Task VoteMVPTask();
    public static bool DalamudDetected();
    public static string? SourceFileName();
    public static DirectoryInfo? SourceDirectory();
}
```

The most widely reusable method is `StopBusy(...)`, which attempts to unwind fishing, duty state, mounts, conversations, crafting windows, gathering windows, and target selection before a new workflow starts.

## `GrandCompanyShop`

Source: `Helpers/GrandCompanyShop.cs`

```csharp
public static class GrandCompanyShop
{
    public static IntPtr ActiveShopPtr { get; }
    public static IntPtr ListStart { get; }
    public static List<GCShopItem> Items { get; }
    public static int CanAfford(GCShopItem item);
    public static Task<int> BuyItem(uint itemId, int qty);
    public static Task<bool> BuyKnownItems(List<(uint ItemId, int qty)> items);
    public static Task<int> BuyKnownItem(uint itemId, int qty);
    public static Task<int> BuyItem(uint itemId, int qty, int gcRankGroup, GCShopCategory category);
    public static Task<bool> OpenShop();
    public static Task<bool> CloseShop();
    public static bool IsBuyableItem(uint itemId);
}
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `itemId` | `uint` | — | The game item ID to buy. |
| `qty` | `int` | — | Requested quantity. |
| `gcRankGroup` | `int` | — | Required rank group when using the explicit overload. |
| `category` | `GCShopCategory` | — | Required item category when using the explicit overload. |

Example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers;

public async Task<bool> PrepareAndBuyAsync()
{
    await GeneralFunctions.StopBusy();
    return await GrandCompanyShop.BuyKnownItem(21072, 20) > 0;
}
```

These helpers are representative of the library's style: they are task-oriented, they return plain `Task` or `Task<T>` values instead of hiding coroutines inside a custom framework, and they orchestrate a mixture of managers, windows, agents, and data resources.
