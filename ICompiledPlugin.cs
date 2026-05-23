using ff14bot.Interfaces;

namespace LlamaLibrary;

/// <summary>
/// Defines a contract for a RebornBuddy plugin that exposes an <see cref="IExportedApi"/>.
/// </summary>
public interface ICompiledPlugin : IBotPlugin
{
    /// <summary>
    /// Gets the exported API instance provided by this plugin.
    /// </summary>
    IExportedApi Api { get; }
}