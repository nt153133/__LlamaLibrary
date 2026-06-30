using System;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Triple Triad card exchange interface.
/// Facilitates the exchange of duplicate Triple Triad cards for Manderville Gold Saucer Points (MGP).
/// </summary>
public class AgentTripleTriadCoinExchange : AgentInterface<AgentTripleTriadCoinExchange>, IAgent
{
    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentTripleTriadCoinExchangeOffsets.VTable;

    /// <summary>
    /// Gets the number of unique card stacks available for exchange.
    /// </summary>
    public int CardCount => Core.Memory.Read<ushort>(Pointer + AgentTripleTriadCoinExchangeOffsets.CardCount);

    /// <summary>
    /// Gets the memory pointer to the start of the exchangeable card list.
    /// </summary>
    public IntPtr ListPtr => Core.Memory.Read<IntPtr>(Pointer + AgentTripleTriadCoinExchangeOffsets.ListPtr);

    /// <summary>
    /// Gets an array of indices used to map the display order to the internal card list.
    /// </summary>
    public uint[] CardIndexArray => Core.Memory.ReadArray<uint>(Core.Memory.Read<IntPtr>(Pointer + AgentTripleTriadCoinExchangeOffsets.CardIndexArray), CardCount);

    /// <summary>
    /// Gets or sets the index of the currently selected card in the exchange interface.
    /// </summary>
    public uint SelectedCardIndex
    {
        get => Core.Memory.Read<uint>(Pointer + AgentTripleTriadCoinExchangeOffsets.SelectedCardIndex);
        set => Core.Memory.Write(Pointer + AgentTripleTriadCoinExchangeOffsets.SelectedCardIndex, value);
    }

    //private void OpenSellWindowRaw(IntPtr cardPTr) => Core.Memory.CallInjectedWraper<IntPtr>(Offsets.OpenSellWindow, Pointer, cardPTr);

    /// <summary>
    /// Gets the list of Triple Triad cards available for exchange, ordered by their display index.
    /// Each card includes its item ID, MGP value, and current inventory count.
    /// </summary>
    public TripleTriadCoinExchangeCard[] Cards
    {
        get
        {
            var cards =  Core.Memory.ReadArray<TripleTriadCoinExchangeCard>(ListPtr, CardCount);
            var cardIndexes = CardIndexArray;
            for (var index = 0; index < cardIndexes.Length; index++)
            {
                cards[cardIndexes[index]].SendAction = (uint)index;
            }

            return cards.OrderBy(i=> i.SendAction).ToArray();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentTripleTriadCoinExchange"/> class.
    /// </summary>
    /// <param name="pointer">The memory address of the agent.</param>
    protected AgentTripleTriadCoinExchange(IntPtr pointer) : base(pointer)
    {
    }
}