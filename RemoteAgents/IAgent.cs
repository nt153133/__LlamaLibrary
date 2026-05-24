using System;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Defines a contract for a remote agent in FFXIV, providing access to its virtual function table (VTable).
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Gets the pointer to the registered virtual function table for this agent.
        /// </summary>
        IntPtr RegisteredVtable { get; }
    }
}