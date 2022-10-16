using System.Linq;
using System.Runtime.InteropServices;
using ff14bot.Managers;
using LlamaLibrary.Helpers.HousingTravel;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public struct TeleportInfo
    {
        private static readonly uint[] PrivateHousing = new uint[] { 59, 60, 61, 97, 165 };
        private static readonly uint[] FcHousing = new uint[] { 56, 57, 58, 96, 164 };

        [FieldOffset(0x00)]
        public uint AetheryteId;

        [FieldOffset(0x04)]
        public uint GilCost;

        [FieldOffset(0x08)]
        public ushort TerritoryId;

        [FieldOffset(0x18)]
        public byte Ward;

        [FieldOffset(0x19)]
        public byte Plot;

        [FieldOffset(0x1A)]
        public byte SubIndex;

        [FieldOffset(0x1B)]
        public byte IsFavourite;

        public bool IsSharedHouse => Ward > 0 && Plot > 0;
        public bool IsApartment => SubIndex == 128 && !IsSharedHouse;
        public string ZoneName => DataManager.ZoneNameResults[TerritoryId].CurrentLocaleName;
        public bool IsResidential => HousingTraveler.HousingZoneIds.Contains(TerritoryId);

        public bool IsPrivateHouse => PrivateHousing.Contains(AetheryteId);

        public bool IsFCHouse => FcHousing.Contains(AetheryteId);

        public bool IsOwnHouse => IsPrivateHouse && !IsSharedHouse;

        public override string ToString()
        {
            return $"AetheryteId: {AetheryteId}, GilCost: {GilCost}, Ward: {Ward}, Plot: {Plot}, SubIndex: {SubIndex}, IsSharedHouse: {IsSharedHouse}, IsApartment: {IsApartment}, ZoneName: {ZoneName}, IsResidential: {IsResidential}, IsPrivateHouse: {IsPrivateHouse}, IsFCHouse: {IsFCHouse}, IsOwnHouse: {IsOwnHouse}";
        }
    }
}