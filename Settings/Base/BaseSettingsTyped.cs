using ff14bot;
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

    public static T Instance
    {
        get => _instance ??= new T();
        set => _instance = value;
    }

    public static void SetInstance(T instance)
    {
        _instance = instance;
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
}

public class AccountBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    public AccountBaseSettings() : base(GetSettingsFilePath($"Account_{Core.Me.AccountId()}", $"{typeof(T).Name}.json"))
    {
    }

    public AccountBaseSettings(string fileName) : base(GetSettingsFilePath($"Account_{Core.Me.AccountId()}", fileName))
    {
    }

    public AccountBaseSettings(int accountId, string fileName) : base(GetSettingsFilePath($"Account_{accountId}", fileName))
    {
    }

    public AccountBaseSettings(int accountId) : base(GetSettingsFilePath($"Account_{accountId}", $"{typeof(T).Name}.json"))
    {
    }
}

public class HomeWorldBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    public HomeWorldBaseSettings() : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", $"{typeof(T).Name}.json"))
    {
    }

    public HomeWorldBaseSettings(string fileName) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", fileName))
    {
    }
}