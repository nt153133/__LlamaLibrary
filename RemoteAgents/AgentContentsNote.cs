using System;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>Remote agent for the Challenge Log.</summary>
    public sealed class AgentContentsNote : AgentInterface
    {
        public const int AgentId = 136;

        private static AgentInterface _instance;

        public static AgentInterface Instance =>
            _instance ??= new AgentContentsNote(AgentModule.AgentPointers[AgentId]);

        private AgentContentsNote(IntPtr pointer) : base(pointer)
        {
        }
    }
}
