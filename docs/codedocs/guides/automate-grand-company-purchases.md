---
title: "Automate Grand Company Purchases"
description: "Open the Grand Company exchange, select the correct category and rank group, and buy items using the helper and window APIs."
---

Grand Company shopping is a good example of how LlamaLibrary layers its abstractions. The high-level entry point is `Helpers/GrandCompanyShop.cs`, but that helper works only because it composes `GrandCompanyHelper`, the `GrandCompanyExchange` window wrapper, and `AgentGrandCompanyExchange`.

## Problem

Buying items manually requires several steps that are easy to get wrong in automation code: reach the right NPC, open the exchange window, switch rank groups, switch categories, confirm the purchase dialog, and wait for bag counts to change. The library already encodes all of those steps, including the known item metadata from `ResourceManager.GCShopItems`.

## Solution

Use `GrandCompanyShop.BuyKnownItem(...)` when you already know the item ID and want the helper to look up the required rank group and category for you. Use `BuyItem(...)` if you already know the group yourself and want more direct control.

<Steps>
<Step>
### Get into a clean state

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers;

public async Task PrepareAsync()
{
    await GeneralFunctions.StopBusy();
}
```

</Step>
<Step>
### Open the shop or let the helper do it

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers;

public async Task<bool> EnsureShopOpenAsync()
{
    return await GrandCompanyShop.OpenShop();
}
```

</Step>
<Step>
### Buy a known item

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers;

public async Task<int> BuyVenturesAsync()
{
    const uint ventureItemId = 21072;
    return await GrandCompanyShop.BuyKnownItem(ventureItemId, 20);
}
```

</Step>
</Steps>

Complete example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers;

public async Task<bool> BuyGcSuppliesAsync()
{
    await GeneralFunctions.StopBusy();

    var boughtVentures = await GrandCompanyShop.BuyKnownItem(21072, 20);
    var boughtCordials = await GrandCompanyShop.BuyKnownItem(12669, 5);

    return boughtVentures > 0 || boughtCordials > 0;
}
```

Internally, `GrandCompanyShop.OpenShop()` ensures a navigation provider exists, calls `GrandCompanyHelper.InteractWithNpc(GCNpc.Quartermaster)`, and waits for `GrandCompanyExchange.Instance.IsOpen`. `BuyKnownItem(...)` then uses `ResourceManager.GCShopItems` to locate the correct rank group and category before delegating to `BuyItem(...)`. That final method uses `AgentGrandCompanyExchange.Instance.BuyItem(...)`, waits for `SelectYesno`, confirms the purchase, and verifies the bag count changed.

This design is useful beyond shopping. It shows the general LlamaLibrary pattern:

- high-level helper for orchestration,
- window wrapper for visible state and selection,
- agent wrapper for the actual client call,
- and resource data for IDs and metadata.

If you need to script more specialized flows, inspect `Helpers/GrandCompanyShop.cs` and `RemoteWindows/GrandCompanyExchange.cs` together; they are intentionally complementary.
