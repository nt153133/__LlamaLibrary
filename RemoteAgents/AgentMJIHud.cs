using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIHud : AgentInterface<AgentMJIHud>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B DA 32 D2 48 83 79 ? ? 74 ? 45 85 C9 7E ? 8B 44 24 ? 83 E8 ? 74 ? 83 E8 ? 74 ? 83 F8 ? 75 ? 49 8B D0 E8 ? ? ? ? EB ? 49 8B D0 E8 ? ? ? ? EB ? 49 8B D0 E8 ? ? ? ? 0F B6 D0 C7 03 ? ? ? ? 48 8B C3 88 53 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B D9 Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 C7 43 ? ? ? ? ? 48 89 03 48 8B C3 66 C7 43 ? ? ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 48 89 7C 24 ? 41 56 48 83 EC ? 48 8B D9 Add 3 TraceRelative")]
            internal static IntPtr VTable;

            [Offset("Search 48 8B 4E ? F6 81 ? ? ? ? ? 74 ? 0F BA EF ? Add 3 Read8")]
            [OffsetDawntrail("Search 49 8B 4E ? 0F B6 81 ? ? ? ? 24 ? Add 3 Read8")]
            internal static int InfoPtr;

            [Offset("Search 89 81 ? ? ? ? 48 8B 46 ? 8B 88 ? ? ? ? Add 2 Read32")]
            [OffsetDawntrail("Search 89 81 ? ? ? ? 49 8B 46 ? 8B 88 ? ? ? ? Add 2 Read32")]
            internal static int CurrentExp;
        }

        protected AgentMJIHud(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr InfoPtr => Core.Memory.Read<IntPtr>(Pointer + Offsets.InfoPtr);
        public uint CurrentExp => Core.Memory.Read<uint>(InfoPtr + Offsets.CurrentExp);
    }
}