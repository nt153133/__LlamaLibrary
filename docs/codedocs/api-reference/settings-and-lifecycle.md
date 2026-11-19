---
title: "Settings and Lifecycle"
description: "API reference for LoginEvents, event args, BaseSettings variants, LlamaLibrarySettings, and path helpers."
---

This page documents the runtime lifecycle and settings APIs that most consumer projects will touch directly. Source files: `Events/LoginEvents.cs`, `Events/LoginEventArgs.cs`, `Settings/Base/BaseSettings.cs`, `Settings/Base/BaseSettingsTyped.cs`, `Settings/LlamaLibrarySettings.cs`, and `Helpers/JsonHelper.cs`.

## `LoginEvents`

Import path: `using LlamaLibrary.Events;`

Source: `Events/LoginEvents.cs`

```csharp
public static class LoginEvents
{
    public static ulong PreviousCharacterId { get; internal set; }
    public static ulong LastKnownCharacterId { get; internal set; }
    public static ulong AccountId { get; internal set; }

    public static event EventHandler<LoginEventArgs>? OnLogin;
    public static event EventHandler<DisconnectedEventArgs>? OnDisconnected;
    public static event EventHandler<CharacterSwitchedEventArgs>? OnCharacterSwitched;

    public static void InvokeOnLogin(bool force = false);
    public static void InvokeOnDisconnected(bool force = false);
    public static void InvokeOnCharacterSwitched(bool force = false);
    public static void UpdateInfo();
}
```

These events are debounced unless `force` is `true`.

## Event argument types

Import path: `using LlamaLibrary.Events;`

Source: `Events/LoginEventArgs.cs`

```csharp
public class LoginEventArgs : EventArgs
{
    public ulong AccountId { get; }
    public ulong LastKnownCharacterId { get; }
    public bool IsInGame { get; }
}

public class CharacterSwitchedEventArgs : LoginEventArgs
{
    public ulong NewCharacterId { get; }
}

public class DisconnectedEventArgs : LoginEventArgs
{
}
```

## Settings base types

Import path: `using LlamaLibrary.Settings.Base;`

Sources: `Settings/Base/BaseSettings.cs`, `Settings/Base/BaseSettingsTyped.cs`

```csharp
public abstract class BaseSettings : INotifyPropertyChanged
{
    public string FilePath { get; }
    public static string AssemblyDirectory { get; }
    public static string AssemblyPath { get; }
    public static string SettingsPath { get; }
    public static string GetSettingsFilePath(params string[] subPathParts);
    public virtual void Save();
    public void SaveAs(string file);
}

public class BaseSettings<T> : BaseSettings where T : BaseSettings<T>, new()
{
    public static T Instance { get; set; }
    public static void SetInstance(T? instance);
    public static void ClearInstance();
}
```

Additional typed variants:

```csharp
public class CharacterBaseSettings<T> : BaseSettings<T> where T : BaseSettings<T>, new()
public class AccountBaseSettings<T> : BaseSettings<T> where T : BaseSettings<T>, new()
public class HomeWorldBaseSettings<T> : BaseSettings<T> where T : BaseSettings<T>, new()
```

## `LlamaLibrarySettings`

Import path: `using LlamaLibrary.Settings;`

Source: `Settings/LlamaLibrarySettings.cs`

```csharp
public class LlamaLibrarySettings : BaseSettings<LlamaLibrarySettings>
{
    public int LastRevision { get; set; }
    public bool DisableInventoryHook { get; set; }
    public bool TempDisableInventoryHook { get; set; }
}
```

| Property | Type | Default | Description |
|---|---|---|---|
| `LastRevision` | `int` | `0` | Stored revision marker for the library. |
| `DisableInventoryHook` | `bool` | `false` | Persistent opt-out for the inventory hook. |
| `TempDisableInventoryHook` | `bool` | `false` | Temporary runtime opt-out for the inventory hook. |

## `JsonHelper`

Import path: `using LlamaLibrary.Helpers;`

Source: `Helpers/JsonHelper.cs`

```csharp
public static class JsonHelper
{
    public static string UniqueCharacterSettingsDirectory { get; }
    public static string HomeWorldSettingsDirectory { get; }
    public static string DataCenterSettingsDirectory { get; }
}
```

Example:

```csharp
using LlamaLibrary.Events;
using LlamaLibrary.Settings;

public void DisableInventoryHookForCurrentSession()
{
    LlamaLibrarySettings.Instance.TempDisableInventoryHook = true;
    LoginEvents.UpdateInfo();
}
```
