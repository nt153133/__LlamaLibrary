using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentJournalDetail : AgentInterface<AgentJournalDetail>, IAgent
    {
        public IntPtr RegisteredVtable => AgentJournalDetailOffsets.VTable;

        

        protected AgentJournalDetail(IntPtr pointer) : base(pointer)
        {
        }
    }
}