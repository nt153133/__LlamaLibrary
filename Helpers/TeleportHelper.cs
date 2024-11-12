using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    public static class TeleportHelper
    {
        private static readonly LLogger Log = new(nameof(TeleportHelper), Colors.MediumTurquoise);

        private static class Offsets
        {
            //7.1
            [Offset("Search E8 ? ? ? ? 49 89 44 24 ? 4C 8B F8 TraceCall")]
            [OffsetCN("Search E8 ? ? ? ? 49 89 47 68 TraceCall")]
            internal static IntPtr UpdatePlayerAetheryteList;
        }

        private static DateTime _lastUpdate;

        private static World _lastUpdateWorld;

        private static TeleportInfo[] _teleportList;

        public static TeleportInfo[] TeleportList
        {
            get
            {
                if ((DateTime.Now.Subtract(_lastUpdate).TotalMinutes > 5) || (WorldHelper.IsOnHomeWorld && _lastUpdateWorld != WorldHelper.HomeWorld))
                {
                    UpdateTeleportArray();
                }

                return _teleportList;
            }
        }

        static TeleportHelper()
        {
            UpdateTeleportArray();
        }

        public static void UpdateTeleportArray()
        {
            //Log.Information("Updating teleport");
            CallUpdate();
            _lastUpdate = DateTime.Now;
            _lastUpdateWorld = WorldHelper.CurrentWorld;
            _teleportList = Core.Memory.Read<Telepo>(LlamaLibrary.Memory.Offsets.UIStateTelepo).TeleportInfos;
            //Log.Information("Finished updating teleport");
        }

        public static void CallUpdate()
        {
            Core.Memory.CallInjected64<IntPtr>(Offsets.UpdatePlayerAetheryteList, LlamaLibrary.Memory.Offsets.UIStateTelepo, 0);
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

            return await TeleportByIndex(house);
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

            return await TeleportByIndex(house);
        }

        public static async Task<bool> TeleportToFreeCompanyEstate()
        {
            var house = -1;

            for (var index = 0; index < TeleportList.Length; index++)
            {
                if (TeleportList[index].IsFCHouse)
                {
                    house = index;
                    break;
                }
            }

            if (house == -1)
            {
                Log.Information("Can't find teleport");
                foreach (var teleportInfo in TeleportList.Take(5))
                {
                    Log.Information(teleportInfo.ToString());
                }

                return false;
            }

            return await TeleportByIndex(house);
        }

        public static async Task<bool> TeleportToSharedEstate(int estateIndex)
        {
            var house = -1;
            var count = 0;
            for (var index = 0; index < TeleportList.Length; index++)
            {
                if (TeleportList[index].IsSharedHouse)
                {
                    if (count == estateIndex)
                    {
                        house = index;
                        break;
                    }

                    count++;
                }
            }

            if (house == -1)
            {
                Log.Information("Can't find teleport");
                return false;
            }

            return await TeleportByIndex(house);
        }

        public static async Task<bool> TeleportToSharedEstate(ushort zone, int ward, int plot)
        {
            var house = -1;
            var count = 0;
            for (var index = 0; index < TeleportList.Length; index++)
            {
                if (TeleportList[index].IsSharedHouse)
                {
                    if (TeleportList[index].TerritoryId == zone && TeleportList[index].Ward == ward && TeleportList[index].Plot == plot)
                    {
                        house = index;
                        break;
                    }

                    count++;
                }
            }

            if (house == -1)
            {
                Log.Information("Can't find teleport");
                return false;
            }

            return await TeleportByIndex(house);
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
                    Log.Information("WTF can't teleport");
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

            Log.Error("Shit failed trying to teleport");
            return false;
        }
    }
}