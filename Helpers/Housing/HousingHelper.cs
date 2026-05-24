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
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers.Housing
{
    /// <summary>
    /// Static helper that provides information about the player's current housing position,
    /// accessible residence registrations, and the state of the game's housing manager.
    /// </summary>
    /// <remarks>
    /// Housing state is read directly from game memory via the <c>HousingHelperOffsets</c> class.
    /// The <see cref="Residences"/> array is refreshed automatically when stale (older than 5 minutes)
    /// or when the player's home world changes.
    /// </remarks>
    public static class HousingHelper
    {

        private static DateTime _lastHousingUpdate;

        public static World _lastUpdateWorld;

        /// <summary>
        /// Gets the unique ID of the house the player is currently inside.
        /// Returns 0 when not inside a house.
        /// </summary>
        public static long CurrentHouseId => Core.Memory.CallInjectedWraper<long>(HousingHelperOffsets.GetCurrentHouseId, Core.Memory.Read<IntPtr>(HousingHelperOffsets.PositionInfoAddress));

        /// <summary>
        /// Gets the 1-based plot number of the house the player is currently on or inside.
        /// </summary>
        public static byte CurrentPlot => Core.Memory.CallInjectedWraper<byte>(HousingHelperOffsets.GetCurrentPlot, Core.Memory.Read<IntPtr>(HousingHelperOffsets.PositionInfoAddress));

        /// <summary>
        /// Gets the 1-based ward number the player is currently in.
        /// </summary>
        public static byte CurrentWard => Core.Memory.CallInjectedWraper<byte>(HousingHelperOffsets.GetCurrentWard, Core.Memory.Read<IntPtr>(HousingHelperOffsets.PositionInfoAddress));

        private static ResidenceInfo[] _residences;
        /// <summary>Gets the pointer to the game's housing manager instance.</summary>
        public static IntPtr HousingInstance => Core.Memory.Read<IntPtr>(HousingHelperOffsets.PositionInfoAddress);

        /// <summary>
        /// Gets the array of housing residences registered to the current character,
        /// automatically refreshing the cache when it is older than 5 minutes or stale.
        /// </summary>
        /// <example>
        /// <code>
        /// var availableHouses = HousingHelper.Residences.ToDictionary(i => i.HouseLocationIndex, i => (HouseLocation?)i);
        /// </code>
        /// </example>
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

        /// <summary>
        /// Gets the player's registered private estate, or <see langword="null"/> if they do not own one.
        /// </summary>
        public static HouseLocation? PersonalEstate => Residences.FirstOrDefault(i => i.HouseLocationIndex == HouseLocationIndex.PrivateEstate);

        /// <summary>
        /// Gets the player's registered Free Company estate, or <see langword="null"/> if unavailable.
        /// </summary>
        public static HouseLocation? FreeCompanyEstate => Residences.FirstOrDefault(i => i.HouseLocationIndex == HouseLocationIndex.FreeCompanyEstate);

        /// <summary>
        /// Gets all shared estates (up to two) registered to the player.
        /// </summary>
        public static HouseLocation?[] SharedEstates => Residences.Where(i => i.HouseLocationIndex == HouseLocationIndex.SharedEstate1 || i.HouseLocationIndex == HouseLocationIndex.SharedEstate2).Select(i => (HouseLocation?)i).ToArray();

        /// <summary>
        /// Gets the pointer to the current territory object within the housing manager struct,
        /// or <see cref="IntPtr.Zero"/> when not in a housing area.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the player is currently inside any housing area
        /// (outdoor ward, indoor house, workshop, or apartment).
        /// </summary>
        /// <example>
        /// <code>
        /// if (HousingHelper.IsInHousingArea &amp;&amp; WorldManager.ZoneId == npc.Location.ZoneId)
        /// {
        ///     ward = HousingHelper.HousingPositionInfo.Ward;
        /// }
        /// </code>
        /// </example>
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

        /// <summary>
        /// Gets a value indicating whether the player is inside a house interior
        /// (the current territory matches the indoor territory pointer).
        /// </summary>
        /// <example>
        /// <code>
        /// if (HousingHelper.IsInsideHouse &amp;&amp; HousingHelper.CurrentHouseLocation?.Plot == recordedPlot.Plot)
        /// {
        ///     return true;
        /// }
        /// </code>
        /// </example>
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

        /// <summary>
        /// Gets a value indicating whether the player is inside a Free Company workshop.
        /// </summary>
        public static bool IsInsideWorkshop
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

                return housingManager.Value.WorkshopTerritory != IntPtr.Zero && housingManager.Value.CurrentTerritory == housingManager.Value.WorkshopTerritory;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the player is inside an apartment or FC room.
        /// </summary>
        public static bool IsInsideRoom => HousingPositionInfo.Room != default;

        /// <summary>
        /// Gets a value indicating whether the player is on a specific housing plot
        /// (either outdoors within the plot boundary or inside the house on that plot).
        /// </summary>
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

        /// <summary>
        /// Gets the raw <see cref="HousingManagerStruct"/> read from game memory, or
        /// <see langword="null"/> when the housing manager pointer is zero or unreadable.
        /// </summary>
        public static HousingManagerStruct? HousingManager
        {
            get
            {
                try
                {
                    var pointer = Core.Memory.Read<IntPtr>(HousingHelperOffsets.PositionInfoAddress);
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

        /// <summary>
        /// Forces an immediate refresh of the <see cref="Residences"/> cache from game memory,
        /// unless the cache is already fresh (updated within the last 5 minutes for the same world).
        /// </summary>
        /// <example>
        /// <code>
        /// HousingHelper.UpdateResidenceArray();
        /// </code>
        /// </example>
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
                //ff14bot.Helpers.Logging.WriteDiagnostic("Updating Residence Array");
                //_residences = Core.Memory.ReadArray<ResidenceInfo>(HousingHelperOffsets.HouseLocationArray, 6);
                _residences = ResidentialHousingManager.GetResidences().ToArray();

                //ff14bot.Helpers.Logging.WriteDiagnostic("Residence Array Updated");
            }
            catch (Exception e)
            {
                ff14bot.Helpers.Logging.WriteException(e);
                _residences = new ResidenceInfo[6];
            }
        }

        /// <summary>
        /// Gets the decoded housing position information for the current territory.
        /// Returns a zeroed struct when the position pointer is invalid.
        /// </summary>
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

        /// <summary>
        /// Gets the <see cref="HouseLocation"/> the player is currently inside or on the plot of,
        /// or <see langword="null"/> when not in a housing area or not on a specific plot.
        /// </summary>
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

        /// <summary>
        /// Returns a diagnostic string summarising all housing state flags and position info.
        /// </summary>
        public new static string ToString()
        {
            return $"IsInHousingArea: {IsInHousingArea}, IsInsideHouse: {IsInsideHouse}, IsInsideRoom: {IsInsideRoom}, IsWithinPlot: {IsWithinPlot}, HousingPositionInfo: {HousingPositionInfo.DynamicString()}";
        }
    }

    /// <summary>
    /// Raw struct layout for the game's HousingManager, providing territory pointers for the
    /// player's current housing context.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xE0)]
    public struct HousingManagerStruct
    {
        /// <summary>Pointer to the current territory instance (non-zero when in any housing area).</summary>
        [FieldOffset(0x00)]
        public IntPtr CurrentTerritory;

        /// <summary>Pointer to the outdoor territory instance (the district ward exterior).</summary>
        [FieldOffset(0x08)]
        public IntPtr OutdoorTerritory;

        /// <summary>Pointer to the indoor territory instance (the house interior).</summary>
        [FieldOffset(0x10)]
        public IntPtr IndoorTerritory;

        /// <summary>Pointer to the workshop territory instance (the FC workshop interior).</summary>
        [FieldOffset(0x18)]
        public IntPtr WorkshopTerritory;
    }
}