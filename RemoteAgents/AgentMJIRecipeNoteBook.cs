using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIRecipeNoteBook : AgentInterface<AgentMJIRecipeNoteBook>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            //6.4
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B DA 32 D2 48 83 79 ? ? 74 ? 45 85 C9 7E ? 48 8B 44 24 ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 C7 43 ? ? ? ? ? 48 89 03 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B D9 48 8B FA 32 C9 Add 3 TraceRelative")]
            internal static IntPtr VTable;
        }

        protected AgentMJIRecipeNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}