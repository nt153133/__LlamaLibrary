using System.Threading.Tasks;
using ff14bot.Behavior;

namespace LlamaLibrary
{
    /// <summary>
    /// Defines the contract for an asynchronous botbase compiled and loaded by LlamaLibrary.
    /// Uses <see cref="Task"/>-based logic instead of <see cref="TreeSharp.Composite"/>.
    /// </summary>
    public interface ICompiledAsyncBotbase
    {
        /// <summary>
        /// Gets the display name of the botbase as shown in the RebornBuddy bot selection dropdown.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the pulse flags that define which RebornBuddy engine features (e.g., targeting, movement)
        /// are active while this botbase is running.
        /// </summary>
        PulseFlags PulseFlags { get; }

        /// <summary>
        /// Gets a value indicating whether this botbase requires a profile (e.g., an XML quest profile) to function.
        /// </summary>
        bool RequiresProfile { get; }

        /// <summary>
        /// Gets a value indicating whether this botbase provides a configuration or settings button in the RebornBuddy UI.
        /// </summary>
        bool WantButton { get; }

        /// <summary>
        /// Gets a value indicating whether the botbase is autonomous, meaning it manages its own navigation and task logic.
        /// </summary>
        bool IsAutonomous { get; }

        /// <summary>
        /// The main asynchronous execution loop of the botbase.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AsyncRoot();

        /// <summary>
        /// Called by RebornBuddy when the user starts the bot.
        /// </summary>
        void Start();

        /// <summary>
        /// Called by RebornBuddy when the user stops the bot.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called when the user clicks the botbase's button in the RebornBuddy UI.
        /// Only invoked if <see cref="WantButton"/> is <see langword="true"/>.
        /// </summary>
        void OnButtonPress();

        /// <summary>
        /// Passes external configuration parameters to the botbase.
        /// </summary>
        /// <param name="param">A string containing configuration or command parameters.</param>
        void SetParameters(string param);

        /// <summary>
        /// Called when the botbase is first loaded or initialized by the assembly loader.
        /// </summary>
        void Initialize();
    }
}