using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x60)]
public struct BeastTribeExdTemp
{
#if RB_TC
    [FieldOffset(0x26)]
#else
    [FieldOffset(0x2A)]
#endif
    public byte MaxRank;

#if RB_TC
    [FieldOffset(0x27)]
#else
    [FieldOffset(0x2B)]
#endif
    public byte Expansion;

#if RB_TC
    [FieldOffset(0x20)]
    public ushort Currency;
#else
    [FieldOffset(0x24)]
    public uint Currency;
#endif

    public override string ToString()
    {
        return $"MaxRank: {MaxRank} Expansion: {Expansion} Currency: {Currency}"; //Name: {Name}
    }
}
