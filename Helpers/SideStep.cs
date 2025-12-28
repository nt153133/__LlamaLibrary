using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers;

[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields")]
public class SideStep
{

    private static readonly LLogger Log = new("SideStepHelper", Color.FromRgb(255, 177, 109));

    private static object? _sideStep;

    private static Action? _loadAvoidanceFunction;
    private static Action<ulong, uint, Func<BattleCharacter, float, IEnumerable<AvoidInfo>>, float>? _addHandlerFunction;
    private static Func<ulong, uint, bool>? _removeHandlerFunction;

    static SideStep()
    {
        FindSideStep();
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
                _addHandlerFunction = (Action<ulong, uint, Func<BattleCharacter, float, IEnumerable<AvoidInfo>>, float>)Delegate.CreateDelegate(typeof(Action<ulong, uint, Func<BattleCharacter, float, IEnumerable<AvoidInfo>>, float>), _sideStep, "AddHandler");
                _removeHandlerFunction = (Func<ulong, uint, bool>)Delegate.CreateDelegate(typeof(Action<ulong, uint, Func<BattleCharacter, float, IEnumerable<AvoidInfo>>, float>), _sideStep, "RemoveHandler");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        Log.Information("SideStep found.");
    }

    public void LoadAvoidanceObjects()
    {
        if (_loadAvoidanceFunction == null)
        {
            Log.Information("SideStepHelper: No LoadAvoidanceObjects found.");
            return;
        }

        _loadAvoidanceFunction.Invoke();
    }

    public bool RemoveHandler(ulong type, uint key)
    {
        if (_removeHandlerFunction == null)
        {
            Log.Information("SideStepHelper: no RemoveHandler found.");
            return false;
        }
        return _removeHandlerFunction.Invoke(type, key);
    }

    public void AddHandler(ulong type, uint key, Func<BattleCharacter, float, IEnumerable<AvoidInfo>> handler, float range = float.NaN)
    {
        if (_addHandlerFunction == null)
        {
            Log.Information("SideStepHelper: no AddHandler found.");
            return;
        }
        _addHandlerFunction?.Invoke(type, key, handler, range);
    }
}