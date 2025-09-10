using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIGatheringNoteBook : AgentInterface<AgentMJIGatheringNoteBook>, IAgent
    {
        public IntPtr RegisteredVtable => AgentMJIGatheringNoteBookOffsets.VTable;

        

        protected AgentMJIGatheringNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}