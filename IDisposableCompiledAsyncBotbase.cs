using System;

namespace LlamaLibrary;

/// <summary>
/// Defines an extended contract for an asynchronous botbase that is also <see cref="IDisposable"/>.
/// Includes additional metadata and lifecycle methods for pulse and shutdown.
/// </summary>
public interface IDisposableCompiledAsyncBotbase : ICompiledAsyncBotbase, IDisposable
{
    /// <summary>
    /// Gets the English display name of the botbase.
    /// </summary>
    string EnglishName { get; }

    /// <summary>
    /// Gets the version of the botbase.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Called when RebornBuddy is shutting down or when the botbase is being unloaded.
    /// </summary>
    void OnShutdown();

    /// <summary>
    /// Called on every RebornBuddy pulse (tick) while this botbase is active.
    /// </summary>
    void Pulse();
}