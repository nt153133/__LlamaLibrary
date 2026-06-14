using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers.LocationTracking;

/// <summary>
/// Static helper that provides high-level navigation methods for entering and returning to housing
/// locations — including houses, housing plots, workshops, and general housing area positions.
/// </summary>
/// <remarks>
/// Methods in this class coordinate with <see cref="HousingTraveler"/> for ward selection and
/// with <see cref="HousingHelper"/> for state detection.
/// </remarks>
public static class HouseTravelHelper
{
    private static readonly LLogger Log = new LLogger("HouseTravelHelper", Colors.IndianRed);
    private const uint HouseEntranceId = 2002737;
    private const uint AptEntranceId = 2007402;
    private static readonly uint[] AdditionalChambers = { 2004353, 2004624, 2004625, 2004626, 2008126, 2011571 };
    private static readonly uint[] HouseExits = { 2002738, 2004361, 2007444 };
    private const uint WorkShopExit = 2005124;
    /// <summary>
    /// Zone IDs for FC workshop areas in all five residential districts, in the order
    /// Mist, Lavender Beds, The Goblet, Shirogane, and Empyreum.
    /// </summary>
    public static readonly ushort[] WorkshopZones = { 423, 424, 425, 653, 984 };

    /// <summary>
    /// Returns to the specified <paramref name="previousHouseLocation"/>, entering the house if
    /// the player is not already inside it.
    /// </summary>
    /// <param name="previousHouseLocation">
    /// The house to return to, or <see langword="null"/> to abort immediately.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the player has successfully entered the target house;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> GoBackToHouse(HouseLocation? previousHouseLocation)
    {
        if (previousHouseLocation == null)
        {
            return false;
        }

        // Apartments and FC private chambers need their own entry flow.
        if (previousHouseLocation.Room.HasValue && previousHouseLocation.RoomKind != HousingRoomKind.None)
        {
            return await GoBackToRoom(previousHouseLocation);
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
            Log.Error("Failed to get to residential");
            return false;
        }

        return await GetIntoHouse(previousHouseLocation.HousingZone, previousHouseLocation.Plot) && CurrentHouseLocation != null && CurrentHouseLocation.Equals(previousHouseLocation);
    }

    /// <summary>
    /// Travels to a saved <see cref="HouseLocation"/>: enters the house, or the apartment / FC
    /// private chamber when the location carries a room (see <see cref="HouseLocation.RoomKind"/>).
    /// This is the entry point external callers should use to reach a saved location.
    /// </summary>
    /// <param name="location">The saved location to travel to.</param>
    /// <returns><see langword="true"/> when the player is at the location; otherwise <see langword="false"/>.</returns>
    public static Task<bool> GoToHouseLocation(HouseLocation? location)
    {
        return GoBackToHouse(location);
    }

    private static readonly HousingZone[] ApartmentZones =
    {
        HousingZone.ApartmentMist, HousingZone.ApartmentLavenderBeds, HousingZone.ApartmentGoblet,
        HousingZone.ApartmentShirogane, HousingZone.ApartmentEmpyreum,
    };

    private static bool IsApartmentZone(HousingZone zone) => ApartmentZones.Contains(zone);

    /// <summary>
    /// Returns the player to a saved apartment or FC private chamber, teleporting to the ward first
    /// and then taking the personal or specified entry option depending on whether it is the
    /// player's own room.
    /// </summary>
    /// <param name="target">The room to return to.</param>
    /// <returns>
    /// <see langword="true"/> when the player is inside the target room; otherwise <see langword="false"/>.
    /// </returns>
    private static async Task<bool> GoBackToRoom(HouseLocation target)
    {
        if (HousingHelper.IsInsideRoom && CurrentHouseLocation != null && CurrentHouseLocation.Equals(target))
        {
            return true;
        }

        var ownRoom = IsOwnRoom(target);

        if (target.RoomKind == HousingRoomKind.Apartment)
        {
            // An apartment isn't a navigable plot (its stored plot is the 128/129 division marker).
            // Navigate to the recorded apartment-building entrance in the target ward, then enter.
            var district = HousingTraveler.GetResidentialDistrictByZone((ushort)target.HousingZone);
            var entrance = district?.GetApartmentEntrance(target.Subdivision) ?? Vector3.Zero;
            if (district == null || entrance.Equals(Vector3.Zero))
            {
                Log.Error($"No apartment building entrance recorded for {target.HousingZone}.");
                return false;
            }

            if (!await HousingTraveler.GetToResidential(target.World, target.HousingZone, entrance, target.Ward))
            {
                Log.Error("Failed to navigate to the apartment building.");
                return false;
            }

            return await GoIntoApartment(target, ownRoom);
        }

        // FC private chamber: reach the FC estate plot, then enter through the chamber entrance.
        if (!await HousingTraveler.GetToResidential(target))
        {
            Log.Error("Failed to reach the residential ward for the room");
            return false;
        }

        return await GoIntoPrivateChambers(target, ownRoom);
    }

    /// <summary>
    /// Determines whether <paramref name="target"/> is one of the player's own registered rooms,
    /// allowing the direct "go to your room" option instead of room-number entry.
    /// </summary>
    private static bool IsOwnRoom(HouseLocation target)
    {
        HousingHelper.UpdateResidenceArray();
        foreach (var residence in HousingHelper.Residences)
        {
            if (residence == null || (!residence.IsApartment && !residence.IsFcRoom))
            {
                continue;
            }

            if (HousingTraveler.TranslateZone((HousingZone)residence.Zone) == HousingTraveler.TranslateZone(target.HousingZone)
                && residence.Ward == target.Ward
                && residence.Room == (target.Room ?? 0))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Enters an apartment from the apartment building entrance in the current ward.
    /// </summary>
    /// <param name="target">The apartment room to enter.</param>
    /// <param name="ownApartment">
    /// <see langword="true"/> to take the direct "go to your apartment" option; otherwise the
    /// "specified apartment" option is used (room-number entry — see <see cref="EnterSpecifiedApartment"/>).
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the player is inside the apartment; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> GoIntoApartment(HouseLocation target, bool ownApartment)
    {
        if (HousingHelper.IsInsideRoom)
        {
            return true;
        }

        // GetToResidential already placed us at the correct apartment building, so only that
        // building's entrance is loaded — the nearest match is the right one.
        var entrance = GameObjectManager.GetObjectsByNPCIds<GameObject>(new[] { AptEntranceId }).OrderBy(gameObject => gameObject.Distance()).FirstOrDefault();
        if (entrance == null)
        {
            Log.Error("No apartment entrance found in this ward");
            return false;
        }

        if (!await NavigationHelper.InteractWithNpc(entrance) || !await Coroutine.Wait(10000, () => Conversation.IsOpen))
        {
            Log.Error("Could not open the apartment entrance menu");
            return false;
        }

        var lines = Conversation.GetConversationList;
        var option = ownApartment ? Translator.ApartmentGoToYourRoom : Translator.ApartmentGoToSpecifiedRoom;
        var index = lines.TakeWhile(line => !line.Contains(option)).Count();
        if (index == lines.Count)
        {
            Log.Error("Could not find the apartment menu option");
            return false;
        }

        Conversation.SelectLine((uint)index);

        if (!ownApartment && !await EnterSpecifiedApartment(target.Room ?? 0))
        {
            return false;
        }

        if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
        {
            return false;
        }

        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
        return HousingHelper.IsInsideRoom;
    }

    /// <summary>
    /// Enters a Free Company private chamber. Requires reaching the FC estate interior, where the
    /// additional-chambers entrance is used.
    /// </summary>
    /// <param name="target">The FC private chamber to enter.</param>
    /// <param name="ownRoom">
    /// <see langword="true"/> for the player's own chamber; otherwise the "specified private
    /// chambers" option is used (room-number entry — see <see cref="EnterSpecifiedFcRoom"/>).
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the player is inside the chamber; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> GoIntoPrivateChambers(HouseLocation target, bool ownRoom)
    {
        if (HousingHelper.IsInsideRoom)
        {
            return true;
        }

        if (!HousingHelper.IsInsideHouse && !await GetIntoHouse(target.HousingZone, target.Plot))
        {
            Log.Error("Could not enter the FC estate to reach the private chambers");
            return false;
        }

        var entrance = GameObjectManager.GetObjectsByNPCIds<GameObject>(AdditionalChambers).OrderBy(gameObject => gameObject.Distance()).FirstOrDefault();
        if (entrance == null)
        {
            Log.Error("No private chamber entrance found");
            return false;
        }

        if (!await NavigationHelper.InteractWithNpc(entrance) || !await Coroutine.Wait(10000, () => Conversation.IsOpen))
        {
            Log.Error("Could not open the private chamber menu");
            return false;
        }

        var lines = Conversation.GetConversationList;
        var option = ownRoom ? Translator.HousePersonalRoom : Translator.HouseOtherRoom;
        var index = lines.TakeWhile(line => !line.Contains(option)).Count();
        if (index == lines.Count)
        {
            Log.Error("Could not find the private chamber menu option");
            return false;
        }

        Conversation.SelectLine((uint)index);

        if (!ownRoom && !await EnterSpecifiedFcRoom(target.Room ?? 0))
        {
            return false;
        }

        if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
        {
            return false;
        }

        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
        return HousingHelper.IsInsideRoom;
    }

    /// <summary>
    /// Enters a specified (non-personal) apartment number through the <c>MansionSelectRoom</c> window.
    /// </summary>
    private static async Task<bool> EnterSpecifiedApartment(int roomNumber)
    {
        if (roomNumber <= 0)
        {
            Log.Error($"Invalid apartment number {roomNumber}");
            return false;
        }

        if (!await Coroutine.Wait(10000, () => MansionSelectRoom.Instance.IsOpen))
        {
            Log.Error("MansionSelectRoom window did not open");
            return false;
        }

        AgentMansionSelectRoom.Instance.SelectApartment(roomNumber);
        return true;
    }

    /// <summary>
    /// Enters a specified (non-personal) FC private chamber number through the <c>HousingSelectRoom</c> window.
    /// </summary>
    private static async Task<bool> EnterSpecifiedFcRoom(int roomNumber)
    {
        if (roomNumber <= 0)
        {
            Log.Error($"Invalid FC room number {roomNumber}");
            return false;
        }

        if (!await Coroutine.Wait(10000, () => HousingSelectRoom.Instance.IsOpen))
        {
            Log.Error("HousingSelectRoom window did not open");
            return false;
        }

        AgentPersonalRoomPortal.Instance.SelectRoom(roomNumber);
        return true;
    }



    /// <summary>
    /// Navigates to the outdoor plot for <paramref name="previousHouseLocation"/> without
    /// entering the house interior.
    /// </summary>
    /// <param name="previousHouseLocation">
    /// The plot to return to, or <see langword="null"/> to abort immediately.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the player is standing within the target plot;
    /// otherwise <see langword="false"/>.
    /// </returns>
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

        if (recorded == null)
        {
            Log.Error("Failed to get recorded plot");
            return false;
        }

        await Navigation.GroundMove(recorded.EntranceLocation, 1f);

        if (!HousingHelper.IsWithinPlot)
        {
            Core.Me.Face(recorded.CenterLocation);
            Log.Error("Not within plot");
            MovementManager.MoveForwardStart();
            await Coroutine.Wait(5000, () => HousingHelper.IsWithinPlot);
            MovementManager.MoveStop();
        }

        return HousingHelper.IsWithinPlot && CurrentHouseLocation != null && CurrentHouseLocation.Equals(previousHouseLocation);
    }

    /// <summary>
    /// Returns to the outdoor housing area described by <paramref name="previousHouseLocation"/>,
    /// switching world and ward as necessary.
    /// </summary>
    /// <param name="previousHouseLocation">
    /// The area snapshot to return to, or <see langword="null"/> to abort immediately.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the player is back in the correct housing area at approximately
    /// the recorded position; otherwise <see langword="false"/>.
    /// </returns>
    internal static async Task<bool> GoBackToHousingLocation(HousingAreaLocation? previousHouseLocation)
    {
        if (previousHouseLocation == null)
        {
            return false;
        }

        if (HousingHelper.IsInHousingArea && CurrentHousingAreaLocation != null && CurrentHousingAreaLocation.Equals(previousHouseLocation))
        {
            return true;
        }

        if (!await WorldTravel.WorldTravel.GoToWorld(previousHouseLocation.World))
        {
            Log.Error("Failed to get to world");
            return false;
        }

        /*
        if (HousingHelper.IsInsideHouse && HousingTraveler.TranslateZone((HousingZone)WorldManager.ZoneId) == HousingTraveler.TranslateZone((HousingZone)previousHouseLocation.HousingZone))
        {
            Log.Error("Failed to get to house");
            return false;
        }*/

        if (!await HousingTraveler.GetToResidential(previousHouseLocation.HousingZone, previousHouseLocation.Location, previousHouseLocation.Ward))
        {
            Log.Error("Failed to get to housing area");
            return false;
        }

        await Navigation.GroundMove(previousHouseLocation.Location, 1f);

        var current = CurrentHousingAreaLocation;
        return HousingHelper.IsInHousingArea && current != null && current.Ward == previousHouseLocation.Ward && current.HousingZone == previousHouseLocation.HousingZone && current.World == previousHouseLocation.World && current.Location.DistanceSqr(previousHouseLocation.Location) < 10;
    }

    /// <summary>
    /// Enters the house at the specified <paramref name="plot"/> within <paramref name="housingZone"/>
    /// by interacting with the plot's entrance object.
    /// </summary>
    /// <param name="housingZone">The outdoor ward zone that contains the target plot.</param>
    /// <param name="plot">The 1-based plot number to enter.</param>
    /// <returns>
    /// <see langword="true"/> when the player has entered the house; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> GetIntoHouse(HousingZone housingZone, int plot)
    {
        if (!ResourceManager.HousingPlots.TryGetValue(housingZone, out var plots))
        {
            Log.Error("Failed to get housing plot");
            return false;
        }

        var plot1 = plots.Value.FirstOrDefault(i => i.Value.Plot == plot).Value;

        return await plot1.Enter();
    }

    /// <summary>
    /// Enters the nearest house in the current zone.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the player has entered the house; otherwise <see langword="false"/>.
    /// </returns>
    [Obsolete("Use GetIntoHouse with housing zone and plot parameters instead")]
    public static async Task<bool> GetIntoHouse()
    {
        if (!ResourceManager.HousingPlots.TryGetValue((HousingZone)WorldManager.ZoneId, out var plots))
        {
            Log.Error("Failed to get housing plot");
            return false;
        }

        var plot = plots.Value.OrderBy(i => i.Value.PlacardLocation.DistanceSqr(Core.Me.Location)).FirstOrDefault().Value;

        return await plot.Enter();
    }

    /// <summary>
    /// Navigates from the player's FC estate house into the attached workshop (Additional Chambers).
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the player is inside the workshop;
    /// <see langword="false"/> when the player is not on the home world, does not have an FC
    /// estate, or the workshop entrance cannot be found.
    /// </returns>
    public static async Task<bool> GoIntoWorkshop()
    {
        if (WorkshopZones.Contains(WorldManager.ZoneId) || HousingHelper.IsInsideWorkshop)
        {
            return true;
        }

        if (!WorldHelper.IsOnHomeWorld)
        {
            return false;
        }

        if (HousingHelper.Residences.Any(i => i.HouseLocationIndex == HouseLocationIndex.FreeCompanyEstate) && !HousingHelper.IsInsideWorkshop && !GameObjectManager.GetObjectsByNPCIds<GameObject>(AdditionalChambers).Any())
        {
            var estate = HousingHelper.Residences.First(i => i.HouseLocationIndex == HouseLocationIndex.FreeCompanyEstate);
            await HousingTraveler.GetToResidential(((HouseLocation?)estate)!);
            await GetIntoHouse((HousingZone)estate.Zone, estate.Plot);
        }

        if (!HousingHelper.IsInsideHouse)
        {
            return false;
        }

        var gameObject = GameObjectManager.GetObjectsByNPCIds<GameObject>(AdditionalChambers);

        if (gameObject == null || !gameObject.Any())
        {
            return false;
        }

        var entrance = gameObject.FirstOrDefault();

        if (entrance == null)
        {
            return false;
        }

        if (!await NavigationHelper.InteractWithNpc(entrance) || !await Coroutine.Wait(10000, () => Conversation.IsOpen))
        {
            Log.Error("Could not get to workshop entrance");
            return false;
        }

        var test = Conversation.GetConversationList.TakeWhile(line => !line.Contains(Translator.HouseWorkshop)).Count();

        if (test == Conversation.GetConversationList.Count)
        {
            return false;
        }

        Conversation.SelectLine((uint)test);

        if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
        {
            return false;
        }

        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);

        return HousingHelper.IsInsideWorkshop;
    }

    /// <summary>
    /// Leaves the workshop by interacting with the exit NPC and confirming the dialogue.
    /// Does nothing if the player is not currently inside a workshop zone.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the player has left the workshop (or was never inside one);
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> LeaveWorkshop()
    {
        if (!WorkshopZones.Contains(WorldManager.ZoneId) || !HousingHelper.IsInsideWorkshop)
        {
            return true;
        }

        var gameObject = GameObjectManager.GetObjectByNPCId<GameObject>(WorkShopExit);

        if (gameObject == null)
        {
            return false;
        }

        if (!await NavigationHelper.InteractWithNpc(gameObject) || !await Coroutine.Wait(10000, () => SelectYesno.IsOpen))
        {
            Log.Error("Could not get to workshop exit");
            return false;
        }

        SelectYesno.Yes();

        if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
        {
            return false;
        }

        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);

        return !HousingHelper.IsInsideWorkshop;
    }

    /// <summary>
    /// Gets a <see cref="HouseLocation"/> representing the house or plot the player is currently
    /// inside or standing on, or <see langword="null"/> when not in a housing interior or plot.
    /// </summary>
    internal static HouseLocation? CurrentHouseLocation
    {
        get
        {
            if (!HousingHelper.IsInsideHouse && !HousingHelper.IsWithinPlot && !HousingHelper.IsInsideWorkshop && !HousingHelper.IsInsideRoom)
            {
                Log.Information("Not inside house or plot");
                return null;
            }

            var info = HousingHelper.HousingPositionInfo;
            if (!info)
            {
                Log.Error("Failed to get housing position info");
                return null;
            }

            var rawZone = (HousingZone)WorldManager.ZoneId;
            int? room = info.Room != 0 ? info.Room : (int?)null;
            var roomKind = room is null
                ? HousingRoomKind.None
                : IsApartmentZone(rawZone) ? HousingRoomKind.Apartment : HousingRoomKind.FreeCompanyRoom;

            return new HouseLocation(HousingTraveler.TranslateZone(rawZone), info.Ward, info.Plot, room, roomKind, info.Subdivision);
        }
    }

    /// <summary>
    /// Gets a <see cref="HousingAreaLocation"/> snapshot of the player's current housing-area
    /// position, or <see langword="null"/> when not in any housing area.
    /// </summary>
    internal static HousingAreaLocation? CurrentHousingAreaLocation
    {
        get
        {
            if (!HousingHelper.IsInHousingArea)
            {
                return null;
            }

            return new HousingAreaLocation
            {
                HousingZone = WorldManager.ZoneId,
                Ward = HousingHelper.HousingPositionInfo.Ward,
                World = WorldHelper.CurrentWorld,
                Location = Core.Me.Location
            };
        }
    }
}

/// <summary>
/// Immutable snapshot of a player's position within a housing area (outdoor ward), capturing the
/// zone, ward number, world, and in-world coordinates.
/// </summary>
public record HousingAreaLocation
{
    /// <summary>Gets or sets the zone ID of the outdoor housing ward.</summary>
    public ushort HousingZone { get; set; }

    /// <summary>Gets or sets the 1-based ward number.</summary>
    public int Ward { get; set; }

    /// <summary>Gets or sets the world the housing area is on.</summary>
    public World World { get; set; }

    /// <summary>Gets or sets the player's position within the housing area.</summary>
    public Vector3 Location { get; set; }
}