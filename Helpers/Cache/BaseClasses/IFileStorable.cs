namespace LlamaLibrary.Helpers.Cache.BaseClasses;

public interface IFileStorable<T> where T : class
{
    string FileExtension { get; }
    string BasePath { get; set; }
    void Save(T obj, string name);
    T? Load(string name);
    bool Exists(string name);
}