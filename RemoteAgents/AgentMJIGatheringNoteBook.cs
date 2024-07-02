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
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 32 C0 48 8B DA 48 83 79 ? ? 74 ? 45 85 C9 7E ? 83 6C 24 ? ? 75 ? 49 8B D0 E8 ? ? ? ? 88 43 ? 48 8B C3 C7 03 ? ? ? ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? 40 53 Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 32 C0 49 8B F0 48 83 79 ? ? 48 8B FA 48 8B D9 74 ? 45 85 C9 7E ? 83 6C 24 ? ? 75 ? 49 8B C8 E8 ? ? ? ? 83 F8 ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
        }

        protected AgentMJIGatheringNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}