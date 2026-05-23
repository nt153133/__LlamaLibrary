using System;
using ff14bot.Behavior;
using TreeSharp;

namespace LlamaLibrary
{
    /// <summary>
    /// Defines the basic contract for a botbase compiled and loaded by LlamaLibrary.
    /// </summary>
    public interface ICompiledBotbase
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
        /// Returns the <see cref="Composite"/> logic tree that RebornBuddy will execute while the bot is running.
        /// </summary>
        /// <returns>A TreeSharp composite representing the bot's behavior.</returns>
        Composite GetRoot();

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
        /// Called when the botbase is first loaded or initialized by the assembly loader.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// An extended version of <see cref="ICompiledBotbase"/> that includes additional lifecycle methods and metadata.
    /// </summary>
    public interface ICompiledBotbaseFull : ICompiledBotbase, IDisposable
    {
        /// <summary>
        /// Gets the version string of the botbase.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Called on every RebornBuddy pulse (tick) while this botbase is active.
        /// </summary>
        void Pulse();

        /// <summary>
        /// Called when RebornBuddy is shutting down or when the botbase is being unloaded.
        /// </summary>
        void OnShutdown();

        /// <summary>
        /// Gets the English display name of the botbase, useful for logging or internationalization.
        /// </summary>
        string EnglishName { get; }
    }
}