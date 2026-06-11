using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

public class AgentDeepDungeonStatus: AgentInterface
{
    public IntPtr RegisteredVtable => AgentDeepDungeonStatusOffsets.VTable;

    private static AgentInterface? _instance;

    public static int IdRaw { get; } = AgentModule.FindAgentIdByVtable(AgentDeepDungeonStatusOffsets.VTable);

    public static AgentInterface Instance => _instance ??= new AgentDeepDungeonStatus(AgentModule.AgentPointers[AgentModule.FindAgentIdByVtable(AgentDeepDungeonStatusOffsets.VTable)]);

    protected AgentDeepDungeonStatus(IntPtr pointer) : base(pointer)
    {
    }

}