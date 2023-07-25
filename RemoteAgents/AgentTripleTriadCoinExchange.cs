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
    }

    public int CardCount => Core.Memory.Read<ushort>(Pointer + Offsets.CardCount);

    public IntPtr ListPtr => Core.Memory.Read<IntPtr>(Pointer + Offsets.ListPtr);

    public TripleTriadCoinExchangeCard[] Cards => Core.Memory.ReadArray<TripleTriadCoinExchangeCard>(ListPtr, CardCount).OrderBy(i=> i.Price).ThenBy(i=> i.Index).ToArray();

    protected AgentTripleTriadCoinExchange(IntPtr pointer) : base(pointer)
    {
    }
}