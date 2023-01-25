using System.IO;

namespace LlamaLibrary.Helpers.Cache;

public static class Caches
{
    public static string BasePath = Path.Combine(Clio.Utilities.Utilities.AssemblyDirectory, "Cache");

    public static IconFileCache IconFileCache = new IconFileCache(Path.Combine(BasePath, "Icons"));

    public static CustomIconFileCache IconFileCache20x20 = new CustomIconFileCache(Path.Combine(BasePath, "Icons"), 20);
}