---
title: "Navigation and Travel"
description: "API reference for the navigation, world-travel, teleport, and housing-travel helpers that move characters across zones and worlds."
---

Import paths:

- `using LlamaLibrary.Helpers;`
- `using LlamaLibrary.Helpers.WorldTravel;`
- `using LlamaLibrary.Helpers.HousingTravel;`

Key source files: `Helpers/Navigation.cs`, `Helpers/WorldTravel/WorldTravel.cs`, `Helpers/TeleportHelper.cs`, and `Helpers/HousingTravel/HousingTraveler.cs`.

## `Navigation`

Source: `Helpers/Navigation.cs`

Representative public surface:

```csharp
public static class Navigation
{
    public static Task<bool> GetTo(Location location);
    public static Task<bool> GetToWithLisbeth(uint zoneId, double x, double y, double z);
    public static Task<bool> GetToWithLisbeth(uint zoneId, Vector3 xyz);
    public static Task<bool> GetTo(uint zoneId, double x, double y, double z);
    public static Task<bool> GetTo(World world, Location location);
    public static Task<bool> GetTo(WorldLocation worldLocation);
    public static Task<bool> GetTo(World world, uint zoneId, Vector3 xyz);
    public static Task<bool> GetTo(uint zoneId, Vector3 xyz);
    public static Task<bool> GetTo(uint zoneId, Vector3 xyz, float distance);
}
```

The helper lazily installs `SlideMover` and `ServiceNavigationProvider`, delegates to `HousingTraveler` for residential zones, and can fall back to teleport-based travel when a direct path is unavailable.

## `WorldTravel`

Source: `Helpers/WorldTravel/WorldTravel.cs`

```csharp
public static class WorldTravel
{
    public static Task OpenWorldTravelMenu(TravelCity travelCity = TravelCity.Cheapest);
    public static Task<bool> GetTo(WorldLocation worldLocation, TravelCity travelCity = TravelCity.Uldah);
    public static Task<bool> GoToWorld(World world, TravelCity travelCity = TravelCity.Cheapest);
    public static Task<bool> GoToWorld(ushort worldId, TravelCity travelCity = TravelCity.Cheapest);
}
```

`GoToWorld` validates data-center compatibility, leaves a normal party if required, opens the world-travel menu, selects the target world, confirms the dialog, and waits for `WorldTravelFinderReady`.

## `TeleportHelper`

Source: `Helpers/TeleportHelper.cs`

Representative public surface:

```csharp
public static class TeleportHelper
{
    public static TeleportInfo[] TeleportList { get; }
    public static void UpdateTeleportArray();
    public static void CallUpdate();
    public static Task<bool> TeleportToApartment();
    public static Task<bool> TeleportToPrivateEstate();
    public static Task<bool> TeleportToFreeCompanyEstate();
    public static Task<bool> TeleportToSharedEstate(int estateIndex);
    public static Task<bool> TeleportToSharedEstate(ushort zone, int ward, int plot);
    public static Task<bool> TeleportByIndex(uint index);
}
```

## `HousingTraveler`

Source: `Helpers/HousingTravel/HousingTraveler.cs`

Representative public surface:

```csharp
public static class HousingTraveler
{
    public static ResidentialDistrict? CurrentResidentialDistrict { get; }
    public static HousingZone TranslateZone(HousingZone zone);
    public static Task<bool> EnterHouse(HouseLocation location);
    public static Task<bool> EnterHouse(RecordedPlot recordedPlot);
    public static RecordedPlot? GetRecordedPlot(HousingZone zone, int plot);
    public static Task<bool> GetToResidential(HouseLocation location);
    public static Task<bool> GetToResidential(Npc npc);
    public static Task<bool> GetToResidential(World world, Npc npc);
    public static Task<bool> GetToResidential(Location location, int ward);
    public static Task<bool> GetToResidential(ushort zoneId, double x, double y, double z, int ward);
    public static Task<bool> GetToResidential(World world, Location location, int ward);
}
```

Example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.JsonObjects;

public async Task<bool> TravelAsync()
{
    var target = new HouseLocation
    {
        HousingZone = HousingZone.Empyreum,
        Plot = 30,
        Ward = 3,
        World = World.Gilgamesh
    };

    return await HousingTraveler.GetToResidential(target);
}
```
