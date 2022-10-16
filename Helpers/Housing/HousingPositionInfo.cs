using System;
using ff14bot;

namespace LlamaLibrary.Helpers.Housing
{
    public readonly struct HousingPositionInfo
    {
        private const ushort RoomMask = 0b0000_1111_1100_0000;
        private const ushort WardMask = 0b0000_0000_0011_1111;
        private const ushort MaskSize = 6;

        private readonly IntPtr _address;

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

            InHouse = Core.Memory.Read<byte>(_address + 0x96A9) == 0;
            House = (ushort)(!InHouse ? 0 : Core.Memory.Read<ushort>(_address + 0x96A0) + 1);
            var internalWard = (ushort)(Core.Memory.Read<ushort>(_address + 0x96A2) + 1);
            var internalPlot = (byte)(InHouse ? 0 : Core.Memory.Read<byte>(_address + 0x96A8) + 1);
            Room = (internalWard & RoomMask) >> MaskSize;
            var wardTemp = ((internalWard & WardMask) >> MaskSize) + 1;
            Ward = (internalWard < 50) ? internalWard : wardTemp;
            Subdivision = Core.Memory.Read<byte>(_address + 0x96A9) == 2;
            Zone = Core.Memory.Read<InternalHousingZone>(_address + 0x96A4);
            Plot = InHouse ? House : internalPlot;
            HousingFloor = Core.Memory.Read<HousingFloor>(_address + 0x9704);
        }

        public static implicit operator bool(HousingPositionInfo ptr)
        {
            return ptr._address != IntPtr.Zero;
        }

        public ushort House { get; }

        public int Ward { get; }

        public int Room { get; }

        public bool Subdivision { get; }

        public InternalHousingZone Zone { get; }

        public ushort Plot { get; }

        public HousingFloor HousingFloor { get; }

        public bool InHouse { get; }
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