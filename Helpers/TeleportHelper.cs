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
        private static readonly string Name = "TeleportHelper";
        private static readonly LLogger Log = new LLogger(Name, Colors.MediumTurquoise);

        private static class Offsets
        {
            [Offset("48 89 5C 24 ? 48 89 74 24 ? 48 89 7C 24 ? 55 41 54 41 55 41 56 41 57 48 8D AC 24 ? ? ? ? 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 85 ? ? ? ? 4C 8B E9 33 D2")]
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
            CallUpdate();
            _lastUpdate = DateTime.Now;
            _lastUpdateWorld = WorldHelper.CurrentWorld;
            _teleportList = Core.Memory.Read<Telepo>(LlamaLibrary.Memory.Offsets.UIStateTelepo).TeleportInfos;
            Log.Information("Updating teleport");
        }

        public static void CallUpdate()
        {
            Core.Memory.CallInjected64<IntPtr>(Offsets.UpdatePlayerAetheryteList, LlamaLibrary.Memory.Offsets.UIStateTelepo, 0);
        }

        public static async Task<bool> TeleportToPrivateEstate()
        {
            int house = -1;

            for (var index = 0; index < TeleportList.Length; index++)
            {
                if (TeleportList[index].IsOwnHouse)
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
            int house = -1;

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
            int house = -1;
            int count = 0;
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
            int house = -1;
            int count = 0;
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