using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Provides helper methods for teleporting to FFXIV aetherytes, housing estates, and other destinations.
/// Wraps injected game functions to trigger teleports and handles the full cast/loading sequence.
/// The teleport destination list is automatically cached and refreshed every 5 minutes or on world change.
/// </summary>
public static class TeleportHelper
{
    private static readonly LLogger Log = new(nameof(TeleportHelper), Colors.MediumTurquoise);

    private static DateTime _lastUpdate;

    private static World _lastUpdateWorld;

    private static TeleportInfo[] _teleportList;

    static TeleportHelper()
    {
        UpdateTeleportArray();
    }

    /// <summary>
    /// Gets the cached array of unlocked aetheryte teleport destinations available to the player.
    /// The list is automatically refreshed when stale (older than 5 minutes) or when the player's
    /// current world differs from the world recorded at the last update.
    /// </summary>
    /// <value>An array of <see cref="TeleportInfo"/> entries representing each unlocked destination.</value>
    public static TeleportInfo[] TeleportList
    {
        get
        {
            if (DateTime.Now.Subtract(_lastUpdate).TotalMinutes > 5 || (WorldHelper.IsOnHomeWorld && _lastUpdateWorld != WorldHelper.HomeWorld))
            {
                UpdateTeleportArray();
            }

            return _teleportList;
        }
    }

    /// <summary>
    /// Forces an immediate refresh of <see cref="TeleportList"/> by calling the injected game function
    /// and reading the updated aetheryte data from game memory. Records the current timestamp and world
    /// so the cache invalidation logic can detect future staleness.
    /// </summary>
    public static void UpdateTeleportArray()
    {
        CallUpdate();
        _lastUpdate = DateTime.Now;
        _lastUpdateWorld = WorldHelper.CurrentWorld;
        _teleportList = Core.Memory.Read<Telepo>(Offsets.UIStateTelepo).TeleportInfos;
    }

    /// <summary>
    /// Calls the injected FFXIV game function that refreshes the player's aetheryte destination list
    /// in the <c>UIStateTelepo</c> memory structure. Used internally by <see cref="UpdateTeleportArray"/>.
    /// </summary>
    public static void CallUpdate()
    {
        Core.Memory.CallInjectedWraper<IntPtr>(TeleportHelperOffsets.UpdatePlayerAetheryteList, Offsets.UIStateTelepo, 0);
    }

    /// <summary>
    /// Teleports the player to their own apartment in an FFXIV housing district.
    /// Searches <see cref="TeleportList"/> for an entry flagged as both <c>IsOwnHouse</c> and <c>IsApartment</c>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the teleport succeeded; <see langword="false"/> if no apartment destination
    /// was found or the teleport failed.
    /// </returns>
    public static async Task<bool> TeleportToApartment()
    {
        var house = -1;

        for (var index = 0; index < TeleportList.Length; index++)
        {
            if (TeleportList[index].IsOwnHouse && _teleportList[index].IsApartment)
            {
                house = index;
                break;
            }
        }

        if (house == -1)
        {
            Log.Information("Can't find teleport");
            return false;
        }

        return await TeleportWithTicket(TeleportList[house]);
    }

    /// <summary>
    /// Teleports the player to their own private estate (personal house, not an apartment) in an FFXIV housing district.
    /// Searches <see cref="TeleportList"/> for an entry flagged as <c>IsOwnHouse</c> but not <c>IsApartment</c>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the teleport succeeded; <see langword="false"/> if no private estate destination
    /// was found or the teleport failed.
    /// </returns>
    public static async Task<bool> TeleportToPrivateEstate()
    {
        var house = -1;

        for (var index = 0; index < TeleportList.Length; index++)
        {
            if (TeleportList[index].IsOwnHouse && !_teleportList[index].IsApartment)
            {
                house = index;
                break;
            }
        }

        if (house == -1)
        {
            Log.Information("Can't find teleport");
            return false;
        }

        return await TeleportWithTicket(TeleportList[house]);
    }

    /// <summary>
    /// Teleports the player to their Free Company's estate (FC house) in an FFXIV housing district.
    /// Searches <see cref="TeleportList"/> for the first entry flagged as <c>IsFCHouse</c>.
    /// Logs the first five teleport entries for diagnostics if no FC house destination is found.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the teleport succeeded; <see langword="false"/> if no FC estate was found
    /// or the teleport failed.
    /// </returns>
    public static async Task<bool> TeleportToFreeCompanyEstate()
    {
        var house = -1;

        for (var index = 0; index < TeleportList.Length; index++)
        {
            if (!TeleportList[index].IsFCHouse)
            {
                continue;
            }

            house = index;
            break;
        }

        if (house != -1)
        {
            return await TeleportWithTicket(TeleportList[house]);
        }

        Log.Information("Can't find teleport");
        foreach (var teleportInfo in TeleportList.Take(5))
        {
            Log.Information(teleportInfo.ToString());
        }

        return false;
    }

    /// <summary>
    /// Teleports the player to one of their shared (co-owner) housing estates by zero-based index.
    /// Iterates over entries in <see cref="TeleportList"/> flagged as <c>IsSharedHouse</c> and selects
    /// the one at the specified ordinal position.
    /// </summary>
    /// <param name="estateIndex">
    /// The zero-based index among all shared estates in <see cref="TeleportList"/>.
    /// Use <c>0</c> for the first shared estate, <c>1</c> for the second, and so on.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the teleport succeeded; <see langword="false"/> if no matching shared estate
    /// was found or the teleport failed.
    /// </returns>
    public static async Task<bool> TeleportToSharedEstate(int estateIndex)
    {
        var house = -1;
        var count = 0;
        for (var index = 0; index < TeleportList.Length; index++)
        {
            if (!TeleportList[index].IsSharedHouse)
            {
                continue;
            }

            if (count == estateIndex)
            {
                house = index;
                break;
            }

            count++;
        }

        if (house != -1)
        {
            return await TeleportWithTicket(TeleportList[house]);
        }

        Log.Information("Can't find teleport");
        return false;
    }

    /// <summary>
    /// Teleports the player to a specific shared (co-owner) housing estate identified by its territory zone,
    /// ward number, and plot number.
    /// </summary>
    /// <param name="zone">The territory/zone ID of the housing district.</param>
    /// <param name="ward">The ward number within the housing district.</param>
    /// <param name="plot">The plot number within the ward.</param>
    /// <returns>
    /// <see langword="true"/> if the teleport succeeded; <see langword="false"/> if no matching shared estate
    /// was found or the teleport failed.
    /// </returns>
    public static async Task<bool> TeleportToSharedEstate(ushort zone, int ward, int plot)
    {
        var house = -1;

        for (var index = 0; index < TeleportList.Length; index++)
        {
            if (!TeleportList[index].IsSharedHouse)
            {
                continue;
            }

            if (TeleportList[index].TerritoryId == zone && TeleportList[index].Ward == ward && TeleportList[index].Plot == plot)
            {
                house = index;
                break;
            }
        }

        if (house != -1)
        {
            return await TeleportWithTicket(TeleportList[house]);
        }

        Log.Information("Can't find teleport");
        return false;
    }

    private static async Task<bool> TeleportByIndex(int index)
    {
        return await TeleportByIndex((uint)index);
    }

    /// <summary>
    /// Teleports to an aetheryte using <c>WorldManager.Teleport(index)</c> with the given aetheryte index
    /// and waits for the full cast → loading screen → zone arrival sequence before returning.
    /// </summary>
    /// <param name="index">The aetheryte index in <c>WorldManager.AvailableLocations</c>.</param>
    /// <returns>
    /// <see langword="true"/> if the player arrives at the target zone; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> TeleportByIndex(uint index)
    {
        Log.Information($"Using teleport index {index}");
        if (WorldManager.CanTeleport() || await Coroutine.Wait(5000, WorldManager.CanTeleport))
        {
            if (!WorldManager.Teleport(index))
            {
                Log.Information("Can't teleport");
            }

            if (await Coroutine.Wait(5000, () => Core.Me.IsCasting) && await Coroutine.Wait(10000, () => !Core.Me.IsCasting))
            {
                if (await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading) && await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading))
                {
                    if (await Coroutine.Wait(10000, () => WorldManager.AvailableLocations[index].ZoneId == WorldManager.ZoneId))
                    {
                        await Coroutine.Sleep(1000);
                        return true;
                    }
                }
            }
        }

        Log.Error("Failed trying to teleport");
        return false;
    }

    /// <summary>
    /// Directly calls the injected FFXIV game function to initiate a teleport, consuming an aetheryte ticket
    /// if available, or gil otherwise. This is a low-level synchronous call — it does not wait for the
    /// teleport animation or loading screen.
    /// </summary>
    /// <param name="ae">The aetheryte ID to teleport to.</param>
    /// <param name="subIndex">
    /// The sub-index for aetheryte networks (e.g. aethernet shards within a city-state).
    /// Use <c>0</c> for the primary (city) aetheryte.
    /// </param>
    /// <returns><see langword="true"/> if the game accepted the teleport request; otherwise <see langword="false"/>.</returns>
    public static bool TeleportUsingTicket(uint ae, byte subIndex = 0)
    {
        return Core.Memory.CallInjectedWraper<byte>(TeleportHelperOffsets.TeleportWithSettings, TeleportHelperOffsets.Telepo, ae, subIndex) != 0;
    }

    /// <summary>
    /// Teleports to the aetheryte represented by an <see cref="AetheryteResult"/> using the ticket method.
    /// Waits up to 5 seconds for teleport eligibility before attempting, then delegates to <see cref="TeleportByIdTicket"/>.
    /// </summary>
    /// <param name="aetheryte">The aetheryte destination to teleport to.</param>
    /// <returns>
    /// <see langword="true"/> if the player arrives at the aetheryte's zone; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> TeleportWithTicket(AetheryteResult aetheryte)
    {
        Log.Information($"Using teleport location {aetheryte.CurrentLocaleAethernetName}");

        if (!WorldManager.CanTeleport() && !await Coroutine.Wait(5000, WorldManager.CanTeleport))
        {
            Log.Error("Can't teleport");
            return false;
        }

        return await TeleportByIdTicket(aetheryte.Id);
    }

    /// <summary>
    /// Teleports to the destination described by a <see cref="TeleportInfo"/> entry using the ticket method.
    /// Waits for teleport eligibility, calls <see cref="TeleportUsingTicket"/>, and then waits for the
    /// full cast/loading sequence via <see cref="HandleTeleport"/>.
    /// </summary>
    /// <param name="location">The <see cref="TeleportInfo"/> destination from <see cref="TeleportList"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the player arrives at the target zone; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> TeleportWithTicket(TeleportInfo location)
    {
        Log.Information($"Using teleport location {location.ZoneName}");

        if (!WorldManager.CanTeleport() && !await Coroutine.Wait(5000, WorldManager.CanTeleport))
        {
            Log.Error("Can't teleport");
            return false;
        }

        if (!TeleportUsingTicket(location.AetheryteId, location.SubIndex))
        {
            Log.Error("Teleport Failed");
            return false;
        }

        return await HandleTeleport(location.TerritoryId);
    }

    /// <summary>
    /// Teleports to the entry at the specified zero-based index in <see cref="TeleportList"/> using the ticket method.
    /// </summary>
    /// <param name="index">The zero-based index into <see cref="TeleportList"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the teleport succeeded; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> TeleportByIndexTicket(int index)
    {
        var location = TeleportList[index];
        return await TeleportWithTicket(location);
    }

    /// <summary>
    /// Teleports to a specific aetheryte by its ID using the ticket method.
    /// Resolves the target territory zone from the aetheryte ID, calls <see cref="TeleportUsingTicket"/>,
    /// and waits for the full cast/loading sequence to complete.
    /// </summary>
    /// <param name="aetheryteId">The aetheryte ID to teleport to (key in <c>DataManager.AetheryteCache</c>).</param>
    /// <returns>
    /// <see langword="true"/> if the player arrives at the aetheryte's zone; otherwise <see langword="false"/>.
    /// </returns>
    public static async Task<bool> TeleportByIdTicket(uint aetheryteId)
    {
        Log.Information($"Using AE id {DataManager.AetheryteCache[aetheryteId].CurrentLocaleAethernetName}");
        if (!WorldManager.CanTeleport() && !await Coroutine.Wait(5000, WorldManager.CanTeleport))
        {
            Log.Error("Can't teleport");
            return false;
        }

        if (!TeleportUsingTicket(aetheryteId))
        {
            Log.Error("Teleport Failed");
            return false;
        }

        return await HandleTeleport((ushort)WorldManager.GetZoneForAetheryteId(aetheryteId));
    }

    private static async Task<bool> HandleTeleport(ushort newZone)
    {
        if (!await Coroutine.Wait(5000, () => Core.Me.IsCasting || SelectYesno.IsOpen))
        {
            Log.Error("Failed to cast");
            return false;
        }

        if (SelectYesno.IsOpen)
        {
            SelectYesno.Yes();
            if (!await Coroutine.Wait(5000, () => Core.Me.IsCasting))
            {
                Log.Error("Failed to cast");
                return false;
            }
        }

        if (await Coroutine.Wait(10000, () => !Core.Me.IsCasting))
        {
            if (await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading) && await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading))
            {
                if (await Coroutine.Wait(10000, () => newZone == WorldManager.ZoneId))
                {
                    await Coroutine.Sleep(1000);
                    return true;
                }
            }
        }

        Log.Error("Failed trying to teleport");
        return false;
    }

    
}