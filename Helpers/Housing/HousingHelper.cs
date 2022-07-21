using System;
using System.Linq;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers.Housing
{
    public static class HousingHelper
    {
        internal static class Offsets
        {
            [Offset("48 39 1D ? ? ? ? 75 ? 45 33 C0 33 D2 B9 ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B C8 E8 ? ? ? ? 48 89 05 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr PositionInfoAddress;

            [Offset("48 8B 05 ? ? ? ? 48 83 F8 ? 74 ? 48 C1 E8 ? 0F B7 C8 Add 3 TraceRelative")]
            internal static IntPtr HouseLocationArray;
        }

        private static DateTime _lastHousingUpdate;

        public static World _lastUpdateWorld;

        private static ResidenceInfo[] _residences;

        public static ResidenceInfo[] Residences
        {
            get
            {
                if ((DateTime.Now.Subtract(_lastHousingUpdate).TotalMinutes > 5 && WorldHelper.IsOnHomeWorld) || (WorldHelper.IsOnHomeWorld && _lastUpdateWorld != WorldHelper.HomeWorld))
                {
                    UpdateResidenceArray();
                }

                return _residences;
            }
        }

        public static HouseLocation[] AccessibleHouseLocations => Residences.Select(i => (HouseLocation)i).ToArray();

        public static HouseLocation PersonalEstate => AccessibleHouseLocations[(int)HouseLocationIndex.PrivateEstate];
        public static HouseLocation FreeCompanyEstate => AccessibleHouseLocations[(int)HouseLocationIndex.FreeCompanyEstate];
        public static HouseLocation[] SharedEstates => new[] { AccessibleHouseLocations[(int)HouseLocationIndex.SharedEstate1], AccessibleHouseLocations[(int)HouseLocationIndex.SharedEstate2] };

        public static IntPtr PositionPointer => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Offsets.PositionInfoAddress));

        public static bool IsInHousingArea => HousingPositionInfo.Ward != default;

        public static bool IsInsideHouse => HousingPositionInfo.InHouse;

        public static bool IsInsideRoom => HousingPositionInfo.Room != default;

        public static bool IsWithinPlot => HousingPositionInfo.Plot != default;

        static HousingHelper()
        {
            UpdateResidenceArray();
        }

        public static void UpdateResidenceArray()
        {
            _lastHousingUpdate = DateTime.Now;
            _lastUpdateWorld = WorldHelper.CurrentWorld;
            _residences = Core.Memory.ReadArray<ResidenceInfo>(Offsets.HouseLocationArray, 6);
        }

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