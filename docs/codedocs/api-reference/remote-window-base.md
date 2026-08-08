---
title: "RemoteWindow Base and Window Families"
description: "API reference for the RemoteWindow base classes and the major concrete window wrapper families exposed by LlamaLibrary."
---

Most concrete window wrappers inherit from the same base and differ only in window name plus a handful of actions. This page documents the shared base surface and then inventories the main exported wrapper families. Import path: `using LlamaLibrary.RemoteWindows;`

## `RemoteWindow<T>` and `RemoteWindow`

Source: `RemoteWindows/RemoteWindow.cs`

```csharp
public class RemoteWindow<T> : RemoteWindow
    where T : RemoteWindow<T>, new()
{
    public static T Instance { get; }
}

public abstract class RemoteWindow
{
    public virtual bool IsOpen { get; }
    public virtual string WindowName { get; }
    public virtual AgentInterface Agent { get; }
    public virtual AtkAddonControl? WindowByName { get; }

    public virtual void Close();
    public int GetAgentInterfaceId();
    public Task<bool> WaitTillWindowOpen(int maxTimeOut = 5000);
    public void SendAction(int pairCount, params ulong[] param);
    public virtual Task<bool> Open();
    public void SendAction(bool updateState = true, params AtkValue[] parms);
}
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `maxTimeOut` | `int` | `5000` | Milliseconds to wait in `WaitTillWindowOpen`. |
| `updateState` | `bool` | `true` | Passed to the `AtkClientFunctions.SendActionPtr` overload. |

## Representative specialized wrappers

### `GrandCompanyExchange`

Source: `RemoteWindows/GrandCompanyExchange.cs`

```csharp
public class GrandCompanyExchange : RemoteWindow<GrandCompanyExchange>
{
    public int GetNumberOfItems { get; }
    public int GCRankGroup { get; }
    public uint[] GetTurninItemsIds();
    public uint[] GetItemCosts();
    public void BuyItemByIndex(uint index, int qty);
    public void ChangeRankGroup(int rankGroup);
    public void ChangeItemGroup(int itemGroup);
}
```

### `FishGuide2`

Source: `RemoteWindows/FishGuide2.cs`

```csharp
public class FishGuide2 : RemoteWindow<FishGuide2>
{
    public void ClickTab(int index);
    public void SelectFishing();
    public void SelectSpearFishing();
}
```

### `RetainerList`

Source: `RemoteWindows/RetainerList.cs`

```csharp
public class RetainerList : RemoteWindow<RetainerList>
{
    public RetainerInfo[] OrderedRetainerList { get; }
    public int NumberOfRetainers { get; }
    public int NumberOfVentures { get; }
    public string RetainerName(int index);
    public int GetRetainerJobLevel(int index);
    public bool RetainerHasJob(int index);
    public RetainerRole RetainerRole(int index);
    public Task<bool> SelectRetainer(ulong retainerContentId);
    public Task<bool> SelectRetainer(int index);
}
```

### `Conversation`

Source: `RemoteWindows/Conversation.cs`

```csharp
public static class Conversation
{
    public static bool IsOpen { get; }
    public static List<string> GetConversationList { get; }
    public static string StripHypen(this string line);
    public static void SelectLine(uint line);
    public static bool SelectLineContains(string line);
    public static void SelectQuit();
}
```

## Exported window families

All of the following files export public window wrappers built on the same base:

- Aetheryte and reward windows: `AWGrowthFragTrade`, `AetherialWheel`, `GoldSaucerReward`, `TripleTriadCoinExchange`
- Grand Company windows: `GrandCompanyExchange`, `GrandCompanyRankUp`, `GrandCompanySupplyList`, `GrandCompanySupplyReward`
- Housing and estate windows: `HousingGoods`, `HousingMenu`, `HousingSelectBlock`, `HousingSignBoard`
- Gathering and island-related windows: `Gathering`, `FishGuide`, `FishGuide2`, `SpearFishing`, `MJIGatheringNoteBook`, `MJIHud`, `MJIPouch`, `MJIRecipeNoteBook`, `WKSPouch`, `WKSRecipeNotebook`
- Retainer windows: `RetainerHistory`, `RetainerList`, `RetainerTaskList`
- Shop windows: `ShopCardDialog`, `ShopExchangeItem`, `ShopExchangeItemDialog`, `ShopProxy`, `InclusionShop`, `FreeShop`

Most of the remaining files under `RemoteWindows/` are similarly structured and inherit the base members documented above.

Example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.RemoteWindows;

public async Task<bool> OpenRetainerListAsync()
{
    if (!await RetainerList.Instance.Open())
    {
        return false;
    }

    return RetainerList.Instance.NumberOfRetainers >= 0;
}
```
