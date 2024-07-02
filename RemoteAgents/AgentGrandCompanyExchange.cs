using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentGrandCompanyExchange : AgentInterface<AgentGrandCompanyExchange>, IAgent
    {
        internal static class Offsets
        {
            //0x
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? E9 ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B D9 Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //0x69 byte
            [Offset("Search 0F B6 41 ? 48 8B CB 88 83 ? ? ? ? Add 3 Read8")]
            [OffsetDawntrail("Search 0F B6 51 ? 48 8B CB 88 93 ? ? ? ? Add 3 Read8")]
            internal static int Rank;

            //0x6a byte
            [Offset("Search 0F B6 40 ? FE C0 Add 3 Read8")]
            internal static int Category;

            //BuyItem (ShopPtr, 0, index, count)
            [Offset("Search 40 55 53 56 57 41 56 48 8D 6C 24 ? 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 45 ? 0F B7 71 ?")]
            [OffsetDawntrail("Search E8 ? ? ? ? 0F B6 D8 84 C0 74 ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B C8 48 8B 10 FF 92 ? ? ? ? Add 1 TraceRelative")]
            internal static IntPtr BuyItem;
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        public byte Category => Core.Memory.Read<byte>(Pointer + Offsets.Category);
        public byte Rank => Core.Memory.Read<byte>(Pointer + Offsets.Rank);

        protected AgentGrandCompanyExchange(IntPtr pointer) : base(pointer)
        {
        }

        public void BuyItem(uint index, int qty)
        {
            Core.Memory.CallInjected64<IntPtr>(Offsets.BuyItem, GrandCompanyShop.ActiveShopPtr, 0, index, qty);
        }
    }
}