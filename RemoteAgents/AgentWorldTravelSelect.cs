using System;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentWorldTravelSelect : AgentInterface<AgentWorldTravelSelect>, IAgent
    {
        public IntPtr RegisteredVtable => AgentWorldTravelSelectOffsets.VTable;

        private const int MaxCountOffset = -1;

#if RB_TC
        private const int MaxSkip = 0;
#else
        private const int MaxSkip = 1;
#endif

        protected AgentWorldTravelSelect(IntPtr pointer) : base(pointer)
        {
        }

        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + AgentWorldTravelSelectOffsets.CurrentWorldOffset);
        public IntPtr ChoicesPointer => Core.Memory.NoCacheRead<IntPtr>(Pointer + AgentWorldTravelSelectOffsets.ChoicesOffset);

        public ushort HomeWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer);

        public int NumberOfWorlds => Core.Memory.NoCacheRead<byte>(Pointer + AgentWorldTravelSelectOffsets.MaxWorldOffset) + MaxCountOffset;

        public WorldChoice[] Choices => Core.Memory.ReadArray<WorldChoice>(ChoicesPointer + 0x8, NumberOfWorlds).Skip(MaxSkip).ToArray();
    }
#if RB_TC
    [StructLayout(LayoutKind.Sequential)]
    public struct WorldChoice
    {
        public ushort WorldID;
        public ushort Unk;
    }
#else
    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct WorldChoice
    {
        [FieldOffset(0x4)]
        public ushort WorldID;

        public World World => (World)WorldID;
    }
#endif
}
