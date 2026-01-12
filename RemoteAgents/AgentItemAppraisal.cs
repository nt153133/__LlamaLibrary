using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentItemAppraisal : AgentInterface<AgentItemAppraisal>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentItemAppraisal;

        

        protected AgentItemAppraisal(IntPtr pointer) : base(pointer)
        {
        }
    }
}