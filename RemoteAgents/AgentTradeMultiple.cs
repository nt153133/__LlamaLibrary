using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the multiple-item trade interface.
/// Manages the exchange of multiple items or currencies in batch trade windows.
/// </summary>
public class AgentTradeMultiple: AgentInterface<AgentTradeMultiple>, IAgent
{
    /// <inheritdoc/>
    public IntPtr RegisteredVtable => PlatypusOffsets.AgentTradeMultiple;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentTradeMultiple"/> class.
    /// </summary>
    /// <param name="pointer">The memory address of the agent.</param>
    protected AgentTradeMultiple(IntPtr pointer) : base(pointer)
    {
    }
}