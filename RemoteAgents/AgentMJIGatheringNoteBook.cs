using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIGatheringNoteBook : AgentInterface<AgentMJIGatheringNoteBook>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentMJIGatheringNoteBook;

        

        protected AgentMJIGatheringNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}