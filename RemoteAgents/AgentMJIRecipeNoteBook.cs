using System;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIRecipeNoteBook : AgentInterface<AgentMJIRecipeNoteBook>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentMJIRecipeNoteBook;

        

        protected AgentMJIRecipeNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}