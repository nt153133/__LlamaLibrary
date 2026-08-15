//!CompilerOption:AddRef:Clio.Localization.dll

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using GreyMagic;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using Newtonsoft.Json.Serialization;

namespace LlamaLibrary;

/// <summary>
/// Provides a proxy for resolving and loading assemblies within the RebornBuddy AppDomain.
/// This ensures that dependencies like <c>Newtonsoft.Json</c> and <c>GreyMagic</c> are correctly resolved
/// even when loaded from different contexts or versions.
/// </summary>
public static class AssemblyProxy
{
    private static readonly ConcurrentDictionary<string, Assembly> Assemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.Ordinal);
    private static readonly LLogger Log = new LLogger("AssemblyProxy", Colors.Bisque);
    private static bool _initialized;
    private static readonly object Lock = new object();

    /// <summary>
    /// Initializes the assembly proxy and registers core assemblies for resolution.
    /// Also hooks into the <see cref="AppDomain.AssemblyResolve"/> event.
    /// </summary>
    public static void Init()
    {
        lock (Lock)
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

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            var entryAssemblyName = entryAssembly.GetName().Name;
            if (!string.IsNullOrEmpty(entryAssemblyName))
            {
                AddAssembly(entryAssemblyName, entryAssembly);
            }
        }

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    /// <summary>
    /// Registers an assembly with the proxy for later resolution.
    /// </summary>
    /// <param name="name">The short name of the assembly (e.g., "Newtonsoft").</param>
    /// <param name="assembly">The <see cref="Assembly"/> instance to register.</param>
    public static void AddAssembly(string name, Assembly assembly)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        Assemblies.TryAdd(name, assembly);
    }

    private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var requested = new AssemblyName(args.Name);
        var simpleName = requested.Name ?? string.Empty;
        if (Assemblies.TryGetValue(simpleName, out var resolve))
        {
            return resolve;
        }

        if (!args.Name.Contains("resources"))
        {
            ReportIfNeverLoaded(simpleName, requested, args.RequestingAssembly?.GetName());
        }

        return null;
    }

    // How long to wait before deciding a resolve miss was a real failure. Other
    // AssemblyResolve handlers run after ours (packed/embedded resolvers, sidecar
    // probing), so a miss here usually still succeeds a moment later.
    private const int MissConfirmDelayMs = 3000;
    private static readonly ConcurrentDictionary<string, byte> PendingMisses = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

    private static void ReportIfNeverLoaded(string simpleName, AssemblyName requested, AssemblyName? requester)
    {
        if (simpleName.Length == 0 || !PendingMisses.TryAdd(simpleName, 0))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(MissConfirmDelayMs).ConfigureAwait(false);
                if (!IsLoaded(simpleName))
                {
                    var message = "Assembly not found: " + Describe(requested)
                                  + " (requested by " + (requester == null ? "unknown assembly" : Describe(requester)) + ")";
                    if (requester?.Name != null && requester.Name.StartsWith("ScriptManager.IronPython", StringComparison.OrdinalIgnoreCase))
                    {
                        // IronPython tries Assembly.Load for every dotted import path
                        // (namespaces included) before falling back to scanning loaded
                        // assemblies, so these misses are structural, not failures.
                        Log.Verbose(message);
                    }
                    else
                    {
                        Log.Debug(message);
                    }
                }
            }
            catch
            {
                // Diagnostics only; never let the watcher surface an exception.
            }
            finally
            {
                PendingMisses.TryRemove(simpleName, out _);
            }
        });
    }

    // Culture and public key token are noise in a load-failure report; the simple
    // name plus version is what identifies the assembly a caller went looking for.
    private static string Describe(AssemblyName assemblyName)
    {
        var name = assemblyName.Name ?? assemblyName.FullName;
        return assemblyName.Version == null ? name : name + " v" + assemblyName.Version;
    }

    private static bool IsLoaded(string simpleName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (string.Equals(assembly.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A manual assembly resolution handler that redirects core library requests to the correct instances.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event data containing the name of the assembly to resolve.</param>
    /// <returns>The resolved <see cref="Assembly"/>, or <see langword="null"/> if resolution fails.</returns>
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
