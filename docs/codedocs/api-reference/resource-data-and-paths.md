---
title: "Resource Data and Paths"
description: "API reference for ResourceManager, JsonHelper path helpers, and the settings/data conventions LlamaLibrary uses for embedded resources."
---

LlamaLibrary ships a large amount of embedded reference data: housing plot metadata, Grand Company shop mappings, ventures, materia lists, hunt data, and custom delivery data. This page documents the public entry points that surface that data and the path conventions used alongside it.

## `ResourceManager`

Import path: `using LlamaLibrary;`

Source: `ResourceManager.cs`

```csharp
public static class ResourceManager
{
    public static readonly Lazy<Dictionary<int, List<MateriaItem>>> MateriaList;
    public static readonly Lazy<SortedDictionary<int, StoredHuntLocation>> DailyHunts;
    public static readonly Lazy<List<RetainerTaskData>> VentureData;
    public static readonly Lazy<List<CustomDeliveryNpc>> CustomDeliveryNpcs;
    public static readonly Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>> HousingPlots;
    public static readonly Dictionary<GrandCompany, List<GCShopItemStored>> GCShopItems;
    public static readonly Dictionary<uint, List<StoredRecipe>> Recipes_Anden;

    public static T LoadResource<T>(string text);
}
```

The static constructor loads JSON from embedded `Properties.Resources` values such as `Ventures`, `Materia`, `AllHunts`, `GCShopItems`, and the district-specific housing plot files.

Example:

```csharp
using LlamaLibrary;
using LlamaLibrary.Enums;

public int GetMistPlotCount()
{
    return ResourceManager.HousingPlots[HousingZone.Mist].Value.Count;
}
```

## `JsonHelper`

Import path: `using LlamaLibrary.Helpers;`

Source: `Helpers/JsonHelper.cs`

```csharp
public static class JsonHelper
{
    public static string UniqueCharacterSettingsDirectory { get; }
    public static string HomeWorldSettingsDirectory { get; }
    public static string DataCenterSettingsDirectory { get; }
}
```

These properties standardize folder naming for data that should live near the rest of the library's settings files.

## Legacy `JsonSettings<T>`

Import path: `using LlamaLibrary.JsonObjects;`

Source: `JsonObjects/JsonSettings.cs`

```csharp
public class JsonSettings<T> : JsonSettings, INotifyPropertyChanged
    where T : JsonSettings<T>, new()
{
    public static T Instance { get; }
}
```

The newer `BaseSettings<T>` system is usually the better choice for fresh code, but `JsonSettings<T>` still appears in the source and is part of the public surface.

## Practical notes

- `ResourceManager.GCShopItems` is consumed by `GrandCompanyShop` to translate item IDs into category and rank metadata.
- `ResourceManager.HousingPlots` is consumed by `HousingTraveler` to translate a `HouseLocation` into a `RecordedPlot` with placard and entrance coordinates.
- `JsonHelper` is useful when a project wants to store additional files in a directory structure that matches the rest of LlamaLibrary's conventions instead of inventing a parallel layout.
