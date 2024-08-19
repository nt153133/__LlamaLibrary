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
            [OffsetDawntrail("Search 48 8D 0D ? ? ? ? 48 89 4C 24 ? 48 8D 0D ? ? ? ? 48 89 4C 24 ? 48 8D 4C 24 ? 48 8B 40 ? 48 89 44 24 ? E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? E9 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8D 79 ? 48 8B D9 41 8B F0")]
            [OffsetDawntrail("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8D 71 ? 48 8B F9 41 8B D8 48 8B CE 8B EA FF 50 ? 84 C0 0F 84 ? ? ? ? 48 8B CE E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 8B 47 ? 83 B8 ? ? ? ? ? 0F 85 ? ? ? ? 44 8B C5 4C 89 74 24 ? 8B D3 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B D0 48 8B CE 4C 8B F0 E8 ? ? ? ? 84 C0 74 ? 48 8B 7F ? 33 C9 0F B6 57 ? 85 D2 74 ? 0F 1F 80 ? ? ? ? 81 7C CF ? ? ? ? ? 8B D9 74 ? FF C1 3B CA 72 ? EB ? 45 0F B7 46 ? 48 8D 0D ? ? ? ? 41 8B 16 E8 ? ? ? ? 41 8B 06 48 8B CE 89 44 DF ? 41 0F BF 46 ? 89 44 DF ? E8 ? ? ? ? 33 D2 45 33 C9 45 33 C0 8D 4A ? E8 ? ? ? ? 4C 8B 74 24 ? 48 8B 5C 24 ? 48 8B 6C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? 48 89 5C 24 ? 57 48 83 EC ? 48 8B 01 48 8B DA 48 8B F9 FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 80 7B ? ? 74 ? 48 8B CB E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? 8B C8 EB ? 0F B6 43 ? 8B 4B ? A8 ? 74 ? 81 C1 ? ? ? ? EB ? A8 ? 74 ?")]
            internal static IntPtr HandIn;

            //0x28
            [Offset("Search FF 50 ? 84 C0 0F 84 ? ? ? ? 48 8B CF E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 8B 43 ? Add 2 Read8")]
            [OffsetDawntrail("Search FF 50 ? 84 C0 0F 84 ? ? ? ? 48 8B CE E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 8B 47 ? 83 B8 ? ? ? ? ? 0F 85 ? ? ? ? 44 8B C5 4C 89 74 24 ? 8B D3 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B D0 48 8B CE 4C 8B F0 E8 ? ? ? ? 84 C0 74 ? 48 8B 7F ? 33 C9 0F B6 57 ? 85 D2 74 ? 0F 1F 80 ? ? ? ? 81 7C CF ? ? ? ? ? 8B D9 74 ? FF C1 3B CA 72 ? EB ? 45 0F B7 46 ? 48 8D 0D ? ? ? ? 41 8B 16 E8 ? ? ? ? 41 8B 06 48 8B CE 89 44 DF ? 41 0F BF 46 ? 89 44 DF ? E8 ? ? ? ? 33 D2 45 33 C9 45 33 C0 8D 4A ? E8 ? ? ? ? 4C 8B 74 24 ? 48 8B 5C 24 ? 48 8B 6C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? 48 89 5C 24 ? 57 48 83 EC ? 48 8B 01 48 8B DA 48 8B F9 FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 80 7B ? ? 74 ? 48 8B CB E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? 8B C8 EB ? 0F B6 43 ? 8B 4B ? A8 ? 74 ? 81 C1 ? ? ? ? EB ? A8 ? 74 ? 81 C1 ? ? ? ? 48 8B 47 ? 3B 48 ? 75 ? 48 8B CB E8 ? ? ? ? 48 8B 4F ? 66 3B 81 ? ? ? ? 72 ? 48 8B CB E8 ? ? ? ? Add 2 Read8")]
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
                Core.Memory.CallInjected64<uint>(
                    Offsets.HandIn,
                    Pointer + Offsets.PointerOffset,
                    slot.Slot,
                    (int)slot.BagId);
            }
        }
    }
}