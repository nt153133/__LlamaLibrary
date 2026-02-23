using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media;
using System.Reflection;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers;

[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields")]
public static class SideStep
{

    private static readonly LLogger Log = new("SideStepHelper", Color.FromRgb(255, 177, 109));

    private static object? _sideStep;

    private static Action? _loadAvoidanceFunction;
    private static Func<uint, bool>? _override;
    private static Func<uint, bool>? _removeOverride;

    static SideStep()
    {
        FindSideStep();
    }

    private static MethodInfo GetRemoveHandler(object pluginInstance)
    {
        var t = pluginInstance.GetType();
    
        var mi = t.GetMethod(
            "RemoveOverride",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: new[] { typeof(uint) },
            modifiers: null);
    
        if (mi == null)
            throw new MissingMethodException(t.FullName, "RemoveOverride(uint)");
    
        return mi;
    }
    
    private static void FindSideStep()
    {
        var loader = PluginManager.Plugins
            .FirstOrDefault(c => string.Equals(c.Plugin.Name, "SideStep", StringComparison.Ordinal));

        if (loader == null)
        {
            Log.Information("SideStepHelper: No SideStep found.");
            return;
        }

        _sideStep = loader.Plugin;


        if (_sideStep != null)
        {
            try
            {

                _loadAvoidanceFunction = (Action)Delegate.CreateDelegate(typeof(Action), _sideStep, "LoadAvoidanceObjects");
                _override = (Func<uint,bool>)Delegate.CreateDelegate(typeof(Func<uint, bool>), _sideStep, "Override");
                _removeOverride = (Func<uint,bool>)Delegate.CreateDelegate(typeof(Func<uint,bool>), _sideStep, "RemoveOverride");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        Log.Information("SideStep found.");
    }

    public static void LoadAvoidanceObjects()
    {
        if (_loadAvoidanceFunction == null)
        {
            Log.Information("SideStepHelper: No LoadAvoidanceObjects found.");
            return;
        }

        _loadAvoidanceFunction.Invoke();
    }

    public static bool RemoveHandler(uint key)
    {
        if (_removeOverride == null)
        {
            Log.Information("SideStepHelper: no RemoveHandler found.");
            return false;
        }
        return _removeOverride.Invoke(key);
    }

    public static bool Override(uint key)
    {
        if (_override == null)
        {
            Log.Information("SideStepHelper: no AddHandler found.");
            return false;
        }
        return _override.Invoke(key);
    }

}





