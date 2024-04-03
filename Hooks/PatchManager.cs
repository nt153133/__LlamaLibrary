using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Hooks;

public static class PatchManager
{
    private static readonly LLogger Log = new("PatchManager", Colors.Plum);
    public static List<AsmFunctionHook> Hooks { get; } = new();

    //public static List<DirectAsmPatch> DirectAsmPatches { get; } = new();
    public static bool Initialized { get; set; }

    public static bool Initialize()
    {
        Log.Information("Initializing PatchManager");
        if (Initialized)
        {
            return true;
        }

        Hooks.Add(new InstanceQuestDungeonHook());
        Hooks.Add(new InventoryUpdatePatch());

        foreach (var hook in Hooks)
        {
            if (hook.Initialize() == false)
            {
                Log.Error($"Failed to initialize hook {hook.Name}");
                return false;
            }

            //Log.Information($"Initialized hook {hook.Name}");
            hook.OnHookStateChange += args => Log.Information($"Hook {args.DisplayName} {(args.Enable ? "Enabled" : "Disabled")}");
            hook.Enable = hook.ShouldEnable;
        }

        Initialized = true;
        return true;
    }

    public static AsmFunctionHook? GetHook(string name)
    {
        foreach (var hook in Hooks)
        {
            if (hook.Name == name)
            {
                return hook;
            }
        }

        return null;
    }

    public static bool Enable(string name)
    {
        var hook = GetHook(name);
        if (hook == null)
        {
            return false;
        }

        hook.Enable = true;
        return true;
    }

    public static bool Disable(string name)
    {
        var hook = GetHook(name);
        if (hook == null)
        {
            return false;
        }

        hook.Enable = false;
        return true;
    }

    //Enable by type
    public static bool Enable<T>() where T : AsmFunctionHook
    {
        foreach (var hook in Hooks)
        {
            if (hook is T)
            {
                hook.Enable = true;
                return true;
            }
        }

        return false;
    }

    //Disable by type
    public static bool Disable<T>() where T : AsmFunctionHook
    {
        foreach (var hook in Hooks)
        {
            if (hook is T)
            {
                hook.Enable = false;
                return true;
            }
        }

        return false;
    }

    //Get by type
    public static T? GetHook<T>() where T : AsmFunctionHook
    {
        foreach (var hook in Hooks)
        {
            if (hook is T functionHook)
            {
                return functionHook;
            }
        }

        return null;
    }

    //Disable all
    public static void DisableAll()
    {
        if (!Initialized)
        {
            return;
        }

        foreach (var hook in Hooks.Where(hook => hook.Initialized))
        {
            hook.Enable = false;
        }

        /*foreach (var patch in DirectAsmPatches.Where(patch => patch.Initialized))
        {
            patch.Enable = false;
        }*/
    }
}

public class OnHookStateChangeArgs
{
    public OnHookStateChangeArgs(AsmFunctionHook hook)
    {
        Name = hook.Name;
        DisplayName = hook.DisplayName;
        Type = hook.GetType();
        Enable = hook.Enable;
    }

    /*public OnHookStateChangeArgs(DirectAsmPatch hook)
    {
        Name = hook.Name;
        DisplayName = hook.DisplayName;
        Type = hook.GetType();
        Enable = hook.Enable;
    }*/

    public string Name { get; }
    public string DisplayName { get; }
    public Type Type { get; }
    public bool Enable { get; }
}