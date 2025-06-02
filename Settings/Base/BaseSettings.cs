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

public abstract class BaseSettings : INotifyPropertyChanged
{
    private readonly LLogger _logger;
    private readonly DebounceDispatcher _saveDebounceDispatcher;
    private Dictionary<string, DebounceDispatcher>? _debounceDispatchers;
    private bool _loaded;

    public BaseSettings(string path)
    {
        Dispatcher = MainWpf.current.Dispatcher;
        _saveDebounceDispatcher = new DebounceDispatcher(SaveLocal);
        _logger = new LLogger($"{GetType().Name}", Colors.Peru, LogLevel.Debug);
        FilePath = path;
        LoadFrom(FilePath);
    }

    public BaseSettings(string path, Dispatcher? dispatcher)
    {
        Dispatcher = dispatcher;
        _saveDebounceDispatcher = new DebounceDispatcher(SaveLocal);
        _logger = new LLogger($"{GetType().Name}", Colors.Peru, LogLevel.Debug);
        FilePath = path;
        LoadFrom(FilePath);
    }

    public static string AssemblyDirectory => JsonSettings.AssemblyPath ?? throw new InvalidOperationException();

    public static string AssemblyPath => AssemblyDirectory;

    public static string SettingsPath => Path.Combine(AssemblyPath, "Settings");

    protected Dispatcher? Dispatcher { get; }

    [JsonIgnore]
    [Browsable(false)]
    public string FilePath { get; }

    public static string GetSettingsFilePath(params string[] subPathParts)
    {
        var list = new List<string> { SettingsPath };
        list.AddRange(subPathParts);
        return Path.Combine(list.ToArray());
    }

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

    public virtual void Save()
    {
        _saveDebounceDispatcher.Debounce(500, null!, DispatcherPriority.ApplicationIdle, Dispatcher);
    }

    private void SaveLocal(object? state = null)
    {
        SaveAs(FilePath);
    }

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

    public event PropertyChangedEventHandler? PropertyChanged;
}