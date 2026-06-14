using System;
using ff14bot;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers.Housing
{
    /// <summary>
    /// Read-only struct that decodes the player's current housing position from a game memory pointer.
    /// </summary>
    /// <remarks>
    /// Construct via <see cref="HousingHelper.HousingPositionInfo"/>. When the underlying pointer is
    /// <see cref="IntPtr.Zero"/> all members return their default/zero values.
    /// </remarks>
    public readonly struct HousingPositionInfo
    {
        private const ushort RoomMask = 0b1111_1111_1100_0000; // bits 6-15: full 10-bit room number (apartments up to 90)
        private const ushort WardMask = 0b0000_0000_0011_1111;
        private const ushort MaskSize = 6;

        private readonly IntPtr _address;

        /// <summary>
        /// Initialises a new <see cref="HousingPositionInfo"/> by reading housing state from
        /// the given game memory pointer.
        /// </summary>
        /// <param name="ptr">
        /// The pointer to the housing territory data.  Pass <see cref="IntPtr.Zero"/> to
        /// produce a zeroed (invalid) instance.
        /// </param>
        public HousingPositionInfo(IntPtr ptr)
        {
            _address = ptr;

            if (ptr == IntPtr.Zero)
            {
                InHouse = false;
                House = 0;
                Room = 0;
                Ward = 0;
                Subdivision = false;
                Zone = InternalHousingZone.Unknown;
                Plot = 0;
                HousingFloor = HousingFloor.Unknown;
                return;
            }

            try
            {
                var houseId = _address + Offsets.IndoorTerritoryHouseId;


                InHouse = Core.Memory.Read<byte>(houseId + 0x9) == 0;
                House = (ushort)(!InHouse ? 0 : Core.Memory.Read<ushort>(houseId) + 1);
                var internalWard = (ushort)(Core.Memory.Read<ushort>(houseId + 0x2) + 1);
                var internalPlot = (byte)(InHouse ? 0 : Core.Memory.Read<byte>(houseId + 0x8) + 1);
                Room = (internalWard & RoomMask) >> MaskSize;
                var wardTemp = ((internalWard & WardMask) >> MaskSize) + 1;
                Ward = (internalWard < 50) ? internalWard : wardTemp;
                Subdivision = Core.Memory.Read<byte>(houseId + 0x9) == 2;
                Zone = Core.Memory.Read<InternalHousingZone>(houseId + 0x4);
                Plot = InHouse ? House : internalPlot;
                HousingFloor = Core.Memory.Read<HousingFloor>(houseId + 0x4);
            }
            catch (Exception e)
            {
                ff14bot.Helpers.Logging.WriteException(e);
                InHouse = false;
                House = 0;
                Room = 0;
                Ward = 0;
                Subdivision = false;
                Zone = InternalHousingZone.Unknown;
                Plot = 0;
                HousingFloor = HousingFloor.Unknown;
            }
        }

        /// <summary>
        /// Implicitly converts a <see cref="HousingPositionInfo"/> to <see langword="bool"/>,
        /// returning <see langword="true"/> when the underlying memory pointer is valid (non-zero).
        /// </summary>
        public static implicit operator bool(HousingPositionInfo ptr)
        {
            return ptr._address != IntPtr.Zero;
        }

        /// <summary>Gets the 1-based house number within the current plot (interior only).</summary>
        public ushort House { get; }

        /// <summary>Gets the 1-based ward number the player is currently in.</summary>
        public int Ward { get; }

        /// <summary>Gets the apartment or FC room number, or 0 when not in a room.</summary>
        public int Room { get; }

        /// <summary>
        /// Gets a value indicating whether the player is in the subdivision rather than the main ward.
        /// </summary>
        public bool Subdivision { get; }

        /// <summary>Gets the internal housing zone identifier as read from game memory.</summary>
        public InternalHousingZone Zone { get; }

        /// <summary>Gets the 1-based plot number the player is currently on or inside.</summary>
        public ushort Plot { get; }

        /// <summary>Gets the floor the player is on within a house interior.</summary>
        public HousingFloor HousingFloor { get; }

        /// <summary>
        /// Gets a value indicating whether the player is inside a house interior (as opposed to
        /// the outdoor plot area).
        /// </summary>
        public bool InHouse { get; }
    }

    /// <summary>Represents the floor within a house interior.</summary>
    public enum HousingFloor : byte
    {
        /// <summary>Floor is unknown or not determinable.</summary>
        Unknown = 0xFF,
        /// <summary>Ground floor.</summary>
        Ground = 0,
        /// <summary>First upper floor.</summary>
        First = 1,
        /// <summary>Cellar / basement.</summary>
        Cellar = 0x0A,
    }

    /// <summary>
    /// Zone ID enumeration for all FFXIV residential-housing territories, including outdoor wards,
    /// house interiors (cottage/house/mansion), chamber and apartment variants.
    /// </summary>
    public enum HousingZone : ushort
    {
        /// <summary>Mist outdoor ward (Limsa Lominsa district).</summary>
        Mist = 339,
        /// <summary>Lavender Beds outdoor ward (Gridania district).</summary>
        LavenderBeds = 340,
        /// <summary>The Goblet outdoor ward (Ul'dah district).</summary>
        Goblet = 341,
        /// <summary>Shirogane outdoor ward (Kugane district).</summary>
        Shirogane = 641,
        /// <summary>Empyreum outdoor ward (Ishgard district).</summary>
        Empyreum = 979,
        /// <summary>Chambers interior (Mist).</summary>
        ChambersMist = 384,
        /// <summary>Chambers interior (Lavender Beds).</summary>
        ChambersLavenderBeds = 385,
        /// <summary>Chambers interior (The Goblet).</summary>
        ChambersGoblet = 386,
        /// <summary>Chambers interior (Shirogane).</summary>
        ChambersShirogane = 652,
        /// <summary>Chambers interior (Empyreum).</summary>
        ChambersEmpyreum = 983,
        /// <summary>Apartment building interior (Mist).</summary>
        ApartmentMist = 608,
        /// <summary>Apartment building interior (Lavender Beds).</summary>
        ApartmentLavenderBeds = 609,
        /// <summary>Apartment building interior (The Goblet).</summary>
        ApartmentGoblet = 610,
        /// <summary>Apartment building interior (Shirogane).</summary>
        ApartmentShirogane = 655,
        /// <summary>Apartment building interior (Empyreum).</summary>
        ApartmentEmpyreum = 999,
        /// <summary>Cottage interior (Mist).</summary>
        CottageMist = 282,
        /// <summary>Cottage interior (Lavender Beds).</summary>
        CottageLavenderBeds = 342,
        /// <summary>Cottage interior (The Goblet).</summary>
        CottageGoblet = 345,
        /// <summary>Cottage interior (Shirogane).</summary>
        CottageShirogane = 649,
        /// <summary>Cottage interior (Empyreum).</summary>
        CottageEmpyreum = 980,
        /// <summary>House interior (Mist).</summary>
        HouseMist = 283,
        /// <summary>House interior (Lavender Beds).</summary>
        HouseLavenderBeds = 343,
        /// <summary>House interior (The Goblet).</summary>
        HouseGoblet = 346,
        /// <summary>House interior (Shirogane).</summary>
        HouseShirogane = 650,
        /// <summary>House interior (Empyreum).</summary>
        HouseEmpyreum = 981,
        /// <summary>Mansion interior (Mist).</summary>
        MansionMist = 284,
        /// <summary>Mansion interior (Lavender Beds).</summary>
        MansionLavenderBeds = 344,
        /// <summary>Mansion interior (The Goblet).</summary>
        MansionGoblet = 347,
        /// <summary>Mansion interior (Shirogane).</summary>
        MansionShirogane = 651,
        /// <summary>Mansion interior (Empyreum).</summary>
        MansionEmpyreum = 982,
    }

    /// <summary>
    /// Internal single-byte zone identifiers used within housing memory structures.
    /// </summary>
    public enum InternalHousingZone : byte
    {
        /// <summary>Zone is not a recognised housing area.</summary>
        Unknown = 0,
        /// <summary>Mist.</summary>
        Mist = 83,
        /// <summary>The Goblet.</summary>
        Goblet = 85,
        /// <summary>Lavender Beds.</summary>
        LavenderBeds = 84,
        /// <summary>Shirogane.</summary>
        Shirogane = 129,
        /// <summary>Empyreum.</summary>
        Empyreum = 211,
    }
}