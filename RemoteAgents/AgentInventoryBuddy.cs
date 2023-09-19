using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentInventoryBuddy : AgentInterface<AgentInventoryBuddy>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            //6.4
            [Offset("Search 48 8D 05 ? ? ? ? BE ? ? ? ? 48 89 07 48 8D 5F ? 48 8D 05 ? ? ? ? 33 ED 48 89 47 ? 0F 1F 40 ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
        }

        protected AgentInventoryBuddy(IntPtr pointer) : base(pointer)
        {
        }
    }
}