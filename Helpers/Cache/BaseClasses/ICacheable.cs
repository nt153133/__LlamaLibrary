namespace LlamaLibrary.Helpers.Cache.BaseClasses;

/// <summary>
/// Defines the contract for retrieving a value that is not yet stored in a <see cref="FileCache{T}"/>.
/// Implementors are responsible for fetching the data from the game or another source on a cache miss.
/// </summary>
/// <typeparam name="T">The type of object this cacheable produces. Must be a reference type.</typeparam>
public interface ICacheable<out T> where T : class
{
    /// <summary>
    /// Called when the requested key is not present in the in-memory cache and does not exist on disk.
    /// Implementors should retrieve or construct the object (e.g. from game data) and return it,
    /// or return <see langword="null"/> if the object cannot be produced.
    /// </summary>
    /// <param name="key">The cache key identifying the requested object (e.g. an item ID string).</param>
    /// <returns>
    /// The newly fetched object for the given <paramref name="key"/>,
    /// or <see langword="null"/> if the object cannot be produced.
    /// </returns>
    public T? CacheMiss(string key);
}
