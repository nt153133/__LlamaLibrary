using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs;
//6.5
[StructLayout(LayoutKind.Explicit, Size = 0x60)]
public struct BeastTribeExdTemp
{
    [FieldOffset(0x26)]
    public byte MaxRank;

    [FieldOffset(0x27)]
    public byte Expansion;

    [FieldOffset(0x20)]
    public ushort Currency;

    public override string ToString()
    {
        return $"MaxRank: {MaxRank} Expansion: {Expansion} Currency: {Currency}"; //Name: {Name}
    }
}