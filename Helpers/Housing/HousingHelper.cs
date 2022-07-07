using System;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers.Housing
{
    public static class HousingHelper
    {
        internal static class Offsets
        {
            [Offset("48 39 1D ? ? ? ? 75 ? 45 33 C0 33 D2 B9 ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B C8 E8 ? ? ? ? 48 89 05 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr PositionInfoAddress;
        }

        public static IntPtr PositionPointer => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Offsets.PositionInfoAddress));

        public static bool IsInHousingArea => HousingPositionInfo.Ward != default;

        public static bool IsInsideHouse => HousingPositionInfo.InHouse;

        public static bool IsInsideRoom => HousingPositionInfo.Room != default;

        public static bool IsWithinPlot => HousingPositionInfo.Plot != default;

        public static HousingPositionInfo HousingPositionInfo
        {
            get
            {
                var positionPointer = PositionPointer;

                if (positionPointer != IntPtr.Zero)
                {
                    return new HousingPositionInfo(positionPointer);
                }

                return default;
            }
        }

        public static HouseLocation CurrentHouseLocation
        {
            get
            {
                if (!IsInHousingArea || !IsWithinPlot)
                {
                    return null;
                }

                var info = HousingPositionInfo;
                return new HouseLocation((HousingZone)WorldManager.ZoneId, info.Ward, info.Plot);
            }
        }

        public static string ToString()
        {
            return $"IsInHousingArea: {IsInHousingArea}, IsInsideHouse: {IsInsideHouse}, IsInsideRoom: {IsInsideRoom}, IsWithinPlot: {IsWithinPlot}, HousingPositionInfo: {HousingPositionInfo.DynamicString()}";
        }
    }
}