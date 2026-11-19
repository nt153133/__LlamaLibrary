---
title: "Getting Started"
description: "Understand what LlamaLibrary adds to RebornBuddy and how to use it as the runtime and development foundation for FF XIV automation projects."
---

LlamaLibrary is a Windows-only `.NET 8` support library that fills the gaps RebornBuddy leaves open for botbases, plugins, quest behaviors, and UI automation in Final Fantasy XIV.

## The Problem

- RebornBuddy projects often need direct access to game UI state, client functions, and memory-backed data that the base SDK does not expose.
- Common automation work such as navigation, retainer interactions, Grand Company workflows, housing travel, and window automation gets reimplemented repeatedly across projects.
- Runtime deployment is unusual: the copy inside `RebornBuddy\QuestBehaviors\__LlamaLibrary` is what actually runs for end users, while the NuGet package is only a development reference.
- Game patches regularly invalidate offsets, so automation code needs a shared bootstrap layer that can discover and cache the current memory layout before higher-level helpers execute.

## The Solution

LlamaLibrary centralizes those responsibilities behind a small set of reusable abstractions: `LibraryClass` boots the library and warms offsets, `OffsetManager` discovers memory addresses, `RemoteWindow` and `RemoteAgents` wrap UI state, template botbase/plugin classes provide project scaffolding, and helper modules compose travel, shopping, retainer, and data workflows.

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
            GrandCompanyExchange.Instance.ChangeItemGroup(1);
            GrandCompanyExchange.Instance.Close();
        }

        return true;
    }
}
```

## Installation

<Callout type="info">LlamaLibrary is not a JavaScript package. Use the NuGet package for compile-time references, and keep the runtime copy installed under `RebornBuddy\QuestBehaviors\__LlamaLibrary` for actual execution.</Callout>

" "Manual runtime"]}>
<Tab value="dotnet CLI">

```bash
dotnet add package LlamaLibrary
```

</Tab>
<Tab value="Package Manager Console">

```powershell
Install-Package LlamaLibrary
```

</Tab>
<Tab value="Manual runtime">

```text
RebornBuddy
+-- QuestBehaviors
    +-- __LlamaLibrary
```

Clone or copy this repository into `QuestBehaviors\__LlamaLibrary`. If you are not relying on UpdateBuddy, also install companion botbases such as `LlamaUtilities` in the expected RebornBuddy folders.

</Tab>
</Tabs>

## Quick Start

The minimum working integration point is the template botbase base class plus one helper or window wrapper.

```csharp
using System.Threading.Tasks;
using LlamaLibrary;
using LlamaLibrary.Helpers;

public sealed class MinimalBotbase : TemplateAsyncBotbase
{
    protected override string BotBaseName => "MinimalBotbase";

    protected override async Task<bool> Run()
    {
        await GeneralFunctions.StopBusy();
        Log.Information("LlamaLibrary is ready.");
        return true;
    }
}
```

Expected output:

```text
[MinimalBotbase] Start
[MinimalBotbase] LlamaLibrary is ready.
```

The log line comes from the `TemplateAsyncBotbase` logger created in `TemplateAsyncBotbase.cs`, while `GeneralFunctions.StopBusy` handles several common blocking states before your workflow proceeds.

## Key Features

- `LibraryClass` initializes offsets and login/account state during RebornBuddy warmup.
- `TemplateBotBase`, `TemplateAsyncBotbase`, and `TemplatePlugin` provide reusable scaffolding for downstream projects.
- `RemoteWindow` and dozens of concrete window wrappers expose in-game UI automation through stable C# classes.
- `RemoteAgents` and helper modules add lower-level access to client-side data structures and actions.
- `BaseSettings<T>` and its character/account/home-world variants provide persisted JSON settings with automatic save behavior.
- High-level helpers cover navigation, world travel, housing travel, Grand Company workflows, retainers, data lookups, and more.

<Cards>
  <Card title="Architecture" href="/docs/architecture">See how bootstrapping, offsets, helpers, and UI wrappers fit together.</Card>
  <Card title="Core Concepts" href="/docs/runtime-bootstrap">Learn the runtime model, UI abstractions, settings, and task orchestration.</Card>
  <Card title="API Reference" href="/docs/api-reference/library-bootstrap-and-templates">Jump into signatures, source paths, and representative usage patterns.</Card>
</Cards>
