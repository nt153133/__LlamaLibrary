using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.LocationTracking;
using LlamaLibrary.Logging;

// ReSharper disable MemberCanBePrivate.Global

namespace LlamaLibrary.Chores;

public static class ChoreManager
{
    private static LLogger Log { get; } = new("ChoreManager");
    private static ConcurrentDictionary<string, Chore> Chores { get; } = new(StringComparer.Ordinal);
    public static List<Chore> ChoreList => Chores.Values.ToList();

    public static void AddChore(Chore chore)
    {
        Chores.AddOrUpdate(chore.Name, chore, (_, _) => chore);
    }

    public static void RemoveChore(Chore chore)
    {
        Chores.TryRemove(chore.Name, out _);
    }

    public static void AddChores(IEnumerable<Chore> chores)
    {
        foreach (var chore in chores)
        {
            AddChore(chore);
        }
    }

    public static void RemoveChores(IEnumerable<Chore> chores)
    {
        foreach (var chore in chores)
        {
            RemoveChore(chore);
        }
    }

    public static void AddChores(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Chore)));
        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is Chore chore)
            {
                AddChore(chore);
            }
        }
    }

    public static void RemoveChores(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Chore)));
        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is Chore chore)
            {
                RemoveChore(chore);
            }
        }
    }

    public static async Task<bool> RunChores(bool returnAfter = true)
    {
        var ran = false;
        var location = new LocationTracker();

        await GeneralFunctions.StopBusy();

        foreach (var chore in ChoreList.OrderBy(i => i.Priority))
        {
            if (!await chore.Check())
            {
                continue;
            }

            Log.Information($"Running Chore: {chore.Name}");
            await chore.DoWork();
            ran = true;
        }

        if (!returnAfter || !ran)
        {
            return ran;
        }

        Log.Information("Returning to previous location");
        if (!await location.GoBack())
        {
            Log.Error("Failed to return to previous location");
        }

        return ran;
    }
}