using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentCharacter : AgentInterface<AgentCharacter>, IAgent
    {
        private static class Offsets
        {
            //7.1
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 06 48 8D 8E ? ? ? ? 0F 57 C0 Add 3 TraceRelative")]
            [OffsetCN("Search 48 8D 05 ? ? ? ? 89 77 ? 48 8D 4F ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;
        }

        protected AgentCharacter(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;
    }
}