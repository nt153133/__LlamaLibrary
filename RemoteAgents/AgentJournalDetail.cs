using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentJournalDetail : AgentInterface<AgentJournalDetail>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentJournalDetail;

        

        protected AgentJournalDetail(IntPtr pointer) : base(pointer)
        {
        }
    }
}