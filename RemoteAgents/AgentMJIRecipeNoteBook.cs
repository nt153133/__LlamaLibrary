using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIRecipeNoteBook : AgentInterface<AgentMJIRecipeNoteBook>, IAgent
    {
        public IntPtr RegisteredVtable => AgentMJIRecipeNoteBookOffsets.VTable;

        

        protected AgentMJIRecipeNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}