using System;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Challenge Log interface (ContentsNote).
    /// Manages the state and provides interaction hooks for the in-game Challenge Log menu.
    /// </summary>
    public sealed class AgentContentsNote : AgentInterface
    {
        /// <summary>
        /// The unique ID identifying the Challenge Log agent in the game's AgentModule.
        /// </summary>
        public const int AgentId = 136;

        private static AgentInterface _instance;

        /// <summary>
        /// Gets the singleton instance of the <see cref="AgentContentsNote"/> remote agent.
        /// Lazily initializes using the pointer obtained from the game's <see cref="AgentModule"/>.
        /// </summary>
        public static AgentInterface Instance =>
            _instance ??= new AgentContentsNote(AgentModule.AgentPointers[AgentId]);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentContentsNote"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        private AgentContentsNote(IntPtr pointer) : base(pointer)
        {
        }
    }
}
