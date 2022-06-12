using System;
using ff14bot;

namespace LlamaLibrary.Helpers.Housing
{
    public readonly struct HousingPositionInfo
    {
        const ushort roomMask = 0b0000_1111_1100_0000;
        const ushort wardMask = 0b0000_0000_0011_1111;
        const ushort maskSize = 6;

        private readonly IntPtr _address;

        public HousingPositionInfo(IntPtr ptr)
            => _address = ptr;

        public static implicit operator bool(HousingPositionInfo ptr)
            => ptr._address != null;

        public ushort House => (ushort)(_address == IntPtr.Zero || !InHouse ? 0 : Core.Memory.Read<ushort>(_address + 0x96A0) + 1);

        private ushort InternalWard => (ushort)(_address == IntPtr.Zero ? 0 : Core.Memory.Read<ushort>(_address + 0x96A2) + 1);

        public int Ward => (InternalWard < 50) ? InternalWard : wardTemp;

        public int Room => roomTemp;

        public bool Subdivision => _address != IntPtr.Zero && Core.Memory.Read<byte>(_address + 0x96A9) == 2;

        public InternalHousingZone Zone => _address == IntPtr.Zero ? InternalHousingZone.Unknown : Core.Memory.Read<InternalHousingZone>(_address + 0x96A4);

        public ushort Plot
        {
            get
            {
                if (_address == IntPtr.Zero)
                {
                    return 0;
                }

                if (InHouse)
                {
                    return House;
                }

                return InternalPlot;
            }
        }

        private byte InternalPlot => (byte)(_address == IntPtr.Zero || InHouse ? 0 : Core.Memory.Read<byte>(_address + 0x96A8) + 1);

        public HousingFloor HousingFloor => _address == IntPtr.Zero ? HousingFloor.Unknown : Core.Memory.Read<HousingFloor>(_address + 0x9704);

        public bool InHouse => Core.Memory.Read<byte>(_address + 0x96A9) == 0;

        private int roomTemp => (InternalWard & roomMask) >> maskSize;

        private int wardTemp => ((InternalWard & wardMask) >> maskSize) + 1;
    }

    public enum HousingFloor : byte
    {
        Unknown = 0xFF,
        Ground = 0,
        First = 1,
        Cellar = 0x0A,
    }

    public enum HousingZone : ushort
    {
        Mist = 339,
        LavenderBeds = 340,
        Goblet = 341,
        Shirogane = 641,
        Empyreum = 979,
        ChambersMist = 384,
        ChambersLavenderBeds = 385,
        ChambersGoblet = 386,
        ChambersShirogane = 652,
        ChambersEmpyreum = 983,
        ApartmentMist = 608,
        ApartmentLavenderBeds = 609,
        ApartmentGoblet = 610,
        ApartmentShirogane = 655,
        ApartmentEmpyreum = 999,
        CottageMist = 282,
        CottageLavenderBeds = 342,
        CottageGoblet = 345,
        CottageShirogane = 649,
        CottageEmpyreum = 980,
        HouseMist = 283,
        HouseLavenderBeds = 343,
        HouseGoblet = 346,
        HouseShirogane = 650,
        HouseEmpyreum = 981,
        MansionMist = 284,
        MansionLavenderBeds = 344,
        MansionGoblet = 347,
        MansionShirogane = 651,
        MansionEmpyreum = 982,
    }

    public enum InternalHousingZone : byte
    {
        Unknown = 0,
        Mist = 83,
        Goblet = 85,
        LavenderBeds = 84,
        Shirogane = 129,
        Empyreum = 211,
    }
}