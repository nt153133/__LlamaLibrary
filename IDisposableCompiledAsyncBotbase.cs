using System;

namespace LlamaLibrary;

public interface IDisposableCompiledAsyncBotbase : ICompiledAsyncBotbase, IDisposable
{
    string EnglishName { get; }

    Version Version { get; }

    void OnShutdown();

    void Pulse();
}