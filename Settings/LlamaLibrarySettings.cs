using System.ComponentModel;
using LlamaLibrary.Settings.Base;

namespace LlamaLibrary.Settings;

public class LlamaLibrarySettings : BaseSettings<LlamaLibrarySettings>
{
    private int _lastRevision;
    private bool _disableInventoryHook;
    private bool _tempDisableInventoryHook;

    public int LastRevision
    {
        get => _lastRevision;
        set => SetField(ref _lastRevision, value);
    }

    [DefaultValue(false)]
    public bool DisableInventoryHook
    {
        get => _disableInventoryHook;
        set => SetField(ref _disableInventoryHook, value);
    }

    [DefaultValue(false)]
    public bool TempDisableInventoryHook
    {
        get => _tempDisableInventoryHook;
        set => SetField(ref _tempDisableInventoryHook, value);
    }
}