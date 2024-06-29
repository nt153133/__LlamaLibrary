﻿using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentBagSlot : AgentInterface<AgentBagSlot>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        private static class Offsets
        {
            //48 8D 05 ? ? ? ? 48 89 79 ? 48 89 01 89 79 ?
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 79 ? 48 89 01 89 79 ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 89 79 ? 48 8B D9 Add 3 TraceRelative")]
            internal static IntPtr VTable;
            [Offset("Search 48 03 83 ? ? ? ? 0F 84 ? ? ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 48 8B 89 ? ? ? ? 48 85 C9 74 ? E8 ? ? ? ? 48 C7 86 ? ? ? ? ? ? ? ? BF ? ? ? ? 48 8D 9E ? ? ? ? 0F 1F 44 00 ? Add 3 Read32")]
            internal static int Offset;
            [Offset("Search 48 8B 48 ? 48 85 C9 0F 84 ? ? ? ? 44 8B 8B ? ? ? ? Add 3 Read8")]
            //May be wrong
            [OffsetDawntrail("Search 48 8B 48 ? 48 85 C9 0F 84 ? ? ? ? 8B 93 ? ? ? ? Add 3 Read8")]
            internal static int FuncOffset;
        }

        protected AgentBagSlot(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr PointerForAether => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + Offsets.Offset) + (0x20 * 7) + Offsets.FuncOffset);
    }
}