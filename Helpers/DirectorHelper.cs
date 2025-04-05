using System;
using System.Runtime.InteropServices;
using System.Security.Policy;
using ff14bot;
using ff14bot.Directors;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;


namespace LlamaLibrary.Helpers
{
#if RB_DT
    public class DirectorHelper
    {
        internal static class Offsets
        {
            [OffsetDawntrail("Search E8 ? ? ? ? 89 6C 24 38 44 8B F5 TraceCall")]
            public static IntPtr GetTodoArgs;

            [OffsetDawntrail("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 89 6C 24 38 Add 3 TraceRelative")]
            public static IntPtr ActiveDirector;
        }

        private static IntPtr CurrentDirector;
        private static IntPtr CurrentDirectorTodoArgs;

        public static IntPtr ActiveDirectorAddress => Core.Memory.Read<IntPtr>(Offsets.ActiveDirector);

        public static TodoArgs[] GetTodoArgs()
        {
            if (ActiveDirectorAddress == IntPtr.Zero)
            {
                return Array.Empty<TodoArgs>();
            }

            if (ActiveDirectorAddress != CurrentDirector)
            {
                CurrentDirector = ActiveDirectorAddress;
                CurrentDirectorTodoArgs = Core.Memory.CallInjectedWraper<IntPtr>(Offsets.GetTodoArgs, Offsets.ActiveDirector);
            }

            if (CurrentDirector == IntPtr.Zero)
            {
                return Array.Empty<TodoArgs>();
            }

            var twovec = Core.Memory.Read<NativeVectorV2<TodoArgs>>(CurrentDirectorTodoArgs);
            return Core.Memory.ReadArray<TodoArgs>(twovec.First, twovec.Count);
        }

        public static bool IsTodoChecked(uint pos)
        {
            var args = GetTodoArgs();

            if (pos > args.Length) {
                return false;
            }

            var arg = args[pos];

            if (arg.DisplayType == 0)
            {
                return false;
            }

            return arg.CountCurrent == arg.CountNeeded;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x160)]
        public struct TodoArgs
        {
            [FieldOffset(0x00)] public bool Enabled;
            [FieldOffset(0x04)] public int DisplayType;

            [FieldOffset(0x78)] public int CountCurrent;
            [FieldOffset(0x7C)] public int CountNeeded;
            [FieldOffset(0x80)] public ulong TimeLeft;
            [FieldOffset(0x88)] public uint MapRowId;
        }
    }

#endif
}