using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;


public class AgentBankaCraftworksSupply : AgentInterface<AgentBankaCraftworksSupply>, IAgent
{
    public static class Offsets
    {
        //7.2 hf
        [Offset("Search 48 8D 0D ? ? ? ? 83 7B ? ? 48 89 4C 24 ? 48 8D 0D ? ? ? ? 88 54 24 ? 41 0F 94 C0 83 3B ? 48 89 4C 24 ? 48 8D 4C 24 ? 48 8B 40 ? 0F 94 C2 48 89 44 24 ? E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? 48 83 C4 ? 5B C3 0F 1F 00 Add 3 TraceRelative")]
        //7.2
        //[Offset("Search 48 8D 0D ? ? ? ? 83 7B ? ? 48 89 4C 24 ? 48 8D 0D ? ? ? ? 88 54 24 ? 41 0F 94 C0 ? ? ? 48 89 4C 24 ? 48 8D 4C 24 ? 48 8B 40 ? 0F 94 C2 48 89 44 24 ? E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? 48 83 C4 ? 5B C3 ? ? ? CA Add 3 TraceRelative")]
        //[OffsetCN("Search 48 8D 0D ? ? ? ? 48 89 4C 24 ? 48 8D 0D ? ? ? ? 48 89 4C 24 ? 48 8D 4C 24 ? 48 8B 40 ? 48 89 44 24 ? E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? 48 83 C4 ? 5B C3 0F 1F 00 Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //7.2 hf
        [Offset("Search FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 48 8B 03 48 8B CB FF 50 ? 48 8B 4F ? 3B 41 ? 75 ? 48 8B 03 48 8B CB FF 90 ? ? ? ? 48 8B 4F ? 66 3B 81 ? ? ? ? 72 ? 48 8B CB E8 ? ? ? ? 48 85 C0 75 ? B0 ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 48 8B 5C 24 ? 32 C0 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 2 Read8")]
        //0x28
        //7.2
        //[Offset("Search FF 50 30 84 C0 74 4A 48 85 DB 74 45 48 8B 03 48 8B CB FF 50 30 48 8B 4F 30 3B 41 08 75 33 48 8B 03 48 8B CB FF 90 B8 00 00 00 48 8B 4F 30 66 3B 81 34 01 00 00 72 1A 48 8B CB E8 3A 99 AA FF Add 2 Read8")]
        //[OffsetCN("Search FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 48 8B 03 48 8B CB FF 50 ? 48 8B 4F ? 3B 41 ? 75 ? 48 8B 03 48 8B CB FF 90 ? ? ? ? A8 ? 74 ? 48 8B 03 48 8B CB FF 50 ? 0F B7 C8 EB ? 33 C9 48 8B 47 ? 66 3B 88 ? ? ? ? 72 ? 48 8B CB E8 ? ? ? ? 48 85 C0 75 ? B0 ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 48 8B 5C 24 ? 32 C0 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? 48 89 5C 24 ? Add 2 Read8")]
        internal static int PointerOffset;
    }

    protected AgentBankaCraftworksSupply(IntPtr pointer) : base(pointer)
    {
    }

    public IntPtr RegisteredVtable => Offsets.Vtable;

    public void HandIn(BagSlot slot)
    {
        var instance = Pointer + Offsets.PointerOffset;
        lock (Core.Memory.Executor.AssemblyLock)
        {
            Core.Memory.CallInjectedWraper<uint>(Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(instance)),
                                                                      instance,
                                                                      slot.Slot,
                                                                      (int)slot.BagId);
        }
    }
}
