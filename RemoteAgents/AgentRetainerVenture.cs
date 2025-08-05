using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has hardcoded memory offsets
    public class AgentRetainerVenture : AgentInterface<AgentRetainerVenture>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 B9 ? ? ? ? 89 53 ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 66 89 43 ? 48 89 43 ? 88 43 ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
            //7.3
            [Offset("Search 48 8B 4E ? 4C 8B E0 48 8B 01 Add 3 Read8")]
            [OffsetCN("Search 48 8B 49 ? 48 8B 01 FF 50 ? BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 ? 48 8B 4D ? 4C 8B E8 48 8B 01 Add 3 Read8")]
            internal static int RetainerTask;
        }

        protected AgentRetainerVenture(IntPtr pointer) : base(pointer)
        {
        }

        public int ExperiencedGain => Core.Memory.Read<int>(Pointer + 0x3c);
        public int RewardItem1 => Core.Memory.Read<int>(Pointer + 0x48);
        public int RewardItem2 => Core.Memory.Read<int>(Pointer + 0x4C);
        public int RewardCount1 => Core.Memory.Read<int>(Pointer + 0x50);
        public int RewardCount2 => Core.Memory.Read<int>(Pointer + 0x54);
        public int RetainerTask => Core.Memory.Read<int>(Pointer + Offsets.RetainerTask);
    }
}