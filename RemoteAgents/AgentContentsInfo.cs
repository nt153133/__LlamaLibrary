using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentContentsInfo : AgentInterface<AgentContentsInfo>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentContentsInfo;
        

        protected AgentContentsInfo(IntPtr pointer) : base(pointer)
        {
        }
    }
}