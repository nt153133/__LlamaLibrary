using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentCharacter : AgentInterface<AgentCharacter>, IAgent
    {
        

        protected AgentCharacter(IntPtr pointer) : base(pointer)
        {
        }

        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentCharacter;
    }
}