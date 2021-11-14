using System.Runtime.InteropServices;

namespace LlamaLibrary.Helpers
{
    [StructLayout(LayoutKind.Explicit, Size = 0x60)]
    public struct BeastTribeExd
    {
        [FieldOffset(0x22)]
        public byte MaxRank;

        [FieldOffset(0x23)]
        public byte Expansion;

        [FieldOffset(0x1C)]
        public ushort Currency;

        [FieldOffset(0x28)]
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string Name;

        public override string ToString()
        {
            return $"MaxRank: {MaxRank} Expansion: {Expansion} Currency: {Currency} Name: {Name}"; //Name: {Name}
        }
    }
}