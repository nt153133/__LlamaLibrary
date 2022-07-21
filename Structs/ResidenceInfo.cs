using System;
using System.Runtime.InteropServices;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.JsonObjects;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResidenceInfo
    {
        private const ushort RoomMask = 0b0000_1111_1100_0000;
        private const ushort WardMask = 0b0000_0000_0011_1111;
        private const ushort MaskSize = 6;

        /// <summary>
        /// The zero-indexed plot number that the player is in.
        ///
        /// <para>
        /// Contains apartment subdivision/non-subdivsion for an apartment.
        /// </para>
        /// </summary>
        private readonly ushort InternalPlot;

        /// <summary>
        /// The zero-indexed ward number that the player is in.
        ///
        /// <para>
        /// Contains apartment room # for an apartment building.
        /// Contains room number # FC rooms
        /// </para>
        /// </summary>
        private readonly ushort InternalWard;

        public readonly InternalHousingZone Zone;

        //0-3 so far mostly 2-3. Sometimes lines up with plot size? only hit 3 on FC estate in Empyreum. Like 90% of the time it's the size of the plot but
        public readonly byte SomeByte;

        public readonly World World;

        //always 1 so far
        private readonly byte unknownByte1;

        //always 1 so far
        private readonly byte unknownBytes2;

        private readonly byte unknownByte3;

        //1 when it's a FC house but not FC Rooms
        public readonly bool FCOwned;
        private readonly byte unknownByte5;
        private readonly byte unknownByte6;
        private readonly byte unknownByte7;
        private readonly byte unknownByte8;
        private readonly IntPtr SomePointer;

        public int Plot
        {
            get
            {
                if (IsApartment)
                {
                    return (ushort)((InternalPlot & ~0x80) + 1);
                }

                return InternalPlot + 1;
            }
        }

        public int Ward
        {
            get
            {
                if (IsApartment)
                {
                    return WardTemp;
                }

                if (IsFcRoom)
                {
                    return ((InternalWard & WardMask) >> MaskSize) + 1;
                }

                return InternalWard + 1;
            }
        }

        public int Room => (InternalWard & RoomMask) >> MaskSize;
        public bool IsApartment => (InternalPlot & 0x80) > 0;

        public bool IsFcRoom => InternalWard > 30 && !IsApartment;
        private int WardTemp => (ushort)((InternalWard & 0x3F) + 1); //((InternalWard & wardMask) >> maskSize) + 1;

        public static implicit operator HouseLocation(ResidenceInfo info)
        {
            if (info.Zone == (InternalHousingZone)255 || info.IsApartment || info.IsFcRoom)
            {
                return null;
            }

            return new HouseLocation(info.World, (HousingZone)Enum.Parse(typeof(HousingZone), info.Zone.ToString()), info.Ward, info.Plot);
        }

        public string UnknownBytes => $"{unknownByte1}, {unknownBytes2}, {unknownByte3}, {unknownByte5}, {unknownByte6}, {unknownByte7}, {unknownByte8}";

        public override string ToString()
        {
            return $" World: {World}, Ward: {Ward}, Plot: {Plot}, Zone: {Zone}, SomeByte: {SomeByte},{((IsApartment || IsFcRoom) ? $" Room: {Room}," : "")} FC Owned: {FCOwned}, UnknownBytes: {UnknownBytes}";
        }
    }
}