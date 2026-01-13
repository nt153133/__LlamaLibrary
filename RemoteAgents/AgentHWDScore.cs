using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has hardcoded memory offsets
    public class AgentHWDScore : AgentInterface<AgentHWDScore>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentHWDScore;

        

        protected AgentHWDScore(IntPtr pointer) : base(pointer)
        {
        }

        public int[] ReadTotalScores()
        {
            return Core.Memory.ReadArray<int>(Pointer + 0x90, 11);
        }
    }
}