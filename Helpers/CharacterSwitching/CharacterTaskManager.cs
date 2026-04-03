using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Provides a central registry for character tasks that can be shared across libraries.
/// </summary>
public static class CharacterTaskManager
{
    private static readonly object SyncRoot = new();
    private static readonly ConcurrentDictionary<string, CharacterTask> TaskMap = new(StringComparer.Ordinal);
    private static readonly ObservableCollection<CharacterTask> MutableTasks = new();

    /// <summary>
    /// Gets an observable view of registered tasks.
    /// </summary>
    public static ReadOnlyObservableCollection<CharacterTask> Tasks { get; } = new(MutableTasks);

    /// <summary>
    /// Registers a task in the shared task registry.
    /// </summary>
    /// <param name="task">The task to register.</param>
    /// <returns><see langword="true"/> if the task was added; otherwise <see langword="false"/> when a matching key already exists.</returns>
    public static bool AddTask(CharacterTask task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        var key = GetTaskKey(task.ProvidingBotbaseName, task.Name);

        lock (SyncRoot)
        {
            if (!TaskMap.TryAdd(key, task))
            {
                return false;
            }

            MutableTasks.Add(task);
            return true;
        }
    }

    /// <summary>
    /// Removes a task from the shared task registry.
    /// </summary>
    /// <param name="task">The task to remove.</param>
    /// <returns><see langword="true"/> if the task was removed; otherwise <see langword="false"/>.</returns>
    public static bool RemoveTask(CharacterTask task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        return RemoveTask(task.ProvidingBotbaseName, task.Name);
    }

    /// <summary>
    /// Removes a task from the shared task registry by provider and task name.
    /// </summary>
    /// <param name="providingBotbaseName">The botbase name that provides the task.</param>
    /// <param name="taskName">The task name.</param>
    /// <returns><see langword="true"/> if the task was removed; otherwise <see langword="false"/>.</returns>
    public static bool RemoveTask(string providingBotbaseName, string taskName)
    {
        var key = GetTaskKey(providingBotbaseName, taskName);

        lock (SyncRoot)
        {
            if (!TaskMap.TryRemove(key, out var task))
            {
                return false;
            }

            return MutableTasks.Remove(task);
        }
    }

    /// <summary>
    /// Gets a snapshot copy of all currently registered tasks.
    /// </summary>
    /// <returns>A point-in-time snapshot of registered tasks.</returns>
    public static IReadOnlyList<CharacterTask> GetTaskSnapshot()
    {
        lock (SyncRoot)
        {
            return new List<CharacterTask>(MutableTasks);
        }
    }

    /// <summary>
    /// Removes all registered tasks.
    /// </summary>
    public static void ClearTasks()
    {
        lock (SyncRoot)
        {
            TaskMap.Clear();
            MutableTasks.Clear();
        }
    }

    private static string GetTaskKey(string providingBotbaseName, string taskName)
    {
        if (string.IsNullOrWhiteSpace(providingBotbaseName))
        {
            throw new ArgumentException("Providing botbase name cannot be null or whitespace.", nameof(providingBotbaseName));
        }

        if (string.IsNullOrWhiteSpace(taskName))
        {
            throw new ArgumentException("Task name cannot be null or whitespace.", nameof(taskName));
        }

        return $"{providingBotbaseName}::{taskName}";
    }
}

