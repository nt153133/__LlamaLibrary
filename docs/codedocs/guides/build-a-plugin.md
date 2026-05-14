---
title: "Build a Plugin"
description: "Use TemplatePlugin to create a RebornBuddy plugin with consistent logging, hook registration, and pulse behavior."
---

This guide focuses on plugin authors instead of botbase authors. `TemplatePlugin.cs` exists because plugins have a different lifecycle than botbases: they may need pulse flags, optional pulse-thread behavior, RebornBuddy tree hooks, and Lisbeth integration points while still exposing a normal settings button and logger.

## Problem

Plain `BotPlugin` implementations tend to accumulate repetitive lifecycle code. LlamaLibrary's `TemplatePlugin` centralizes logger creation, hook registration, pulse breakdown logic, and settings-window handling so each plugin can focus on its own behavior.

## Solution

Derive from `TemplatePlugin`, override the required identity properties, and then opt into the pieces you need. The source shows the base class maintaining pulse state, tracking hook registrations, and starting a UI-thread-backed timer when a pulse loop is required.

<Steps>
<Step>
### Create the plugin shell

```csharp
using LlamaLibrary;

public sealed class ExamplePlugin : TemplatePlugin
{
    public override string PluginName => "ExamplePlugin";
}
```

</Step>
<Step>
### Enable pulses if your plugin needs them

```csharp
using ff14bot.Behavior;
using LlamaLibrary;

public sealed class ExamplePlugin : TemplatePlugin
{
    public override string PluginName => "ExamplePlugin";
    protected override PulseFlags PulseFlags => PulseFlags.Objects | PulseFlags.Windows;
    protected override bool RequiresPulseThread => true;
}
```

</Step>
<Step>
### Add your own startup and shutdown behavior

```csharp
using LlamaLibrary;

public sealed class ExamplePlugin : TemplatePlugin
{
    public override string PluginName => "ExamplePlugin";

    public override void OnInitialize()
    {
        Log.Information("Plugin initialized.");
    }

    public override void OnShutdown()
    {
        Log.Information("Plugin shutting down.");
    }
}
```

</Step>
</Steps>

Complete example with a settings form and event subscription:

```csharp
using System;
using LlamaLibrary;
using LlamaLibrary.Events;

public sealed class ExamplePlugin : TemplatePlugin
{
    public override string PluginName => "ExamplePlugin";
    protected override Type? SettingsForm => typeof(System.Windows.Forms.Form);

    public override void OnInitialize()
    {
        LoginEvents.OnCharacterSwitched += OnCharacterSwitched;
        Log.Information("Initialized.");
    }

    public override void OnShutdown()
    {
        LoginEvents.OnCharacterSwitched -= OnCharacterSwitched;
    }

    private void OnCharacterSwitched(object? sender, CharacterSwitchedEventArgs args)
    {
        Log.Information($"Switched to {args.NewCharacterId}");
    }
}
```

This pattern matters because `TemplatePlugin` already knows how to add and remove tree hooks and Lisbeth hooks. Even if your first version does not need those integrations, inheriting from the template keeps your plugin aligned with the same conventions the rest of the library uses.

When you need a plugin API surface for other components, combine `TemplatePlugin` with `ICompiledPlugin` and `IExportedApi`. The interface types live in `ICompiledPlugin.cs` and `ExportedAPI.cs`.
