using System.Windows.Threading;
using ff14bot;
using LlamaLibrary.Events;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Settings.Base;

/// <summary>
/// Provides a generic base class for settings that follow the singleton pattern.
/// </summary>
/// <typeparam name="T">The type of the settings class, which must inherit from <see cref="BaseSettings{T}"/> and have a parameterless constructor.</typeparam>
public class BaseSettings<T> : BaseSettings
    where T : BaseSettings<T>, new()
{
    private static T? _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettings{T}"/> class.
    /// The default settings file path is derived from the type name.
    /// </summary>
    public BaseSettings() : base(GetSettingsFilePath($"{typeof(T).Name}.json"))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettings{T}"/> class with a custom settings file path.
    /// </summary>
    /// <param name="settingsFilePath">The full path to the settings file.</param>
    public BaseSettings(string settingsFilePath) : base(settingsFilePath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettings{T}"/> class with a custom dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public BaseSettings(Dispatcher? dispatcher) : base(GetSettingsFilePath($"{typeof(T).Name}.json"), dispatcher)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettings{T}"/> class with a custom settings file path and dispatcher.
    /// </summary>
    /// <param name="settingsFilePath">The full path to the settings file.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public BaseSettings(string settingsFilePath, Dispatcher? dispatcher) : base(settingsFilePath, dispatcher)
    {
    }

    /// <summary>
    /// Gets or sets the singleton instance of the settings.
    /// If the instance has not been created, it will be initialized using the parameterless constructor.
    /// </summary>
    public static T Instance
    {
        get => _instance ??= new T();
        set => _instance = value;
    }

    /// <summary>
    /// Manually sets the singleton instance of the settings.
    /// </summary>
    /// <param name="instance">The settings instance to use.</param>
    public static void SetInstance(T? instance)
    {
        _instance = instance;
    }

    /// <summary>
    /// Clears the current singleton instance, forcing it to be reloaded or reinitialized on the next access.
    /// </summary>
    public static void ClearInstance()
    {
        SetInstance(null);
    }
}

/// <summary>
/// Provides a base class for settings that are unique to the current character.
/// Automatically handles instance cleanup when the character is switched.
/// </summary>
/// <typeparam name="T">The type of the settings class.</typeparam>
public class CharacterBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterBaseSettings{T}"/> class.
    /// The settings file is stored in a character-specific subdirectory.
    /// </summary>
    public CharacterBaseSettings() : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId():X16}", $"{typeof(T).Name}.json"))
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterBaseSettings{T}"/> class with a custom filename.
    /// </summary>
    /// <param name="fileName">The name of the settings file.</param>
    public CharacterBaseSettings(string fileName) : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId():X16}", fileName))
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterBaseSettings{T}"/> class with a custom filename and dispatcher.
    /// </summary>
    /// <param name="fileName">The name of the settings file.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public CharacterBaseSettings(string fileName, Dispatcher? dispatcher) : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId():X16}", fileName), dispatcher)
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterBaseSettings{T}"/> class with a custom dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public CharacterBaseSettings(Dispatcher? dispatcher) : base(GetSettingsFilePath($"{Core.Me.Name}_{Core.Me.PlayerId():X16}", $"{typeof(T).Name}.json"), dispatcher)
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }
}

/// <summary>
/// Provides a base class for settings that are unique to the current game account.
/// </summary>
/// <typeparam name="T">The type of the settings class.</typeparam>
public class AccountBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class.
    /// The settings file is stored in an account-specific subdirectory.
    /// </summary>
    public AccountBaseSettings() : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", $"{typeof(T).Name}.json"))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class with a custom dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public AccountBaseSettings(Dispatcher? dispatcher) : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", $"{typeof(T).Name}.json"), dispatcher)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class with a custom filename.
    /// </summary>
    /// <param name="fileName">The name of the settings file.</param>
    public AccountBaseSettings(string fileName) : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", fileName))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class with a custom filename and dispatcher.
    /// </summary>
    /// <param name="fileName">The name of the settings file.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public AccountBaseSettings(string fileName, Dispatcher? dispatcher) : base(GetSettingsFilePath($"Account_{LoginEvents.AccountId}", fileName), dispatcher)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class for a specific account ID and filename.
    /// </summary>
    /// <param name="accountId">The numeric ID of the game account.</param>
    /// <param name="fileName">The name of the settings file.</param>
    public AccountBaseSettings(int accountId, string fileName) : base(GetSettingsFilePath($"Account_{accountId}", fileName))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class for a specific account ID and dispatcher.
    /// </summary>
    /// <param name="accountId">The numeric ID of the game account.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public AccountBaseSettings(int accountId, Dispatcher? dispatcher) : base(GetSettingsFilePath($"Account_{accountId}", $"{typeof(T).Name}.json"), dispatcher)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class for a specific account ID.
    /// </summary>
    /// <param name="accountId">The numeric ID of the game account.</param>
    public AccountBaseSettings(int accountId) : base(GetSettingsFilePath($"Account_{accountId}", $"{typeof(T).Name}.json"))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountBaseSettings{T}"/> class for a specific account ID, filename, and dispatcher.
    /// </summary>
    /// <param name="accountId">The numeric ID of the game account.</param>
    /// <param name="fileName">The name of the settings file.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public AccountBaseSettings(int accountId, string fileName, Dispatcher? dispatcher) : base(GetSettingsFilePath($"Account_{accountId}", fileName), dispatcher)
    {
    }
}

/// <summary>
/// Provides a base class for settings that are unique to the current Home World (server).
/// Automatically handles instance cleanup when the character is switched.
/// </summary>
/// <typeparam name="T">The type of the settings class.</typeparam>
public class HomeWorldBaseSettings<T> : BaseSettings<T>
    where T : BaseSettings<T>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeWorldBaseSettings{T}"/> class.
    /// The settings file is stored in a Home World-specific subdirectory.
    /// </summary>
    public HomeWorldBaseSettings() : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", $"{typeof(T).Name}.json"))
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeWorldBaseSettings{T}"/> class with a custom dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public HomeWorldBaseSettings(Dispatcher? dispatcher) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", $"{typeof(T).Name}.json"), dispatcher)
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeWorldBaseSettings{T}"/> class with a custom filename.
    /// </summary>
    /// <param name="fileName">The name of the settings file.</param>
    public HomeWorldBaseSettings(string fileName) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", fileName))
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeWorldBaseSettings{T}"/> class with a custom filename and dispatcher.
    /// </summary>
    /// <param name="fileName">The name of the settings file.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public HomeWorldBaseSettings(string fileName, Dispatcher? dispatcher) : base(GetSettingsFilePath($"HomeWorld_{Core.Me.HomeWorld()}", fileName), dispatcher)
    {
        LoginEvents.OnCharacterSwitched += (_, _) => { ClearInstance(); };
    }
}