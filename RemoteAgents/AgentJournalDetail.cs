using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentJournalDetail : AgentInterface<AgentJournalDetail>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 54 24 ? 48 89 03 Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 89 07 48 8D 8F ? ? ? ? C7 87 ? ? ? ? ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
        }

        protected AgentJournalDetail(IntPtr pointer) : base(pointer)
        {
        }
    }
}