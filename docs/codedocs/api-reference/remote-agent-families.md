---
title: "RemoteAgent Families"
description: "API reference for the exported IAgent contract and representative RemoteAgent wrappers that expose client-side arrays, vtables, and actions."
---

Import path: `using LlamaLibrary.RemoteAgents;`

The exported agent surface is broad, but the design is consistent: each concrete type inherits `AgentInterface<T>`, implements `IAgent`, exposes a `RegisteredVtable`, and sometimes adds properties or methods for pointer-backed state. This page documents the interface and the most behavior-rich agent wrappers.

## `IAgent`

Source: `RemoteAgents/IAgent.cs`

```csharp
public interface IAgent
{
    IntPtr RegisteredVtable { get; }
}
```

## Representative agents

### `AgentGrandCompanyExchange`

Source: `RemoteAgents/AgentGrandCompanyExchange.cs`

```csharp
public class AgentGrandCompanyExchange : AgentInterface<AgentGrandCompanyExchange>, IAgent
{
    public IntPtr RegisteredVtable { get; }
    public byte Category { get; }
    public byte Rank { get; }
    public void BuyItem(uint index, int qty);
}
```

### `AgentFishGuide2`

Source: `RemoteAgents/AgentFishGuide2.cs`

```csharp
public class AgentFishGuide2 : AgentInterface<AgentFishGuide2>, IAgent
{
    public const int TabCount = 37;
    public IntPtr RegisteredVtable { get; }
    public IntPtr InfoPointer { get; }
    public IntPtr StartingPointer { get; }
    public IntPtr EndingPointer { get; }
    public IntPtr FishListPointer(int start = 0);
    public int SlotCount { get; }
    public FishGuide2Item[] GetFishListRaw();
    public FishGuide2Item[] GetFishListRaw(int start, int count);
    public Task<FishGuide2Item[]> GetFishList(int start = 0, int count = 0);
}
```

`FishGuide2Item` is also exported from the same file:

```csharp
public struct FishGuide2Item
{
    public uint FishItem;
    public ushort Index;
    public bool HasCaught { get; }
}
```

### `AgentFreeCompany`

Source: `RemoteAgents/AgentFreeCompany.cs`

```csharp
public class AgentFreeCompany : AgentInterface<AgentFreeCompany>, IAgent
{
    public IntPtr RegisteredVtable { get; }
    public byte HistoryLineCount { get; }
    public IntPtr ActionAddress { get; }
    public IntPtr GetRosterPtr();
    public List<(string Name, bool Online)> GetMembers();
    public Task<FcAction[]> GetCurrentActions();
    public Task<FcAction[]> GetAvailableActions();
}
```

### `AgentFreeCompanyChest`

Source: `RemoteAgents/AgentFreeCompanyChest.cs`

```csharp
public class AgentFreeCompanyChest : AgentInterface<AgentFreeCompanyChest>, IAgent
{
    public IntPtr RegisteredVtable { get; }
    public byte SelectedTabIndex { get; }
    public bool CrystalsTabSelected { get; }
    public bool GilTabSelected { get; }
    public byte GilWithdrawDeposit { get; }
    public uint GilAmountTransfer { get; set; }
    public uint GilBalance { get; }
    public bool FullyLoaded { get; }
    public byte LoadBagCall(InventoryBagId bagId);
    public Task LoadBag(InventoryBagId bagId);
}
```

### `AgentAWGrowthFragTrade`

Source: `RemoteAgents/AgentAWGrowthFragTrade.cs`

```csharp
public class AgentAWGrowthFragTrade : AgentInterface<AgentAWGrowthFragTrade>, IAgent
{
    public IntPtr RegisteredVtable { get; }
    public IntPtr ArrayPtr { get; }
    public int ArrayCount { get; }
    public AnimaExchangeItemInfo[] ExchangeItems { get; }
    public void Buy(int index, int qty);
    public static Task<bool> BuyCrystalSand(uint itemToSpend, int qty, bool buyAnyAmount = false);
}
```

## Exported agent inventory

The repository also exports many narrower wrappers such as `AgentAchievement`, `AgentAetherWheel`, `AgentDawn`, `AgentDawnStory`, `AgentGrandCompanySupply`, `AgentHandIn`, `AgentHousingSignboard`, `AgentItemAppraisal`, `AgentRetainerCharacter`, `AgentRetainerInventory`, `AgentRetainerList`, `AgentRetainerVenture`, `AgentTradeMultiple`, `AgentVoteMVP`, and `AgentWorldTravelSelect`.

Example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.RemoteAgents;

public async Task<bool> BuyCrystalSandAsync()
{
    return await AgentAWGrowthFragTrade.BuyCrystalSand(13589, 10, buyAnyAmount: true);
}
```
