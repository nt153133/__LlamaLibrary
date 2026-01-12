using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIPouch : AgentInterface<AgentMJIPouch>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentMJIPouch;

        

        protected AgentMJIPouch(IntPtr pointer) : base(pointer)
        {
        }
    }
}