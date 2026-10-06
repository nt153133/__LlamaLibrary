---
title: "Travel to a House"
description: "Use HousingTraveler, WorldTravel, and TeleportHelper to move to a residential district and enter a specific plot."
---

Housing travel is one of the richer orchestration flows in the library because it combines several subsystems: home-world checks, world travel, housing-zone normalization, teleport shortcuts, and district-specific movement. The main entry point is `Helpers/HousingTravel/HousingTraveler.cs`.

## Problem

Residential travel is more complicated than generic navigation. The target might be on another world, the zone enum might represent an apartment or chamber variant rather than the top-level district, and the fastest route may be a personal, shared, or free-company teleport rather than walking from a city entrance.

## Solution

Use `HousingTraveler.GetToResidential(...)` or `HousingTraveler.EnterHouse(...)` instead of trying to stitch world travel and navigation together yourself. The helper already normalizes housing zones, checks direct residential teleports, and falls back to district-specific navigation when needed.

<Steps>
<Step>
### Build a `HouseLocation`

```csharp
using LlamaLibrary.Enums;
using LlamaLibrary.JsonObjects;

var target = new HouseLocation
{
    HousingZone = HousingZone.Mist,
    Plot = 5,
    Ward = 1,
    World = World.Gilgamesh
};
```

</Step>
<Step>
### Travel to the district

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers.HousingTravel;

public async Task<bool> TravelAsync(HouseLocation target)
{
    return await HousingTraveler.GetToResidential(target);
}
```

</Step>
<Step>
### Enter the house if needed

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers.HousingTravel;

public async Task<bool> EnterAsync(HouseLocation target)
{
    return await HousingTraveler.EnterHouse(target);
}
```

</Step>
</Steps>

Complete example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.JsonObjects;

public async Task<bool> GoToMistHouseAsync()
{
    var target = new HouseLocation
    {
        HousingZone = HousingZone.Mist,
        Plot = 5,
        Ward = 1,
        World = World.Gilgamesh
    };

    if (!await HousingTraveler.GetToResidential(target))
    {
        return false;
    }

    return await HousingTraveler.EnterHouse(target);
}
```

What the helper actually does:

- `TranslateZone(...)` collapses apartment, chamber, cottage, house, and mansion variants to the base district.
- If the target world is the home world, the helper tries direct estate teleports first through `TeleportHelper`.
- If direct teleport is not available, it uses `WorldTravel.WorldTravel.GoToWorld(...)` and then district navigation.
- District implementations such as `Mist`, `LavenderBeds`, and `Empyreum` live under `Helpers/HousingTravel/Districts/` and provide specialized route data.

This is a good example of why LlamaLibrary favors composition. `HousingTraveler` does not reimplement teleport or world-transfer logic; it coordinates existing helpers and keeps the residential rules in one place.
