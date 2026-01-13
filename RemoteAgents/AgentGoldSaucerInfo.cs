using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentGoldSaucerInfo : AgentInterface<AgentGoldSaucerInfo>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentGoldSaucerInfo;
        

        protected AgentGoldSaucerInfo(IntPtr pointer) : base(pointer)
        {
        }
    }
}