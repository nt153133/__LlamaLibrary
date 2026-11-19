---
title: "Library Bootstrap and Templates"
description: "API reference for the startup entry point, template botbases, template plugin, and compiled interface contracts."
---

This page covers the public entry and scaffolding types under the `LlamaLibrary` namespace. Source files: `LibraryClass.cs`, `TemplateBotBase.cs`, `TemplateAsyncBotbase.cs`, `TemplatePlugin.cs`, `ICompiledBotbase.cs`, `ICompiledAsyncBotbase.cs`, `ICompiledAPIBotbase.cs`, `ICompiledPlugin.cs`, and `ExportedAPI.cs`.

## `LibraryClass`

Import path: `using LlamaLibrary;`

Source: `LibraryClass.cs`

```csharp
public class LibraryClass : ILibrary
{
    public static bool SafeMode { get; private set; }
    public async Task<bool> PreOffsetWarmup();
    public Task<bool> PostOffsetWarmup();
}
```

| Member | Type | Default | Description |
|---|---|---|---|
| `SafeMode` | `bool` | `false` | Set when `-safemode` is present or when `Shift` + `Ctrl` are held during startup. |

`PreOffsetWarmup()` resolves safe mode, ensures trace logging, and calls `OffsetManager.InitLib()`. `PostOffsetWarmup()` finalizes offsets, records account and character metadata, and hooks `TreeRoot.OnStart`.

Example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary;

public async Task<bool> WarmLibraryAsync()
{
    var lib = new LibraryClass();
    return await lib.PreOffsetWarmup() && await lib.PostOffsetWarmup();
}
```

## `TemplateBotBase`

Import path: `using LlamaLibrary;`

Source: `TemplateBotBase.cs`

```csharp
public abstract class TemplateBotBase : BotBase
{
    protected abstract string BotBaseName { get; }
    protected virtual Type? SettingsForm { get; } = null;
    protected virtual Color LogColor { get; } = Colors.Aqua;
    protected abstract Task<bool> Run();
    protected virtual LLogger GetLogger();
}
```

`Start()` configures `SlideMover` and `ServiceNavigationProvider`. `Stop()` disposes the provider. `OnButtonPress()` instantiates the configured settings form if one exists.

## `TemplateAsyncBotbase`

Import path: `using LlamaLibrary;`

Source: `TemplateAsyncBotbase.cs`

```csharp
public abstract class TemplateAsyncBotbase : AsyncBotBase
{
    protected abstract string BotBaseName { get; }
    protected virtual Type? SettingsForm { get; } = null;
    protected virtual Color LogColor { get; } = Colors.Aqua;
    protected abstract Task<bool> Run();
}
```

Use this when your botbase logic is already coroutine- or task-oriented. `AsyncRoot()` simply returns `Run()`.

## `TemplatePlugin`

Import path: `using LlamaLibrary;`

Source: `TemplatePlugin.cs`

Representative public surface:

```csharp
public abstract class TemplatePlugin : BotPlugin, IBotPlugin
{
    public abstract string PluginName { get; }
    protected virtual Color LogColor { get; } = Colors.CornflowerBlue;
    protected virtual Type? SettingsForm { get; } = null;
    protected virtual bool RequiresPulseThread { get; } = false;
    protected virtual PulseFlags PulseFlags { get; } = PulseFlags.None;
    public static int FPS { get; }
    public static ConcurrentBag<PulseFlags> BreakDownPulseFlags(PulseFlags flags);
}
```

The class also manages hook registration and a dispatcher-backed pulse timer internally.

## Compiled contracts

Import path: `using LlamaLibrary;`

Sources: `ICompiledBotbase.cs`, `ICompiledAsyncBotbase.cs`, `ICompiledAPIBotbase.cs`, `ICompiledPlugin.cs`, `ExportedAPI.cs`

```csharp
public interface IExportedApi
{
    bool Init();
}

public interface ICompiledBotbase
{
    string Name { get; }
    PulseFlags PulseFlags { get; }
    bool RequiresProfile { get; }
    bool WantButton { get; }
    bool IsAutonomous { get; }
    Composite GetRoot();
    void Start();
    void Stop();
    void OnButtonPress();
    void Initialize();
}

public interface ICompiledAsyncBotbase
{
    string Name { get; }
    PulseFlags PulseFlags { get; }
    bool RequiresProfile { get; }
    bool WantButton { get; }
    bool IsAutonomous { get; }
    Task AsyncRoot();
    void Start();
    void Stop();
    void OnButtonPress();
    void SetParameters(string param);
    void Initialize();
}
```

These interfaces are useful when a project loads a botbase or plugin dynamically and needs a stable shared contract instead of a concrete type reference.
