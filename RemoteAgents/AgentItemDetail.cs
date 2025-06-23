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
            //7.2
            [Offset("Search 48 8D 05 ? ? ? ? 48 8B F9 ? ? ? 48 81 C1 ? ? ? ? E8 ? ? ? ? 48 8D 8F ? ? ? ? E8 ? ? ? ? 48 8B 8F Add 3 TraceRelative")]
            //[OffsetCN("Search 48 8D 05 ? ? ? ? 48 8B F9 48 89 01 48 81 C1 ? ? ? ? E8 ? ? ? ? 48 8B 8F ? ? ? ? 48 8D 05 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //0x5
            [Offset("Search 8B 93 ? ? ? ? E8 ? ? ? ? 4C 63 F0 Add 2 Read32")]
            [OffsetDawntrail("Search 44 8B 8F ? ? ? ? 4C 8B C6 48 8B D5 48 8B CF E8 ? ? ? ? 4C 8B C6 48 8B D5 48 8B CF 0F B6 D8 E8 ? ? ? ? 84 DB 48 8B 5C 24 ? 75 ? 45 33 C9 C6 44 24 ? ? 45 33 C0 48 8B CE 41 8D 51 ? E8 ? ? ? ? 48 8B 4F ? Add 3 Read32")]
            internal static int ItemID;
        }

        protected AgentItemDetail(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        public uint HoverOverItemID => Core.Memory.Read<uint>(Pointer + Offsets.ItemID);
    }
}