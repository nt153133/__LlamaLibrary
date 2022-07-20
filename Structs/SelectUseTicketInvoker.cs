using System;
using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x28)]
    public struct SelectUseTicketInvoker
    {
        [FieldOffset(0x00)]
        public IntPtr vtbl;

        [FieldOffset(0x10)]
        public IntPtr Telepo;
    }
}