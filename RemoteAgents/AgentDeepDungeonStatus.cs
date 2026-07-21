using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Deep Dungeon Status interface.
/// Tracks player progression, stats, and state inside Deep Dungeons such as Palace of the Dead, Heaven-on-High, and Eureka Orthos.
/// </summary>
public class AgentDeepDungeonStatus : AgentInterface
{
    /// <summary>
    /// Gets the pointer to the registered virtual function table (VTable) for this agent.
    /// </summary>
    public IntPtr RegisteredVtable => AgentDeepDungeonStatusOffsets.VTable;

    private static AgentInterface? _instance;

    /// <summary>
    /// Gets the raw identifier of this agent, found dynamically by searching for the agent's VTable in the agent module.
    /// </summary>
    public static int IdRaw { get; } = AgentModule.FindAgentIdByVtable(AgentDeepDungeonStatusOffsets.VTable);

    /// <summary>
    /// Gets the singleton instance of the <see cref="AgentDeepDungeonStatus"/> agent.
    /// Resolves the raw agent pointer dynamically using the agent module and VTable mapping.
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
