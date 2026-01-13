using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentInventoryBuddy : AgentInterface<AgentInventoryBuddy>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentInventoryBuddy;

        

        protected AgentInventoryBuddy(IntPtr pointer) : base(pointer)
        {
        }
    }
}