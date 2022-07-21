using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Logging;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    public static class TeleportHelper
    {
        private static readonly string Name = "TeleportHelper";
        private static readonly LLogger Log = new LLogger(Name, Colors.MediumTurquoise);

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
            _lastUpdate = DateTime.Now;
            _lastUpdateWorld = WorldHelper.CurrentWorld;
            _teleportList = Core.Memory.Read<Telepo>(LlamaLibrary.Memory.Offsets.UIStateTelepo).TeleportInfos;
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
                WorldManager.Teleport(index);
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