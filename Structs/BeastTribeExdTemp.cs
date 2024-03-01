using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs;
//6.5
#if RB_CN
        [StructLayout(LayoutKind.Explicit, Size = 0x60)]
    public struct BeastTribeExdTemp
    {
        [FieldOffset(0x22)]
        public byte MaxRank;

        [FieldOffset(0x23)]
        public byte Expansion;

        [FieldOffset(0x1C)]
        public ushort Currency;

        public override string ToString()
        {
            return $"MaxRank: {MaxRank} Expansion: {Expansion} Currency: {Currency}"; //Name: {Name}
        }
    }
#else

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
#endif