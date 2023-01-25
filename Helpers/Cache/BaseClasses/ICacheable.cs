namespace LlamaLibrary.Helpers.Cache.BaseClasses;

public interface ICacheable<out T> where T : class
{
    public T? CacheMiss(string key);
}
