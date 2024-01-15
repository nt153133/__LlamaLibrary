using System.Threading.Tasks;

namespace ff14bot.AClasses;

/// <summary>
/// Interface for Quest Behavior Libraries to get initalized during RB startup
/// </summary>
public interface ILibrary
{
    /// <summary>
    /// Called directly after the quest behaviors are compiled, runs asynchronously alongside the botbase->plugin->routine compile steps
    /// </summary>
    /// <returns>true if warmup was successful</returns>
    public Task<bool> PreOffsetWarmup();

    /// <summary>
    /// Runs after offsets have been downloaded and imported
    /// </summary>
    /// <returns></returns>
    public Task<bool> PostOffsetWarmup();
}