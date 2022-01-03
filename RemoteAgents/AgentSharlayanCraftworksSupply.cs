using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentSharlayanCraftworksSupply : AgentInterface<AgentSharlayanCraftworksSupply>, IAgent
    {
        public static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 33 F6 48 89 01 48 8B D9 48 8D 05 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8D 79 ? 48 8B D9 41 8B F0")]
            internal static IntPtr HandIn;

            //0x28
            [Offset("Search FF 50 ? 84 C0 0F 84 ? ? ? ? 48 8B CF E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 8B 43 ? Add 2 Read8")]
            internal static int PointerOffset;
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        protected AgentSharlayanCraftworksSupply(IntPtr pointer) : base(pointer)
        {
        }

        public void HandIn(BagSlot slot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjected64<uint>(Offsets.HandIn,
                                                 Pointer + Offsets.PointerOffset,
                                                 slot.Slot,
                                                 (int)slot.BagId);
            }
        }
    }
}