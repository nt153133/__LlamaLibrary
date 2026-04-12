using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

public class AgentTradeMultiple: AgentInterface<AgentTradeMultiple>, IAgent
{
    public IntPtr RegisteredVtable => PlatypusOffsets.AgentTradeMultiple;

    protected AgentTradeMultiple(IntPtr pointer) : base(pointer)
    {
    }
}