using System.Collections.Generic;
using System.Threading.Tasks;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Supplies profile metadata and materializes profiles for LoadServerProfile without exposing credentials to LlamaLibrary.
/// </summary>
public interface IServerProfileSource
{
    /// <summary>
    /// Gets the profiles available to the calling plugin.
    /// </summary>
    Task<IReadOnlyList<ServerProfile>> GetProfilesAsync();

    /// <summary>
    /// Materializes a profile and returns a local path that NeoProfileManager can load.
    /// </summary>
    Task<string?> MaterializeProfileAsync(ServerProfile profile);
}
