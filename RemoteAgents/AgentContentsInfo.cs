using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentContentsInfo : AgentInterface<AgentContentsInfo>, IAgent
    {
        public IntPtr RegisteredVtable => AgentContentsInfoOffsets.VTable;
        

        protected AgentContentsInfo(IntPtr pointer) : base(pointer)
        {
        }
    }
}