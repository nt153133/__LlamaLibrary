using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x8)]
    public struct CycleTime
    {
        [FieldOffset(0)]
        public uint FirstCycle;

        [FieldOffset(0x4)]
        public uint Cycle;
    }
}