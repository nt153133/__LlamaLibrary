using System.IO;

namespace LlamaLibrary.Helpers.Cache;

/// <summary>
/// Central registry of shared <see cref="FileCache{T}"/> instances used throughout LlamaLibrary.
/// Each cache persists data under a sub-folder of <see cref="BasePath"/>, which is located
/// alongside the currently loaded assembly.
/// </summary>
public static class Caches
{
    /// <summary>
    /// The root directory under which all cache sub-folders are created.
    /// Defaults to a <c>Cache</c> folder next to the loaded assembly.
    /// </summary>
    public static string BasePath = Path.Combine(Clio.Utilities.Utilities.AssemblyDirectory, "Cache");

    /// <summary>
    /// Full-size FFXIV item icon cache. Icons are stored as PNG files keyed by
    /// <c>(itemId % 1_000_000).ToString()</c> under <see cref="BasePath"/><c>/Icons</c>.
    /// </summary>
    public static IconFileCache IconFileCache = new IconFileCache(Path.Combine(BasePath, "Icons"));

    /// <summary>
    /// Thumbnail item icon cache that saves icons at 20×20 pixels.
    /// Icons share the same key scheme as <see cref="IconFileCache"/> but are stored with a
    /// <c>_20x20</c> suffix to avoid collisions.
    /// </summary>
    public static CustomIconFileCache IconFileCache20x20 = new CustomIconFileCache(Path.Combine(BasePath, "Icons"), 20);
}