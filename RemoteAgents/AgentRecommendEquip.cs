using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentRecommendEquip : AgentInterface<AgentRecommendEquip>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentRecommendEquip;
        

        protected AgentRecommendEquip(IntPtr pointer) : base(pointer)
        {
        }
    }
}