namespace LlamaLibrary
{
    /// <summary>
    /// Defines a botbase contract that exposes an <see cref="IExportedApi"/> for external interaction.
    /// </summary>
    public interface ICompiledAPIBotbase : ICompiledBotbase
    {
        /// <summary>
        /// Gets the exported API instance provided by this botbase.
        /// </summary>
        IExportedApi Api { get; }
    }
}