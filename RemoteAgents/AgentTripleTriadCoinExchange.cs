using System;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.RemoteAgents;

public class AgentTripleTriadCoinExchange : AgentInterface<AgentTripleTriadCoinExchange>, IAgent
{
    public IntPtr RegisteredVtable => Offsets.VTable;

    private static class Offsets
    {
        [Offset("Search 3B 79 ? 0F 83 ? ? ? ? 48 8B 41 ? Add 2 Read8")]
        internal static int CardCount;

        [Offset("Search 48 03 59 ? 48 8B CB 41 89 3C 06 Add 3 Read8")]
        internal static int ListPtr;

        [Offset("Search 48 8D 05 ? ? ? ? 48 8B D3 48 8D 4F ? 48 89 07 E8 ? ? ? ? 48 8B 5C 24 ? 33 C0 48 89 47 ? 48 89 47 ? 48 89 47 ? 48 89 47 ? 89 87 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 41 54 41 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 48 8B 01")]
        internal static IntPtr OpenSellWindow;

        [Offset("Search 41 8B 96 ? ? ? ? 49 8B CE 49 8B 46 ? Add 3 Read8")]
        internal static int SelectedCardIndex;

        [Offset("Search 49 8B 46 ? 8B 14 90 48 69 D2 ? ? ? ? Add 3 Read8")]
        internal static int CardIndexArray;
    }

    public int CardCount => Core.Memory.Read<ushort>(Pointer + Offsets.CardCount);

    public IntPtr ListPtr => Core.Memory.Read<IntPtr>(Pointer + Offsets.ListPtr);

    public uint[] CardIndexArray => Core.Memory.ReadArray<uint>(Core.Memory.Read<IntPtr>(Pointer + Offsets.CardIndexArray), CardCount);

    public uint SelectedCardIndex
    {
        get => Core.Memory.Read<uint>(Pointer + Offsets.SelectedCardIndex);
        set => Core.Memory.Write(Pointer + Offsets.SelectedCardIndex, value);
    }

    private void OpenSellWindowRaw(IntPtr cardPTr) => Core.Memory.CallInjected64<IntPtr>(Offsets.OpenSellWindow, Pointer, cardPTr);

    public TripleTriadCoinExchangeCard[] Cards
    {
        get
        {
            var cards =  Core.Memory.ReadArray<TripleTriadCoinExchangeCard>(ListPtr, CardCount);
            var cardIndexes = CardIndexArray;
            for (var index = 0; index < cardIndexes.ToList().Count; index++)
            {
                cards[cardIndexes[index]].SendAction = (uint)index;
            }

            return cards.OrderBy(i=> i.SendAction).ToArray();
        }
    }

    protected AgentTripleTriadCoinExchange(IntPtr pointer) : base(pointer)
    {
    }
}