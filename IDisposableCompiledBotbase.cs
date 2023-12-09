using System;

namespace LlamaLibrary;

public interface IDisposableCompiledBotbase : ICompiledBotbase, IDisposable
{
    string EnglishName { get; }

    Version Version { get; }

    void OnShutdown();

    void Pulse();
}