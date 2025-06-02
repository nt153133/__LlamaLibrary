using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentRetainerInventory : AgentInterface<AgentRetainerInventory>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        // ReSharper disable once PartialTypeWithSinglePart
        private static partial class Offsets
        {
            //7.2
            //48 8D 05 ? ? ? ? 48 89 6E ? 48 89 06 48 8D 9E ? ? ? ?
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 6E ? 48 89 06 48 8D 9E ? ? ? ? Add 3 TraceRelative")]
            [OffsetCN("Search 48 8D 05 ? ? ? ? 48 89 6F ? 48 89 07 48 8D 9F ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
            [Offset("Search 48 8B 8B ? ? ? ? 48 85 C9 74 ? 48 83 C4 ? 5B E9 ? ? ? ? B0 ? Add 3 Read32")]
            internal static int ShopOffset;
        }

        protected AgentRetainerInventory(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RetainerShopPointer => Core.Memory.Read<IntPtr>(Pointer + Offsets.ShopOffset);
    }
}