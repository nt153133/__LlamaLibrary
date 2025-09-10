using System;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

public class AgentTripleTriadCoinExchange : AgentInterface<AgentTripleTriadCoinExchange>, IAgent
{
    public IntPtr RegisteredVtable => AgentTripleTriadCoinExchangeOffsets.VTable;

    

    public int CardCount => Core.Memory.Read<ushort>(Pointer + AgentTripleTriadCoinExchangeOffsets.CardCount);

    public IntPtr ListPtr => Core.Memory.Read<IntPtr>(Pointer + AgentTripleTriadCoinExchangeOffsets.ListPtr);

    public uint[] CardIndexArray => Core.Memory.ReadArray<uint>(Core.Memory.Read<IntPtr>(Pointer + AgentTripleTriadCoinExchangeOffsets.CardIndexArray), CardCount);

    public uint SelectedCardIndex
    {
        get => Core.Memory.Read<uint>(Pointer + AgentTripleTriadCoinExchangeOffsets.SelectedCardIndex);
        set => Core.Memory.Write(Pointer + AgentTripleTriadCoinExchangeOffsets.SelectedCardIndex, value);
    }

    //private void OpenSellWindowRaw(IntPtr cardPTr) => Core.Memory.CallInjectedWraper<IntPtr>(Offsets.OpenSellWindow, Pointer, cardPTr);

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