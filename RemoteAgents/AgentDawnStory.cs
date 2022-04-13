using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentDawnStory : AgentInterface<AgentDawnStory>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.Vtable;

        private static class Offsets
        {
            [Offset("48 8D 05 ? ? ? ? C6 43 ? ? 48 89 03 48 8B C3 48 C7 43 ? ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;
        }

        protected AgentDawnStory(IntPtr pointer) : base(pointer)
        {
        }

        public byte SelectedDuty => Core.Memory.Read<byte>(Pointer + 0x28);
    }
}