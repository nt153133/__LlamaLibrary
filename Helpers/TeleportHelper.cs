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

namespace LlamaLibrary.Helpers;

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

    public static void UpdateTeleportArray()
    {
        CallUpdate();
        _lastUpdate = DateTime.Now;
        _lastUpdateWorld = WorldHelper.CurrentWorld;
        _teleportList = Core.Memory.Read<Telepo>(Memory.Offsets.UIStateTelepo).TeleportInfos;
    }

    public static void CallUpdate()
    {
        Core.Memory.CallInjectedWraper<IntPtr>(Offsets.UpdatePlayerAetheryteList, Memory.Offsets.UIStateTelepo, 0);
    }

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

    public static bool TeleportUsingTicket(uint ae, byte subIndex = 0)
    {
        return Core.Memory.CallInjectedWraper<byte>(Offsets.TeleportWithSettings, Offsets.Telepo, ae, subIndex) != 0;
    }

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

    public static async Task<bool> TeleportByIndexTicket(int index)
    {
        var location = TeleportList[index];
        return await TeleportWithTicket(location);
    }

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

    private static class Offsets
    {
        //7.1
        [Offset("Search E8 ? ? ? ? 49 89 44 24 ? 4C 8B F8 TraceCall")]
        //[OffsetCN("Search E8 ? ? ? ? 49 89 47 68 TraceCall")]
        internal static IntPtr UpdatePlayerAetheryteList;

        [Offset("Search E8 ? ? ? ? 48 8B 4B ? 84 C0 48 8B 01 74 ? Add 1 TraceRelative")]
        internal static IntPtr TeleportWithSettings;

        [Offset("Search 48 8D 0D ? ? ? ? 8B FA E8 ? ? ? ? 48 8B 4B ? Add 3 TraceRelative")]
        internal static IntPtr Telepo;
    }
}