using System;

namespace LlamaLibrary.RemoteAgents
{
    public interface IAgent
    {
        int RegisteredAgentId { get; }
    }
}