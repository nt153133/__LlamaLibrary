namespace LlamaLibrary
{
    public interface ICompiledAPIBotbase : ICompiledBotbase
    {
        IExportedApi Api { get; }
    }
}