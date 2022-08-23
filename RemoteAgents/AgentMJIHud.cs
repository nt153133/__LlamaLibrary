using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIHud : AgentInterface<AgentMJIHud>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B DA 32 D2 48 83 79 ? ? 74 ? 45 85 C9 7E ? 8B 44 24 ? 83 E8 ? 74 ? 83 E8 ? 74 ? 83 F8 ? 75 ? 49 8B D0 E8 ? ? ? ? EB ? 49 8B D0 E8 ? ? ? ? EB ? 49 8B D0 E8 ? ? ? ? 0F B6 D0 C7 03 ? ? ? ? 48 8B C3 88 53 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B D9 Add 3 TraceRelative")]
            internal static IntPtr VTable;
        }

        protected AgentMJIHud(IntPtr pointer) : base(pointer)
        {
        }
    }
}