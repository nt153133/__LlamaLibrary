using System;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentWorldTravelSelect : AgentInterface<AgentWorldTravelSelect>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentWorldTravelSelect;
        

        protected AgentWorldTravelSelect(IntPtr pointer) : base(pointer)
        {
        }

        public int NumberOfWorlds => Core.Memory.NoCacheRead<byte>(Pointer + AgentWorldTravelSelectOffsets.MaxWorldOffset) - 1;
        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + 0x4);
        public IntPtr ChoicesPointer => Core.Memory.NoCacheRead<IntPtr>(Pointer + AgentWorldTravelSelectOffsets.ChoicesOffset);

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