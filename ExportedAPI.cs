namespace LlamaLibrary
{
    /// <summary>
    /// Defines a contract for an API exported by a botbase or plugin, allowing other components to interact with it.
    /// </summary>
    public interface IExportedApi
    {
        /// <summary>
        /// Initializes the exported API.
        /// </summary>
        /// <returns><see langword="true"/> if initialization was successful; otherwise <see langword="false"/>.</returns>
        bool Init();
    }
}