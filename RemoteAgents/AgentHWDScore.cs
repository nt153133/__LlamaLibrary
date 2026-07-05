using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Ishgardian Restoration (Holy Works of Development) score tracking.
    /// Manages the display of rankings and total scores for the various crafting and gathering classes.
    /// </summary>
    //TODO This agent has hardcoded memory offsets
    public class AgentHWDScore : AgentInterface<AgentHWDScore>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentHWDScoreOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHWDScore"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentHWDScore(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Reads the total scores for the 11 crafting and gathering classes from game memory.
        /// </summary>
        /// <returns>An array of 11 integers representing the scores for each class.</returns>
        public int[] ReadTotalScores()
        {
            return Core.Memory.ReadArray<int>(Pointer + 0x90, 11);
        }
    }
}