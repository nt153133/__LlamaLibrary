using System;
using ff14bot.Interfaces;

namespace LlamaLibrary;

public interface ICompiledPlugin : IBotPlugin
{
    IExportedApi Api { get; }
}