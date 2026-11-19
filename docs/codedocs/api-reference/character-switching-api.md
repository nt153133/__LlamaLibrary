---
title: "Character Switching API"
description: "API reference for CharacterAvatar, CharacterSwitcher, CharacterTask, CharacterTaskManager, and task result types."
---

The character-switching API lives entirely under `using LlamaLibrary.Helpers.CharacterSwitching;` and is concentrated in the `Helpers/CharacterSwitching/` folder.

## `CharacterAvatar`

Source: `Helpers/CharacterSwitching/CharacterAvatar.cs`

```csharp
public class CharacterAvatar
{
    public string Name { get; set; } = string.Empty;
    public WorldDCGroupType DC { get; set; } = WorldDCGroupType.Invalid;
    public World HomeWorld { get; set; } = World.SetMe;
    public ulong CharacterId { get; set; }
    public bool Censored { get; set; } = false;

    public string FullName { get; }
    public string CensoredName { get; }
    public string DisplayName { get; }
    public string Server { get; }
}
```

## `CharacterSwitcher`

Source: `Helpers/CharacterSwitching/CharacterSwitcher.cs`

```csharp
public static class CharacterSwitcher
{
    public static ReadOnlyObservableCollection<CharacterTask> CharacterTasks { get; }

    public delegate Task<bool> FillCharacterListDelegate();
    public delegate List<CharacterAvatar> GetCharacterListDelegate();
    public delegate Task<bool> SwitchCharacterByAvatarDelegate(CharacterAvatar characterAvatar);
    public delegate Task<bool> SwitchCharacterDelegate(long characterId);

    public static FillCharacterListDelegate? FillCharacterListAsync { get; set; }
    public static SwitchCharacterDelegate? SwitchCharacterAsync { get; set; }
    public static SwitchCharacterByAvatarDelegate? SwitchCharacterByAvatarAsync { get; set; }
    public static GetCharacterListDelegate? GetCharacterList { get; set; }

    public static bool IsCharacterSwitchingAvailable();
    public static bool AddCharacterTask(CharacterTask task);
    public static bool RemoveCharacterTask(CharacterTask task);
}
```

## `CharacterTask`

Source: `Helpers/CharacterSwitching/CharacterTask.cs`

```csharp
public abstract class CharacterTask
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string ProvidingBotbaseName { get; }
    public virtual string Parameter { get; set; } = "";
    public virtual IEnumerable<string> ParameterOptions { get; } = Array.Empty<string>();
    public virtual ValidationRule ParameterValidationRule { get; } = new DefaultValidationRule();
    public virtual bool RequiresParameter => false;

    public bool IsRunning { get; }
    public bool LastRunWasSuccessful { get; }
    public DateTime? LastRun { get; }
    public CharacterTaskResultStatus? LastRunStatus { get; }
    public string LastRunStatusMessage { get; }
    public TimeSpan LastRunDuration { get; }

    public abstract Task<(bool canRun, string reason)> CheckAvailabilityAsync();
    protected abstract Task<CharacterTaskResult> ExecuteAsync(string? parameter = null);
    public virtual Task<CharacterTaskResult> RunAsync();
}
```

## `CharacterTaskManager`

Source: `Helpers/CharacterSwitching/CharacterTaskManager.cs`

```csharp
public static class CharacterTaskManager
{
    public static ReadOnlyObservableCollection<CharacterTask> Tasks { get; }
    public static bool AddTask(CharacterTask task);
    public static bool RemoveTask(CharacterTask task);
    public static bool RemoveTask(string providingBotbaseName, string taskName);
    public static IReadOnlyList<CharacterTask> GetTaskSnapshot();
    public static void ClearTasks();
}
```

## Result types

Sources: `Helpers/CharacterSwitching/CharacterTaskResult.cs`, `Helpers/CharacterSwitching/CharacterTaskResultStatus.cs`

```csharp
public sealed class CharacterTaskResult
{
    public CharacterTaskResult(bool wasSuccessful, CharacterTaskResultStatus status, string message);
    public bool WasSuccessful { get; }
    public CharacterTaskResultStatus Status { get; }
    public string Message { get; }
}

public enum CharacterTaskResultStatus
{
    Success,
    FailedCannotContinue,
    FailedUnavailable,
    FailedNavigation,
    FailedOther,
    FailedException
}
```

Example:

```csharp
using System.Threading.Tasks;
using LlamaLibrary.Helpers.CharacterSwitching;

public async Task<bool> RunTaskAsync(CharacterTask task)
{
    var result = await task.RunAsync();
    return result.WasSuccessful;
}
```
