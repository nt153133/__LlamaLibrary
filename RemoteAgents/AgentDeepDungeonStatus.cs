using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for tracking the player's Deep Dungeon status, progression, and state.
/// Handles accessing variables and vtable pointers specific to Deep Dungeons (such as Palace of the Dead, Heaven-on-High, and Eureka Orthos).
/// </summary>
public class AgentDeepDungeonStatus: AgentInterface
{
    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentDeepDungeonStatusOffsets.VTable;

    private static AgentInterface? _instance;

    /// <summary>
    /// Gets the raw Agent ID of the Deep Dungeon status agent.
    /// </summary>
    public static int IdRaw { get; } = AgentModule.FindAgentIdByVtable(AgentDeepDungeonStatusOffsets.VTable);

    /// <summary>
    /// Gets the singleton or active instance of the <see cref="AgentDeepDungeonStatus"/> remote agent.
    /// </summary>
    public static AgentInterface Instance => _instance ??= new AgentDeepDungeonStatus(AgentModule.AgentPointers[AgentModule.FindAgentIdByVtable(AgentDeepDungeonStatusOffsets.VTable)]);

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentDeepDungeonStatus"/> class.
    /// </summary>
    /// <param name="pointer">The memory address of the agent.</param>
    protected AgentDeepDungeonStatus(IntPtr pointer) : base(pointer)
    {
    }

}