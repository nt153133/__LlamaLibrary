using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentItemAppraisal : AgentInterface<AgentItemAppraisal>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentItemAppraisal;

        

        protected AgentItemAppraisal(IntPtr pointer) : base(pointer)
        {
        }
    }
}