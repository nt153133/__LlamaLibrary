using System;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentWorldTravelSelect : AgentInterface<AgentWorldTravelSelect>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 48 8D 73 ? 48 89 6B ? 48 89 6B ? 8D 7D ? 48 89 6B ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
            [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 0F B6 50 ? 84 D2 TraceCall")]
            internal static IntPtr ExdData__getWorld;
            //6.4
            [Offset("Search 48 8B 46 ? 0F B7 1C B8 Add 3 Read8")]
            internal static int ChoicesOffset;
            //6.4
            [Offset("Search 66 89 5E ? E8 ? ? ? ? 0F B7 D0 Add 3 Read8")]
            internal static int CurrentWorldOffset;
            //6.4
            [Offset("Search 3B 46 ? 0F 8F ? ? ? ? Add 2 Read8")]
            internal static int MaxWorldOffset;
        }

        protected AgentWorldTravelSelect(IntPtr pointer) : base(pointer)
        {
        }

        public int NumberOfWorlds => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.MaxWorldOffset) - 1;
        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + 0x4);
        public IntPtr ChoicesPointer => Core.Memory.NoCacheRead<IntPtr>(Pointer + Offsets.ChoicesOffset);

        public ushort HomeWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer);

        public WorldChoice[] Choices => Core.Memory.ReadArray<WorldChoice>(ChoicesPointer + 0x8, NumberOfWorlds);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WorldChoice
    {
        public ushort WorldID;
        public ushort Unk;
    }
}