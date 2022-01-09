using System.Threading.Tasks;
using ff14bot.Behavior;

namespace LlamaLibrary
{
    public interface ICompiledAsyncBotbase
    {
        string Name { get; }
        PulseFlags PulseFlags { get; }
        bool RequiresProfile { get; }
        bool WantButton { get; }
        bool IsAutonomous { get; }

        Task AsyncRoot();
        void Start();
        void Stop();
        void OnButtonPress();
    }
}