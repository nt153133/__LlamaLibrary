using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIGatheringNoteBook : AgentInterface<AgentMJIGatheringNoteBook>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            //6.4
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 32 C0 48 8B DA 48 83 79 ? ? 74 ? 45 85 C9 7E ? 83 6C 24 ? ? 75 ? 49 8B D0 E8 ? ? ? ? 88 43 ? 48 8B C3 C7 03 ? ? ? ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? 40 56 Add 3 TraceRelative")]
            [OffsetCN("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 32 C0 48 8B DA 48 83 79 ? ? 74 ? 45 85 C9 7E ? 83 6C 24 ? ? 75 ? 49 8B D0 E8 ? ? ? ? 88 43 ? 48 8B C3 C7 03 ? ? ? ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? 40 56 Add 3 TraceRelative")]
            internal static IntPtr VTable;
        }

        protected AgentMJIGatheringNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}