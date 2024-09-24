using System.Windows.Threading;
using ff14bot;
using LlamaLibrary.Events;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Settings.Base;

public class BaseSettings<T> : BaseSettings
    where T : BaseSettings<T>, new()
{
    private static T? _instance;

    public BaseSettings() : base(GetSettingsFilePath($"{typeof(T).Name}.json"))
    {
    }

    public BaseSettings(string settingsFilePath) : base(settingsFilePath)
    {
    }

    public BaseSettings(Dispatcher dispatcher) : base(GetSettingsFilePath($"{typeof(T).Name}.json"), dispatcher)
    {
    }

    public BaseSettings(string settingsFilePath, Dispatcher dispatcher) : base(settingsFilePath, dispatcher)
    {
    }

    public static T Instance
    {
        get => _instance ??= new T();
        set => _instance = value;
    }

    public static void SetInstance(T? instance)
    {
        _instance = instance;
    }

    public static void ClearInstance()
    {
        SetInstance(null);
    }
}

public class CharacterBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    public CharacterBaseSettings() : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId()}", $"{typeof(T).Name}.json"))
    {
    }

    public CharacterBaseSettings(string fileName) : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId()}", fileName))
    {
    }

    public CharacterBaseSettings(string fileName, Dispatcher dispatcher) : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId()}", fileName), dispatcher)
    {
    }

    public CharacterBaseSettings(Dispatcher dispatcher) : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId()}", $"{typeof(T).Name}.json"), dispatcher)
    {
    }
}

public class AccountBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    public AccountBaseSettings() : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", $"{typeof(T).Name}.json"))
    {
    }

    public AccountBaseSettings(Dispatcher dispatcher) : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", $"{typeof(T).Name}.json"), dispatcher)
    {
    }

    public AccountBaseSettings(string fileName) : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", fileName))
    {
    }

    public AccountBaseSettings(string fileName, Dispatcher dispatcher) : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", fileName), dispatcher)
    {
    }

    public AccountBaseSettings(int accountId, string fileName) : base(GetSettingsFilePath($"Account_{accountId}", fileName))
    {
    }

    public AccountBaseSettings(int accountId, Dispatcher dispatcher) : base(GetSettingsFilePath($"Account_{accountId}", $"{typeof(T).Name}.json"), dispatcher)
    {
    }

    public AccountBaseSettings(int accountId) : base(GetSettingsFilePath($"Account_{accountId}", $"{typeof(T).Name}.json"))
    {
    }

    public AccountBaseSettings(int accountId, string fileName, Dispatcher dispatcher) : base(GetSettingsFilePath($"Account_{accountId}", fileName), dispatcher)
    {
    }
}

public class HomeWorldBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    public HomeWorldBaseSettings() : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", $"{typeof(T).Name}.json"))
    {
    }

    public HomeWorldBaseSettings(Dispatcher dispatcher) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", $"{typeof(T).Name}.json"), dispatcher)
    {
    }

    public HomeWorldBaseSettings(string fileName) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", fileName))
    {
    }

    public HomeWorldBaseSettings(string fileName, Dispatcher dispatcher) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", fileName), dispatcher)
    {
    }
}