using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIPouch : AgentInterface<AgentMJIPouch>, IAgent
    {
        public IntPtr RegisteredVtable => AgentMJIPouchOffsets.VTable;

        

        protected AgentMJIPouch(IntPtr pointer) : base(pointer)
        {
        }
    }
}