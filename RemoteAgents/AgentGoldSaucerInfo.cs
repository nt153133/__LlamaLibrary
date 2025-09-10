using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentGoldSaucerInfo : AgentInterface<AgentGoldSaucerInfo>, IAgent
    {
        public IntPtr RegisteredVtable => AgentGoldSaucerInfoOffsets.VTable;
        

        protected AgentGoldSaucerInfo(IntPtr pointer) : base(pointer)
        {
        }
    }
}