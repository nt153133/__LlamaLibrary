using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for managing multi-item trade interfaces (e.g., trading multiple items to an NPC for a single reward).
/// </summary>
public class AgentTradeMultiple : AgentInterface<AgentTradeMultiple>, IAgent
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