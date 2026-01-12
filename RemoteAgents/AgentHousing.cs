using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentHousing : AgentInterface<AgentHousing>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentHousing;

        

        protected AgentHousing(IntPtr pointer) : base(pointer)
        {
        }
    }
}