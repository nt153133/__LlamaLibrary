using System.Collections.Generic;
using System.IO;

namespace LlamaLibrary.Helpers.Cache.BaseClasses;

/// <summary>
/// A three-level cache for objects of type <typeparamref name="T"/>:
/// <list type="number">
///   <item><description>In-memory dictionary (fastest).</description></item>
///   <item><description>On-disk file store via <see cref="IFileStorable{T}"/> (survives restarts).</description></item>
///   <item><description>Fallback fetch via <see cref="ICacheable{T}"/> on a full cache miss (e.g. reading from game data).</description></item>
/// </list>
/// </summary>
/// <typeparam name="T">The type of cached object. Must be a reference type.</typeparam>
public class FileCache<T> where T : class
{
    private readonly IFileStorable<T> _store;
    private readonly ICacheable<T> _cacheable;
    private readonly Dictionary<string, T> _cache = new(System.StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new <see cref="FileCache{T}"/> using the provided store and cacheable strategies.
    /// The <see cref="IFileStorable{T}.BasePath"/> must already be configured on <paramref name="store"/>.
    /// </summary>
    /// <param name="store">The file store that handles disk persistence.</param>
    /// <param name="cache">The cache-miss handler that fetches objects not yet on disk.</param>
    public FileCache(IFileStorable<T> store, ICacheable<T> cache)
    {
        _store = store;
        _cacheable = cache;
    }

    /// <summary>
    /// Initializes a new <see cref="FileCache{T}"/> with an explicit base directory, creating it if necessary.
    /// </summary>
    /// <param name="store">The file store that handles disk persistence.</param>
    /// <param name="cache">The cache-miss handler that fetches objects not yet on disk.</param>
    /// <param name="basePath">
    /// The root directory for cached files. Created automatically if it does not exist.
    /// </param>
    public FileCache(IFileStorable<T> store, ICacheable<T> cache, string basePath)
    {
        _store = store;
        _cacheable = cache;
        _store.BasePath = basePath;
        Directory.CreateDirectory(basePath);
    }

    /// <summary>
    /// Retrieves the object associated with <paramref name="key"/>, consulting the in-memory cache,
    /// disk store, and finally the <see cref="ICacheable{T}"/> fallback in that order.
    /// A successfully fetched object is stored to disk and promoted to the in-memory cache.
    /// </summary>
    /// <param name="key">The string key that uniquely identifies the object (e.g. an item ID).</param>
    /// <returns>
    /// The cached object, or <see langword="null"/> if the object could not be found or produced.
    /// </returns>
    public T? Get(string key)
    {
        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }

        T? item;
        if (_store.Exists(key))
        {
            item = _store.Load(key);
            if (item == null)
            {
                return null;
            }

            _cache[key] = item;
            return _cache[key];
        }

        item = _cacheable.CacheMiss(key);

        if (item == null)
        {
            return null;
        }

        _cache[key] = item;
        _store.Save(item, key);
        return _cache[key];
    }
}

/// <summary>
/// A convenience subclass of <see cref="FileCache{T}"/> that constructs its
/// <see cref="IFileStorable{T}"/> and <see cref="ICacheable{T}"/> instances automatically
/// via their default constructors.
/// </summary>
/// <typeparam name="T">The type of cached object. Must be a reference type.</typeparam>
/// <typeparam name="TFileStore">
/// The <see cref="IFileStorable{T}"/> implementation. Must have a public parameterless constructor.
/// </typeparam>
/// <typeparam name="TCacheable">
/// The <see cref="ICacheable{T}"/> implementation. Must have a public parameterless constructor.
/// </typeparam>
public class FileCache<T, TFileStore, TCacheable> : FileCache<T> where T : class where TFileStore : IFileStorable<T>, new() where TCacheable : ICacheable<T>, new()
{
    /// <summary>
    /// Initializes a new cache using default-constructed store and cacheable instances.
    /// The <see cref="IFileStorable{T}.BasePath"/> must be set on the store before use.
    /// </summary>
    public FileCache() : base(new TFileStore(), new TCacheable())
    {
    }

    /// <summary>
    /// Initializes a new cache rooted at <paramref name="path"/>, creating the directory if needed.
    /// </summary>
    /// <param name="path">The root directory for on-disk cached files.</param>
    public FileCache(string path) : base(new TFileStore(), new TCacheable(), path)
    {
    }
}