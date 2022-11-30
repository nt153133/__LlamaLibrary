using ff14bot.Behavior;
using TreeSharp;

namespace LlamaLibrary
{
    public interface ICompiledBotbase
    {
        string Name { get; }
        PulseFlags PulseFlags { get; }
        bool RequiresProfile { get; }
        bool WantButton { get; }
        bool IsAutonomous { get; }

        Composite GetRoot();
        void Start();
        void Stop();
        void OnButtonPress();
        void Initialize();
    }
}