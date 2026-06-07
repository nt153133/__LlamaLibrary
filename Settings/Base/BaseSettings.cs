using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using ff14bot.Forms.ugh;
using ff14bot.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using Newtonsoft.Json;
using LogLevel = LlamaLibrary.Logging.LogLevel;

namespace LlamaLibrary.Settings.Base;

using LogLevel = LogLevel;

/// <summary>
/// Provides a base class for persistent settings in LlamaLibrary.
/// Handles JSON serialization, debounced saving, and property change notification with UI thread synchronization.
/// </summary>
public abstract class BaseSettings : INotifyPropertyChanged
{
    private readonly LLogger _logger;
    private readonly DebounceDispatcher _saveDebounceDispatcher;
    private Dictionary<string, DebounceDispatcher>? _debounceDispatchers;
    private bool _loaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettings"/> class using the specified file path.
    /// Uses the main WPF dispatcher for synchronization.
    /// </summary>
    /// <param name="path">The full path to the settings file.</param>
    public BaseSettings(string path)
    {
        Dispatcher = MainWpf.current.Dispatcher;
        _saveDebounceDispatcher = new DebounceDispatcher(SaveLocal);
        _logger = new LLogger($"{GetType().Name}", Colors.Peru, LogLevel.Debug);
        FilePath = path;
        LoadFrom(FilePath);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettings"/> class using the specified file path and dispatcher.
    /// </summary>
    /// <param name="path">The full path to the settings file.</param>
    /// <param name="dispatcher">The dispatcher to use for UI thread synchronization.</param>
    public BaseSettings(string path, Dispatcher? dispatcher)
    {
        Dispatcher = dispatcher;
        _saveDebounceDispatcher = new DebounceDispatcher(SaveLocal);
        _logger = new LLogger($"{GetType().Name}", Colors.Peru, LogLevel.Debug);
        FilePath = path;
        LoadFrom(FilePath);
    }

    /// <summary>
    /// Gets the directory path where the LlamaLibrary assembly is located.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the assembly path cannot be determined.</exception>
    public static string AssemblyDirectory => JsonSettings.AssemblyPath ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the path to the LlamaLibrary assembly.
    /// </summary>
    public static string AssemblyPath => AssemblyDirectory;

    /// <summary>
    /// Gets the default directory path for LlamaLibrary settings files.
    /// </summary>
    public static string SettingsPath => Path.Combine(AssemblyPath, "Settings");

    /// <summary>
    /// Gets the dispatcher used for synchronizing property changes and UI updates.
    /// </summary>
    protected Dispatcher? Dispatcher { get; }

    /// <summary>
    /// Gets the full file path where these settings are stored.
    /// </summary>
    [JsonIgnore]
    [Browsable(false)]
    public string FilePath { get; }

    /// <summary>
    /// Constructs a full settings file path by combining the base <see cref="SettingsPath"/> with the provided sub-path parts.
    /// </summary>
    /// <param name="subPathParts">The parts of the sub-path to combine.</param>
    /// <returns>A string representing the combined file path.</returns>
    public static string GetSettingsFilePath(params string[] subPathParts)
    {
        var list = new List<string> { SettingsPath };
        list.AddRange(subPathParts);
        return Path.Combine(list.ToArray());
    }

    /// <summary>
    /// Loads the settings from the specified JSON file.
    /// Initializes default values and sets up property change listeners for collections and nested objects.
    /// </summary>
    /// <param name="file">The path to the settings file to load.</param>
    protected void LoadFrom(string file)
    {
        var properties = GetType().GetProperties();

        foreach (var propertyInfo in properties)
        {
            _debounceDispatchers ??= new Dictionary<string, DebounceDispatcher>(StringComparer.Ordinal);
            _debounceDispatchers[propertyInfo.Name] = new DebounceDispatcher(OnPropertyChangedDebounce);

            var customAttributes = propertyInfo.GetCustomAttributes<DefaultValueAttribute>(true).ToList();

            if (customAttributes.Count == 0)
            {
                continue;
            }

            foreach (var custom in customAttributes)
            {
                if (propertyInfo.GetSetMethod() != null)
                {
                    propertyInfo.SetValue(this, custom.Value, null);
                }
            }
        }

        if (File.Exists(file))
        {
            Dispatcher?.Invoke(() =>
            {
                try
                {
                    JsonConvert.PopulateObject(File.ReadAllText(file), this);
                }
                catch (Exception e)
                {
                    _logger.Exception(e);
                }
            });
        }

        foreach (var propertyInfo in properties)
        {
            //Check if property is an observable collection
            if (typeof(INotifyCollectionChanged).IsAssignableFrom(propertyInfo.PropertyType))
            {
                //_logger.Debug($"Property {propertyInfo.Name} is an INotifyCollectionChanged");
                //Set list changed event to trigger on property change
                if (propertyInfo.GetValue(this) is INotifyCollectionChanged collection)
                {
                    if (propertyInfo.PropertyType.IsGenericType && typeof(INotifyPropertyChanged).IsAssignableFrom(propertyInfo.PropertyType.GenericTypeArguments[0]))
                    {
                        //_logger.Debug($"Property {propertyInfo.Name} is an INotifyPropertyChanged with generic type {propertyInfo.PropertyType.GenericTypeArguments[0]}");

                        //loop through all items in collection and add property changed event
                        foreach (var item in (IEnumerable<object>)collection)
                        {
                            if (item is INotifyPropertyChanged notifyPropertyChanged)
                            {
                                notifyPropertyChanged.PropertyChanged += OnNotifyPropertyChangedOnPropertyChanged;
                            }
                        }

                        collection.CollectionChanged += (_, args) =>
                        {
                            OnPropertyChanged(propertyInfo.Name);

                            if (args is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
                            {
                                foreach (var item in args.NewItems)
                                {
                                    if (item is INotifyPropertyChanged notifyPropertyChanged)
                                    {
                                        notifyPropertyChanged.PropertyChanged += OnNotifyPropertyChangedOnPropertyChanged;
                                    }
                                }
                            }

                            if (args is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
                            {
                                foreach (var item in args.OldItems)
                                {
                                    if (item is INotifyPropertyChanged notifyPropertyChanged)
                                    {
                                        notifyPropertyChanged.PropertyChanged -= OnNotifyPropertyChangedOnPropertyChanged;
                                    }
                                }
                            }
                        };

                        void OnNotifyPropertyChangedOnPropertyChanged(object? o, PropertyChangedEventArgs eventArgs)
                        {
                            OnPropertyChanged(propertyInfo.Name);
                        }
                    }
                    else
                    {
                        collection.CollectionChanged += (_, _) => { OnPropertyChanged(propertyInfo.Name); };
                    }
                }
                else
                {
                    _logger.Error($"Property {propertyInfo.Name} is not an INotifyCollectionChanged it is {propertyInfo.PropertyType}");
                }
            }

            if (typeof(IBindingList).IsAssignableFrom(propertyInfo.PropertyType))
            {
                //Set list changed event to trigger on property change
                var collection = propertyInfo.GetValue(this) as IBindingList;
                if (collection != null)
                {
                    collection.ListChanged += (_, _) => { OnPropertyChanged(propertyInfo.Name); };
                }
                else
                {
                    _logger.Error($"Property {propertyInfo.Name} is not an IBindingList it is {propertyInfo.PropertyType}");
                }
            }

            if (typeof(INotifyPropertyChanged).IsAssignableFrom(propertyInfo.PropertyType))
            {
                var notifyPropertyChanged = propertyInfo.GetValue(this) as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged += (_, _) => { OnPropertyChanged(propertyInfo.Name); };
                }
                else
                {
                    _logger.Error($"Property {propertyInfo.Name} is not an INotifyPropertyChanged it is {propertyInfo.PropertyType}");
                }
            }
        }

        _loaded = true;
        if (file != FilePath || !File.Exists(file))
        {
            Save();
        }
    }

    /// <summary>
    /// Triggers a debounced save of the settings to the current <see cref="FilePath"/>.
    /// The save is delayed by 500ms to consolidate multiple rapid changes.
    /// </summary>
    public virtual void Save()
    {
        _saveDebounceDispatcher.Debounce(500, null!, DispatcherPriority.ApplicationIdle, Dispatcher);
    }

    private void SaveLocal(object? state = null)
    {
        SaveAs(FilePath);
    }

    /// <summary>
    /// Synchronously saves the settings to the specified file path in JSON format.
    /// Creates the target directory if it does not exist.
    /// </summary>
    /// <param name="file">The path to the file where settings should be saved.</param>
    public void SaveAs(string file)
    {
        try
        {
            if (!_loaded)
            {
                _logger.Information("Not loaded yet");
                return;
            }

            if (!File.Exists(file))
            {
                var directoryName = Path.GetDirectoryName(file);
                if (directoryName != null && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
            }

            File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        catch (Exception e)
        {
            _logger.Exception(e);
        }
    }

    private void OnPropertyChangedDebounce(object? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((string)(propertyName ?? string.Empty)));
        Save();
    }

    /// <summary>
    /// Triggers a debounced property change notification and schedules a save operation.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null)
        {
            return;
        }

        if (!_loaded)
        {
            return;
        }

        _debounceDispatchers![propertyName].Debounce(50, propertyName, DispatcherPriority.ApplicationIdle, Dispatcher);
    }

    /// <summary>
    /// Sets a field to a new value and triggers property change notification if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">A reference to the field to be updated.</param>
    /// <param name="value">The new value for the field.</param>
    /// <param name="propertyName">The name of the property (automatically populated via <see cref="CallerMemberNameAttribute"/>).</param>
    /// <returns><see langword="true"/> if the value was changed and notification was triggered; otherwise <see langword="false"/>.</returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
}