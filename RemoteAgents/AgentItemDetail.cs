using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentItemDetail : AgentInterface<AgentItemDetail>, IAgent
    {
        private static class Offsets
        {
            //0x18C9FC0
            [Offset("Search 48 8D 05 ? ? ? ? 48 8B F9 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? C6 81 ? ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //0x5
            [Offset("Search 8B 93 ? ? ? ? E8 ? ? ? ? 4C 63 F0 Add 2 Read32")]
            //[OffsetCN("Search 8B 93 ? ? ? ? 48 8B CB E8 ? ? ? ? 4C 63 F0 Add 2 Read32")]
            internal static int ItemID;
        }

        protected AgentItemDetail(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        public uint HoverOverItemID => Core.Memory.Read<uint>(Pointer + Offsets.ItemID);
    }
}