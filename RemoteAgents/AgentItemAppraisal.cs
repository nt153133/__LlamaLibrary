using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentItemAppraisal : AgentInterface<AgentItemAppraisal>, IAgent
    {
        public IntPtr RegisteredVtable => AgentItemAppraisalOffsets.VTable;

        

        protected AgentItemAppraisal(IntPtr pointer) : base(pointer)
        {
        }
    }
}