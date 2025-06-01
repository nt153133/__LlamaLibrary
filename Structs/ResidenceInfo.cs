using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.JsonObjects;

namespace LlamaLibrary.Structs;

public class ResidenceInfo
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
    private readonly ushort _internalPlot;

    /// <summary>
    /// The zero-indexed ward number that the player is in.
    ///
    /// <para>
    /// Contains apartment room # for an apartment building.
    /// Contains room number # FC rooms.
    /// </para>
    /// </summary>
    private readonly ushort _internalWard;

    public readonly ushort Zone;

    public readonly World World;

    public HouseLocationIndex HouseLocationIndex { get; }

    public ResidenceInfo(long residentObject, HouseLocationIndex houseLocationIndex)
    {
        var (plot, ward, zone, world, _) = GetResidentObjectInfo(residentObject);
        _internalPlot = plot;
        _internalWard = ward;
        Zone = zone;
        World = (World)world;
        HouseLocationIndex = houseLocationIndex;
    }

    public int Plot
    {
        get
        {
            if (IsApartment)
            {
                return (ushort)((_internalPlot & ~0x80) + 1);
            }

            return _internalPlot + 1;
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
                return ((_internalWard & WardMask) >> MaskSize) + 1;
            }

            return _internalWard + 1;
        }
    }

    public int Room => (_internalWard & RoomMask) >> MaskSize;
    public bool IsApartment => (_internalPlot & 0x80) > 0;

    public bool IsFcRoom => _internalWard > 30 && !IsApartment;
    private int WardTemp => (ushort)((_internalWard & 0x3F) + 1); //((InternalWard & wardMask) >> maskSize) + 1;

    public static implicit operator HouseLocation?(ResidenceInfo? info)
    {
        if (info == null)
        {
            return null;
        }

        if (info.Zone == 255 || info.IsApartment || info.IsFcRoom)
        {
            return null;
        }

        return new HouseLocation((HousingZone)info.Zone, info.Ward, info.Plot);
    }

    public override string ToString()
    {
        return $"{HouseLocationIndex}[{(int)HouseLocationIndex}] Ward: {Ward}, Plot: {Plot}, Zone: {DataManager.ZoneNameResults[Zone]}, {((IsApartment || IsFcRoom) ? $" Room: {Room}," : "")} World: {World}";
    }

    public static (ushort plot, ushort ward, ushort zone, byte world, byte unknown) GetResidentObjectInfo(long residentObject)
    {
        var plot = (ushort)(residentObject & 0xFFFF);
        var ward = (ushort)((residentObject >> 16) & 0xFFFF);
        var zone = (ushort)((residentObject >> 32) & 0xFFFF);
        var world = (byte)((residentObject >> 48) & 0xFF);
        var unknown = (byte)((residentObject >> 56) & 0xFF);
        return (plot, ward, zone, world, unknown);
    }
}