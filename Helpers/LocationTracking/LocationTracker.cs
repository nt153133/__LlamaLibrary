using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Helpers.WorldTravel;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.LocationTracking;

/// <summary>
/// Captures the player's current location at construction time and provides a
/// <see cref="GoBack"/> method to return to that location after an automation task moves the player elsewhere.
/// </summary>
/// <remarks>
/// The tracker recognises four location types: open world, housing area (outdoor ward),
/// housing plot (outdoor within a specific plot), and house interior.  On <see cref="GoBack"/> it
/// restores the player to the correct world, ward, and position for each type.
/// </remarks>
/// <example>
/// <code>
/// var tracker = new LocationTracker();
/// // … do automation work that moves the player …
/// await tracker.GoBack();
/// </code>
/// </example>
public class LocationTracker
{
    private static readonly LLogger Log = new LLogger("LocationTracker", Colors.IndianRed);
    private readonly HousingAreaLocation? _previousHousingAreaLocation;
    private readonly HouseLocation? _previousHouseLocation;
    private readonly WorldLocation? _previousLocation;
    private readonly LocationType _previousLocationType;
    private float _previousHeading;

    /// <summary>
    /// Initialises a new <see cref="LocationTracker"/> by snapshotting the player's current world,
    /// zone, coordinates, heading, and housing state.
    /// </summary>
    public LocationTracker()
    {
        Log.Verbose("LocationTracker initialized.");
        _previousLocation = new WorldLocation(WorldHelper.CurrentWorld, new Location(WorldManager.ZoneId, Core.Me.Location));
        _previousHeading = Core.Me.Heading;
        if (!HousingHelper.IsInHousingArea)
        {
            _previousLocationType = LocationType.World;
            return;
        }

        if (HousingHelper.IsWithinPlot && !HousingHelper.IsInsideHouse)
        {
            _previousLocationType = LocationType.HousingPlot;
            _previousHouseLocation = HouseTravelHelper.CurrentHouseLocation;
        }
        else if (HousingHelper.IsInsideHouse)
        {
            _previousLocationType = LocationType.House;
            _previousHouseLocation = HouseTravelHelper.CurrentHouseLocation;
        }
        else
        {
            _previousLocationType = LocationType.HousingArea;
            _previousHousingAreaLocation = HouseTravelHelper.CurrentHousingAreaLocation;
        }
    }

    /// <summary>
    /// Returns the player to the location captured at construction time.
    /// </summary>
    /// <remarks>
    /// Handles world travel if the player has changed worlds.  Delegates to
    /// <see cref="HouseTravelHelper"/> for housing-specific navigation.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> when the player has successfully returned;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> GoBack()
    {
        if (_previousLocation == null)
        {
            return false;
        }

        if (WorldHelper.CurrentWorld != _previousLocation.World)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(_previousLocation.World))
            {
                Log.Error("Failed to go back to previous world");
                return false;
            }
        }

        if (_previousLocationType == LocationType.World)
        {
            if (GrandCompanyHelper.IsInBarracks && _previousLocation.Location.ZoneId is 130 or 132 or 128)
            {
                uint[] npcIds = { 2007528, 2006963, 2007530 };
                var exitNpc = GameObjectManager.GetObjectsByNPCIds<GameObject>(npcIds).FirstOrDefault();
                if (exitNpc != null)
                {
                    if (await NavigationHelper.InteractWithNpc(exitNpc))
                    {
                        if (await Coroutine.Wait(10000, () => SelectYesno.IsOpen))
                        {
                            SelectYesno.Yes();
                            await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading);
                            Log.Information("Waiting for loading to finish...");
                            await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                        }
                    }
                }
                else
                {
                    Log.Error("Couldn't find the exit");
                    //return;
                }
            }

            if (!await Navigation.GetTo(_previousLocation)) //|| !await Navigation.GroundMove(_previousLocation.Location.Coordinates, 1f))
            {
                Log.Error("Failed to go back to previous location");
                return false;
            }

            MovementManager.SetFacing(_previousHeading);
            return true;
        }

        switch (_previousLocationType)
        {
            case LocationType.HousingArea:
                Log.Information("Going back to housing area");
                if (!await HouseTravelHelper.GoBackToHousingLocation(_previousHousingAreaLocation))
                {
                    Log.Error($"Failed to get back to housing area {_previousLocation}");
                    return false;
                }

                break;
            case LocationType.House:
                Log.Information("Going back to house");
                if (!await HouseTravelHelper.GoBackToHouse(_previousHouseLocation))
                {
                    Log.Error("Failed to get back to house");
                    return false;
                }

                break;
            case LocationType.HousingPlot:
                Log.Information("Going back to plot");
                if (!await HouseTravelHelper.GoBackToPlot(_previousHouseLocation))
                {
                    Log.Error($"Failed to get back to plot {_previousLocation}");
                    return false;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Log.Information("Going back to previous location");
        if (!await Navigation.GetTo(_previousLocation))
        //if (!await Navigation.GroundMove(_previousLocation.Location.Coordinates, 1f))
        {
            Log.Error("Failed to go back to previous location");
            return false;
        }

        MovementManager.SetFacing(_previousHeading);
        return true;
    }
}

/// <summary>Classifies the type of location the player occupied when <see cref="LocationTracker"/> was constructed.</summary>
public enum LocationType
{
    /// <summary>An ordinary non-housing open-world zone.</summary>
    World,
    /// <summary>An outdoor housing ward (not on a specific plot).</summary>
    HousingArea,
    /// <summary>Inside a house or FC workshop interior.</summary>
    House,
    /// <summary>Standing within a plot's boundary but not inside the house interior.</summary>
    HousingPlot,
}