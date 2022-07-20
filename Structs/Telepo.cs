using System;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x58)]
    public struct Telepo
    {
        [FieldOffset(0x00)]
        public IntPtr vtbl;

        [FieldOffset(0x10)]
        public NativeVectorV3<TeleportInfo> TeleportList;

        [FieldOffset(0x28)]
        public SelectUseTicketInvoker UseTicketInvoker;

        public TeleportInfo[] TeleportInfos => Core.Memory.ReadNativeVector(TeleportList);
    }
}