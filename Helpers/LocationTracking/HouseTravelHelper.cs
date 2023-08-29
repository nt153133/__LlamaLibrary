using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.LocationTracking;

public static class HouseTravelHelper
{
    private static readonly LLogger Log = new LLogger("HouseTravelHelper", Colors.IndianRed);
    private const uint HouseEntranceId = 2002737;
    private const uint AptEntranceId = 2007402;

    public static async Task<bool> GoBackToHouse(HouseLocation? previousHouseLocation)
    {
        if (previousHouseLocation == null)
        {
            return false;
        }

        var location = CurrentHouseLocation;
        var skip = false;
        if (location != null)
        {
            if (HousingHelper.IsInsideHouse && location.Equals(previousHouseLocation))
            {
                return true;
            }

            if (HousingHelper.IsWithinPlot)
            {
                skip = location.Equals(previousHouseLocation);
            }
        }

        if (!skip && !await HousingTraveler.GetToResidential(previousHouseLocation))
        {
            return false;
        }

        if (!HousingHelper.IsWithinPlot)
        {
            Log.Error("Not within plot");
            MovementManager.MoveForwardStart();
            await Coroutine.Wait(5000, () => HousingHelper.IsWithinPlot);
            MovementManager.MoveStop();
        }

        MovementManager.MoveForwardStart();
        await Coroutine.Sleep(1000);
        MovementManager.MoveStop();

        //Log.Information("Getting closest housing entrance");

        var entranceIds = new[] { HouseEntranceId, AptEntranceId };

        var locations = GameObjectManager.GetObjectsByNPCIds<GameObject>(entranceIds).OrderBy(x => x.DistanceSqr()).ToList();

        var entrance = locations.FirstOrDefault();

        if (entrance == null)
        {
            return false;
        }

        Log.Information("Found housing entrance, approaching");
        await Navigation.GroundMove(entrance.Location, 2f);

        if (!entrance.IsWithinInteractRange)
        {
            return false;
        }

        Navigator.NavigationProvider.ClearStuckInfo();
        Navigator.Stop();
        await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);

        entrance.Interact();

        switch (entrance.NpcId)
        {
            // Handle different housing entrance menus
            case HouseEntranceId:
            {
                Log.Information("Entering house");

                if (await Coroutine.Wait(10000, () => SelectYesno.IsOpen))
                {
                    SelectYesno.Yes();
                }

                break;
            }
            case AptEntranceId:
            {
                Log.Information("Entering apartment");

                if (await Coroutine.Wait(10000, () => SelectString.IsOpen))
                {
                    SelectString.ClickSlot(0);
                }

                break;
            }
        }

        if (await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading))
        {
            await CommonTasks.HandleLoading();
        }

        return HousingHelper.IsInsideHouse && CurrentHouseLocation.Equals(previousHouseLocation);
    }

    internal static async Task<bool> GoBackToPlot(HouseLocation? previousHouseLocation)
    {
        var location = CurrentHouseLocation;

        if (previousHouseLocation == null)
        {
            Log.Error("Previous house location is null");
            return false;
        }

        if (location != null)
        {
            if (HousingHelper.IsWithinPlot)
            {
                return true;
            }
        }

        if (!await HousingTraveler.GetToResidential(previousHouseLocation))
        {
            return false;
        }

        var recorded = HousingTraveler.GetRecordedPlot(previousHouseLocation.HousingZone, previousHouseLocation.Plot);

        await Navigation.GroundMove(recorded.EntranceLocation, 1f);

        if (!HousingHelper.IsWithinPlot)
        {
            Log.Error("Not within plot");
            MovementManager.MoveForwardStart();
            await Coroutine.Wait(5000, () => HousingHelper.IsWithinPlot);
            MovementManager.MoveStop();
        }

        return HousingHelper.IsWithinPlot && CurrentHouseLocation.Equals(previousHouseLocation);
    }

    internal static async Task<bool> GoBackToHousingLocation(HousingAreaLocation? previousHouseLocation)
    {
        if (previousHouseLocation == null)
        {
            return false;
        }

        if (HousingHelper.IsInHousingArea && CurrentHousingAreaLocation.Equals(previousHouseLocation))
        {
            return true;
        }

        if (!await WorldTravel.WorldTravel.GoToWorld(previousHouseLocation.World))
        {
            Log.Error("Failed to get to world");
            return false;
        }

        if (!await HousingTraveler.GetToResidential(previousHouseLocation.HousingZone, previousHouseLocation.Location, previousHouseLocation.Ward))
        {
            Log.Error("Failed to get to housing area");
            return false;
        }

        await Navigation.GroundMove(previousHouseLocation.Location, 1f);

        var current = CurrentHousingAreaLocation;
        return HousingHelper.IsInHousingArea && current != null && current.Ward == previousHouseLocation.Ward && current.HousingZone == previousHouseLocation.HousingZone && current.World == previousHouseLocation.World && current.Location.DistanceSqr(previousHouseLocation.Location) < 10;
    }

    internal static HouseLocation? CurrentHouseLocation
    {
        get
        {
            if (!HousingHelper.IsInsideHouse && !HousingHelper.IsWithinPlot)
            {
                return null;
            }

            var info = HousingHelper.HousingPositionInfo;
            if (!info)
            {
                return null;
            }

            return new HouseLocation(HousingTraveler.TranslateZone((HousingZone)WorldManager.ZoneId), info.Ward, info.Plot);
        }
    }

    internal static HousingAreaLocation? CurrentHousingAreaLocation
    {
        get
        {
            if (!HousingHelper.IsInHousingArea)
            {
                return null;
            }

            return new HousingAreaLocation()
            {
                HousingZone = WorldManager.ZoneId,
                Ward = HousingHelper.HousingPositionInfo.Ward,
                World = WorldHelper.CurrentWorld,
                Location = Core.Me.Location
            };
        }
    }
}

public record HousingAreaLocation
{
    public ushort HousingZone { get; set; }
    public int Ward { get; set; }
    public World World { get; set; }
    public Vector3 Location { get; set; }
}