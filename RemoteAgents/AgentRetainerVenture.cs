using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has hardcoded memory offsets
    public class AgentRetainerVenture : AgentInterface<AgentRetainerVenture>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentRetainerVenture;
        

        protected AgentRetainerVenture(IntPtr pointer) : base(pointer)
        {
        }

        public int ExperiencedGain => Core.Memory.Read<int>(Pointer + 0x3c);
        public int RewardItem1 => Core.Memory.Read<int>(Pointer + 0x48);
        public int RewardItem2 => Core.Memory.Read<int>(Pointer + 0x4C);
        public int RewardCount1 => Core.Memory.Read<int>(Pointer + 0x50);
        public int RewardCount2 => Core.Memory.Read<int>(Pointer + 0x54);
        public int RetainerTask => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.RetainerTask);
    }
}