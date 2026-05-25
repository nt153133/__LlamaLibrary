using System;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Represents the remote agent responsible for managing the list of retainers.
    /// Provides access to the raw retainer pointer array and helper methods to order retainer information.
    /// </summary>
    //TODO This agent might be completely useless given the current way I get the retainers
    public class AgentRetainerList : AgentInterface<AgentRetainerList>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentRetainerListOffsets.VTable;

        protected AgentRetainerList(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets an array of pointers to the retainers managed by this agent.
        /// </summary>
        public IntPtr[] RetainerList => Core.Memory.ReadArray<IntPtr>(Pointer + AgentRetainerListOffsets.AgentRetainerOffset, AgentRetainerListOffsets.MaxRetainers);

        /// <summary>
        /// Returns an ordered list of <see cref="RetainerInfo"/> based on the memory order in <see cref="RetainerList"/>.
        /// </summary>
        /// <param name="retainers">The source array of retainer information to be ordered.</param>
        /// <returns>An array of <see cref="RetainerInfo"/> sorted according to the agent's internal list.</returns>
        public RetainerInfo[] OrderedRetainerList(RetainerInfo[] retainers)
        {
            var count = RetainerList.Count(i => i != IntPtr.Zero);

            if (count == 0)
            {
                return retainers;
            }

            var result = new RetainerInfo[count]; // new List<KeyValuePair<int, RetainerInfo>>();

            //IntPtr[] RetainerList = Core.Memory.ReadArray<IntPtr>(new IntPtr(0x18FD0C64510) + 0x4a8, 0xA);
            var index = 0;
            foreach (var ptr in RetainerList.Where(i => i != IntPtr.Zero))
            {
                var next = Core.Memory.Read<IntPtr>(ptr);

                result[index] = retainers.First(j => j.Name.Equals(Core.Memory.ReadStringUTF8(next)));
                index++;
            }

            return result;
        }
    }
}