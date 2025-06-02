//!CompilerOption:AddRef:Clio.Localization.dll

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Windows.Media;
using ff14bot;
using GreyMagic;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using Newtonsoft.Json.Serialization;

namespace LlamaLibrary;

public static class AssemblyProxy
{
    private static readonly ConcurrentDictionary<string, Assembly> Assemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.Ordinal);
    private static readonly LLogger Log = new LLogger("AssemblyProxy", Colors.Bisque);
    private static bool _initialized;
    private static object _lock = new object();

    public static void Init()
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
        }

        try
        {
            AddAssembly("Newtonsoft", typeof(JsonContract).Assembly);
            AddAssembly("GreyMagic", typeof(ExternalProcessMemory).Assembly);
            AddAssembly("ff14bot", typeof(Core).Assembly);
            AddAssembly("LlamaLibrary", typeof(OffsetManager).Assembly);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }

        try
        {
            AddAssembly(Assembly.GetEntryAssembly()?.GetName().Name!, Assembly.GetEntryAssembly()!);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    public static void AddAssembly(string name, Assembly assembly)
    {
        //Add to dictionary, make sure it's not already there
        if (Assemblies.ContainsKey(name))
        {
            return;
        }

        Assemblies.TryAdd(name, assembly);
    }

    private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        if (Assemblies.TryGetValue(new AssemblyName(args.Name).Name ?? string.Empty, out var resolve))
        {
            return resolve;
        }

        if (!args.Name.Contains("resources"))
        {
            Log.Debug("Assembly not found: " + args.Name + "");
        }

        return null;
    }

    public static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        switch (assemblyName.Name)
        {
            case "Newtonsoft":
                return typeof(JsonContract).Assembly;
            case "GreyMagic":
                return Core.Memory.GetType().Assembly;
            case "ff14bot":
                return Core.Me.GetType().Assembly;
            case "LlamaLibrary":
                return typeof(OffsetManager).Assembly;
            /*case "Clio.Localization":
                return typeof(Infralution.Localization.Wpf.CultureManager).Assembly;*/
            default:
                return null!;
        }
    }
}