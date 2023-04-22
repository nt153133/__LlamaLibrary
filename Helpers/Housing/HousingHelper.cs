using System;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers.Housing
{
    public static class HousingHelper
    {
        internal static class Offsets
        {
            [Offset("Search 48 39 1D ? ? ? ? 75 ? 45 33 C0 33 D2 B9 ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B C8 E8 ? ? ? ? 48 89 05 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr PositionInfoAddress;

            [Offset("Search 48 8B 05 ? ? ? ? 48 83 F8 ? 74 ? 48 C1 E8 ? 0F B7 C8 Add 3 TraceRelative")]
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

        //public static HouseLocation?[] AccessibleHouseLocations => Residences.Select(i => (HouseLocation?)i).ToArray();

        public static HouseLocation? PersonalEstate => Residences.FirstOrDefault(i=> i.HouseLocationIndex == HouseLocationIndex.PrivateEstate);
        public static HouseLocation? FreeCompanyEstate => Residences.FirstOrDefault(i=> i.HouseLocationIndex == HouseLocationIndex.FreeCompanyEstate);
        public static HouseLocation?[] SharedEstates => Residences.Where(i=> i.HouseLocationIndex == HouseLocationIndex.SharedEstate1 || i.HouseLocationIndex == HouseLocationIndex.SharedEstate2).Select(i => (HouseLocation?)i).ToArray();

        public static IntPtr PositionPointer
        {
            get
            {
                var housingManager = HousingManager;
                if (!housingManager.HasValue)
                {
                    return IntPtr.Zero;
                }

                if (housingManager.Value.CurrentTerritory == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                return housingManager.Value.CurrentTerritory;
            }
        }

        public static bool IsInHousingArea
        {
            get
            {
                var housingManager = HousingManager;
                if (!housingManager.HasValue)
                {
                    return false;
                }

                if (housingManager.Value.CurrentTerritory == IntPtr.Zero)
                {
                    return false;
                }

                return housingManager.Value.CurrentTerritory != IntPtr.Zero;
            }
        }

        public static bool IsInsideHouse
        {
            get
            {
                var housingManager = HousingManager;
                if (!housingManager.HasValue)
                {
                    return false;
                }

                if (housingManager.Value.CurrentTerritory == IntPtr.Zero)
                {
                    return false;
                }

                return housingManager.Value.CurrentTerritory != IntPtr.Zero && housingManager.Value.CurrentTerritory == housingManager.Value.IndoorTerritory;
            }
        }

        public static bool IsInsideRoom => HousingPositionInfo.Room != default;

        public static bool IsWithinPlot
        {
            get
            {
                var housingManager = HousingManager;
                if (!housingManager.HasValue)
                {
                    return false;
                }

                if (housingManager.Value.CurrentTerritory == IntPtr.Zero)
                {
                    return false;
                }

                return housingManager.Value.CurrentTerritory != IntPtr.Zero && (housingManager.Value.CurrentTerritory == housingManager.Value.OutdoorTerritory || housingManager.Value.CurrentTerritory == housingManager.Value.IndoorTerritory) && HousingPositionInfo.Plot != default;
            }
        }

        public static HousingManagerStruct? HousingManager
        {
            get
            {
                try
                {
                    var pointer = Core.Memory.Read<IntPtr>(Offsets.PositionInfoAddress);
                    return pointer == IntPtr.Zero ? null : Core.Memory.Read<HousingManagerStruct>(pointer);
                }
                catch (Exception e)
                {
                    ff14bot.Helpers.Logging.WriteException(e);
                    return null;
                }
            }
        }

        static HousingHelper()
        {
            if (WorldHelper.IsOnHomeWorld)
            {
                UpdateResidenceArray();
            }
        }

        public static void UpdateResidenceArray()
        {
            if (_lastUpdateWorld == WorldHelper.CurrentWorld && DateTime.Now.Subtract(_lastHousingUpdate).TotalMinutes < 5)
            {
                return;
            }

            _lastHousingUpdate = DateTime.Now;
            _lastUpdateWorld = WorldHelper.CurrentWorld;
            try
            {
                ff14bot.Helpers.Logging.WriteDiagnostic("Updating Residence Array");
                //_residences = Core.Memory.ReadArray<ResidenceInfo>(Offsets.HouseLocationArray, 6);
                _residences = ResidentialHousingManager.GetResidences().ToArray();

                ff14bot.Helpers.Logging.WriteDiagnostic("Residence Array Updated");
            }
            catch (Exception e)
            {
                ff14bot.Helpers.Logging.WriteException(e);
                _residences = new ResidenceInfo[6];
            }
        }

        public static HousingPositionInfo HousingPositionInfo
        {
            get
            {
                try
                {
                    var positionPointer = PositionPointer;

                    if (positionPointer != IntPtr.Zero)
                    {
                        return new HousingPositionInfo(positionPointer);
                    }
                }
                catch (Exception e)
                {
                    ff14bot.Helpers.Logging.WriteException(e);
                    return new HousingPositionInfo(IntPtr.Zero);
                }

                return new HousingPositionInfo(IntPtr.Zero);
            }
        }

        public static HouseLocation? CurrentHouseLocation
        {
            get
            {
                if (!IsInHousingArea || !IsWithinPlot)
                {
                    return null;
                }

                var info = HousingPositionInfo;
                if (!info)
                {
                    return null;
                }

                return info.InHouse ? new HouseLocation((HousingZone)WorldManager.ZoneId, info.Ward, info.Plot) : null;
            }
        }

        public new static string ToString()
        {
            return $"IsInHousingArea: {IsInHousingArea}, IsInsideHouse: {IsInsideHouse}, IsInsideRoom: {IsInsideRoom}, IsWithinPlot: {IsWithinPlot}, HousingPositionInfo: {HousingPositionInfo.DynamicString()}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0xE0)]
    public struct HousingManagerStruct
    {
        [FieldOffset(0x00)]
        public IntPtr CurrentTerritory;

        [FieldOffset(0x08)]
        public IntPtr OutdoorTerritory;

        [FieldOffset(0x10)]
        public IntPtr IndoorTerritory;

        [FieldOffset(0x18)]
        public IntPtr WorkshopTerritory;
    }
}