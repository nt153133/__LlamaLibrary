using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMeld : AgentInterface<AgentMeld>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 33 FF 48 89 03 48 8D 4B ? Add 3 TraceRelative")]

            //[OffsetCN("Search 48 8D 05 ? ? ? ? 48 8D 4B ? 48 89 03 E8 ? ? ? ? 48 8D 4B ? E8 ? ? ? ? 33 C9 Add 3 TraceRelative")]
            internal static IntPtr VTable;

            [Offset("Search 38 9F ? ? ? ? 48 8D 8D ? ? ? ? Add 2 Read32")]
            [OffsetDawntrail("Search 0F B6 9F ? ? ? ? 48 8D 8D ? ? ? ? BA ? ? ? ? 44 89 AD ? ? ? ? Add 3 Read32")]
            internal static int CanMeld;

            [Offset("Search 89 83 ? ? ? ? 48 89 83 ? ? ? ? 48 89 83 ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? Add 2 Read32")]
            internal static int ItemsToMeldCount;

            [Offset("Search 66 89 83 ? ? ? ? 66 89 83 ? ? ? ? 66 89 83 ? ? ? ? C6 83 ? ? ? ? ? Add 3 Read32")]
            internal static int IndexOfSelectedItem;

            [Offset("Search 0F BF B3 ? ? ? ? 49 8D 8F ? ? ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 0F BF BE ? ? ? ? 4D 8D 64 24 ? Add 3 Read32")]
            internal static int MateriaCount;
        }

        protected AgentMeld(IntPtr pointer) : base(pointer)
        {
        }

        public bool CanMeld => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.CanMeld) == 1;

        public bool Ready => Core.Memory.NoCacheRead<byte>(LlamaLibrary.Memory.Offsets.Conditions + 7) == 0;

        public byte ItemsToMeldCount => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.ItemsToMeldCount);

        public byte IndexOfSelectedItem => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.IndexOfSelectedItem);

        public byte MateriaCount => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.MateriaCount);
    }
}