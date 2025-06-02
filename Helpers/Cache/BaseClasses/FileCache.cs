using System.Collections.Generic;
using System.IO;

namespace LlamaLibrary.Helpers.Cache.BaseClasses;

public class FileCache<T> where T : class
{
    private readonly IFileStorable<T> _store;
    private readonly ICacheable<T> _cacheable;
    private readonly Dictionary<string, T> _cache = new(System.StringComparer.Ordinal);

    public FileCache(IFileStorable<T> store, ICacheable<T> cache)
    {
        _store = store;
        _cacheable = cache;
    }

    public FileCache(IFileStorable<T> store, ICacheable<T> cache, string basePath)
    {
        _store = store;
        _cacheable = cache;
        _store.BasePath = basePath;
        Directory.CreateDirectory(basePath);
    }

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

public class FileCache<T, TFileStore, TCacheable> : FileCache<T> where T : class where TFileStore : IFileStorable<T>, new() where TCacheable : ICacheable<T>, new()
{
    public FileCache() : base(new TFileStore(), new TCacheable())
    {
    }

    public FileCache(string path) : base(new TFileStore(), new TCacheable(), path)
    {
    }
}