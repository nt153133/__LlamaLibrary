using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentCharacter : AgentInterface<AgentCharacter>, IAgent
    {
        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 89 77 ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 8B D9 48 89 01 E8 ? ? ? ? 48 8B 4B ? 48 85 C9 74 ? 48 8B 53 ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;
        }

        protected AgentCharacter(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;
    }
}