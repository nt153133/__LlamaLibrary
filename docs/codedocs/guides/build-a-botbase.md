---
title: "Build a Botbase"
description: "Create a minimal RebornBuddy botbase on top of TemplateAsyncBotbase and LlamaLibrary helpers."
---

This guide shows the most direct way to consume LlamaLibrary as a botbase author. The goal is not just to compile, but to use the same runtime assumptions the library itself expects: service navigation, coroutine-based execution, and busy-state cleanup before work starts.

## Problem

Writing a RebornBuddy botbase from scratch means recreating the same boilerplate every time: navigation provider setup, logger creation, settings form wiring, and a coroutine root. `TemplateBotBase.cs` and `TemplateAsyncBotbase.cs` already solve that, so downstream projects should start there instead of subclassing `BotBase` directly.

## Solution

Use `TemplateAsyncBotbase` when your main loop is naturally asynchronous. It creates an `LLogger`, sets `Navigator.PlayerMover` to `SlideMover`, installs `ServiceNavigationProvider`, and maps your `Run()` method into both `AsyncRoot()` and the botbase root behavior.

<Steps>
<Step>
### Create the botbase class

```csharp
using System.Threading.Tasks;
using LlamaLibrary;

public sealed class ExampleBotbase : TemplateAsyncBotbase
{
    protected override string BotBaseName => "ExampleBotbase";

    protected override Task<bool> Run()
    {
        return Task.FromResult(true);
    }
}
```

</Step>
<Step>
### Add real work inside `Run()`

```csharp
using System.Threading.Tasks;
using LlamaLibrary;
using LlamaLibrary.Helpers;

public sealed class ExampleBotbase : TemplateAsyncBotbase
{
    protected override string BotBaseName => "ExampleBotbase";

    protected override async Task<bool> Run()
    {
        await GeneralFunctions.StopBusy();
        Log.Information("Ready to execute work.");
        return true;
    }
}
```

</Step>
<Step>
### Optional: provide a settings window

```csharp
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LlamaLibrary;

public sealed class ExampleBotbase : TemplateAsyncBotbase
{
    protected override string BotBaseName => "ExampleBotbase";
    protected override Type? SettingsForm => typeof(Form);

    protected override async Task<bool> Run()
    {
        await Task.Delay(10);
        return true;
    }
}
```

</Step>
</Steps>

Complete runnable example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary;
using LlamaLibrary.Helpers;
using LlamaLibrary.RemoteWindows;

public sealed class ExampleBotbase : TemplateAsyncBotbase
{
    protected override string BotBaseName => "ExampleBotbase";

    protected override async Task<bool> Run()
    {
        await GeneralFunctions.StopBusy();

        if (await GrandCompanyExchange.Instance.Open())
        {
            Log.Information($"GC rank group: {GrandCompanyExchange.Instance.GCRankGroup}");
            GrandCompanyExchange.Instance.Close();
        }

        return true;
    }
}
```

Why this works:

- `TemplateAsyncBotbase.Start()` in `TemplateAsyncBotbase.cs` sets up `SlideMover` and `ServiceNavigationProvider`.
- `GeneralFunctions.StopBusy()` clears several common blockers such as crafting, fishing, conversations, gathering windows, and targeting conflicts.
- `GrandCompanyExchange.Instance` is a singleton `RemoteWindow` wrapper, so you can treat it like a stable service instead of creating UI objects yourself.

If your botbase needs a synchronous `BotBase` root instead, `TemplateBotBase` exposes the same basic contract but derives from `BotBase` rather than `AsyncBotBase`.
