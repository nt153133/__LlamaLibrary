using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentAchievement : AgentInterface<AgentAchievement>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        private static class Offsets
        {
            [Offset("Search 48 8D 0D ? ? ? ? 49 89 5E ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
            [Offset("Search 8B 43 ? 4C 89 6C 24 ? 83 F8 ? Add 2 Read8")]
            internal static int Status;
        }

        protected AgentAchievement(IntPtr pointer) : base(pointer)
        {
        }

        public byte Status => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.Status);
    }
}