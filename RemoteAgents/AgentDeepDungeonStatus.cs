using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Deep Dungeon status and progress interface.
/// Manages information about current Deep Dungeon floor, score, and state.
/// </summary>
public class AgentDeepDungeonStatus : AgentInterface
{
    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentDeepDungeonStatusOffsets.VTable;

    private static AgentInterface? _instance;

    /// <summary>
    /// Gets the raw agent ID for the Deep Dungeon status agent by looking up its VTable.
    /// </summary>
    public static int IdRaw { get; } = AgentModule.FindAgentIdByVtable(AgentDeepDungeonStatusOffsets.VTable);

    /// <summary>
    /// Gets the singleton instance of the <see cref="AgentDeepDungeonStatus"/> by resolving its pointer from the <see cref="AgentModule"/>.
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
