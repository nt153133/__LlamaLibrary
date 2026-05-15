namespace LlamaLibrary.Helpers.Cache.BaseClasses;

/// <summary>
/// Defines the contract for persisting and loading cached objects to and from disk.
/// Implementations control how objects are serialized (e.g. as PNG images) and where they are stored.
/// </summary>
/// <typeparam name="T">The type of object to store. Must be a reference type.</typeparam>
public interface IFileStorable<T> where T : class
{
    /// <summary>Gets the file extension (including the leading dot) used when persisting objects, e.g. <c>".png"</c>.</summary>
    string FileExtension { get; }

    /// <summary>Gets or sets the root directory where cached files are written and read.</summary>
    string BasePath { get; set; }

    /// <summary>
    /// Serializes <paramref name="obj"/> and writes it to disk under the given <paramref name="name"/>.
    /// </summary>
    /// <param name="obj">The object to persist.</param>
    /// <param name="name">The file name (without extension) used to identify the stored object.</param>
    void Save(T obj, string name);

    /// <summary>
    /// Reads a previously saved object from disk and deserializes it.
    /// </summary>
    /// <param name="name">The file name (without extension) of the stored object.</param>
    /// <returns>The deserialized object, or <see langword="null"/> if the file cannot be loaded.</returns>
    T? Load(string name);

    /// <summary>
    /// Checks whether a file for the given <paramref name="name"/> already exists on disk.
    /// </summary>
    /// <param name="name">The file name (without extension) to check.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>.</returns>
    bool Exists(string name);
}