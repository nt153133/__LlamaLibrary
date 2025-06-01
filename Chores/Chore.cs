using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace LlamaLibrary.Chores;

/// <summary>
/// Represents an abstract base class for a chore.
/// </summary>
public abstract class Chore : INotifyPropertyChanged, IEquatable<Chore>
{
    private bool _isDisabledDueToError;
    private bool _isEnabled = true;
    private DateTime _lastCheck = DateTime.MinValue;
    private string _lastError = string.Empty;
    private DateTime _lastRun = DateTime.MinValue;
    private DateTime _nextCheck = DateTime.MinValue;

    /// <summary>
    /// Gets the name of the chore.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets or sets the last run time of the chore.
    /// </summary>
    public DateTime LastRun
    {
        get => _lastRun;
        protected set => SetField(ref _lastRun, value);
    }

    /// <summary>
    /// Gets or sets the last check time of the chore.
    /// </summary>
    public DateTime LastCheck
    {
        get => _lastCheck;
        protected set => SetField(ref _lastCheck, value);
    }

    /// <summary>
    /// Gets or sets the next check time of the chore.
    /// </summary>
    public DateTime NextCheck
    {
        get => _nextCheck;
        protected set => SetField(ref _nextCheck, value);
    }

    /// <summary>
    /// Gets or sets the interval between checks.
    /// </summary>
    public virtual TimeSpan CheckInterval { get; protected set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string LastError
    {
        get => _lastError;
        protected set => SetField(ref _lastError, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the chore is enabled.
    /// </summary>
    public virtual bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the chore is disabled due to an error.
    /// </summary>
    public bool IsDisabledDueToError
    {
        get => _isDisabledDueToError;
        set => SetField(ref _isDisabledDueToError, value);
    }

    /// <summary>
    /// Gets the priority of the chore. Lower number means higher priority.
    /// </summary>
    public int Priority { get; } = 99;

    /// <summary>
    /// Performs the work of the chore.
    /// </summary>
    /// <param name="returnCharacter">Indicates whether to return the character.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    protected abstract Task<bool> Work(bool returnCharacter = false);

    /// <summary>
    /// Determines whether the chore should perform its work.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the work should be performed.</returns>
    protected abstract Task<bool> ShouldDoWork();

    /// <summary>
    /// Checks whether the chore should be performed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the chore should be performed.</returns>
    public virtual async Task<bool> Check()
    {
        if (!IsEnabled || IsDisabledDueToError)
        {
            return false;
        }

        if (DateTime.Now < NextCheck)
        {
            return false;
        }

        LastCheck = DateTime.Now;
        NextCheck = DateTime.Now + CheckInterval;

        return await ShouldDoWork();
    }

    /// <summary>
    /// Performs the work of the chore.
    /// </summary>
    /// <param name="returnCharacter">Indicates whether to return the character to it's starting position.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    public virtual async Task<bool> DoWork(bool returnCharacter = false)
    {
        if (!IsEnabled || IsDisabledDueToError)
        {
            return false;
        }

        LastRun = DateTime.Now;
        var result = await Work(returnCharacter);
        if (!result)
        {
            IsDisabledDueToError = true;
        }

        return result;
    }

    /// <summary>
    /// Resets the chore, enabling it if it was disabled due to an error.
    /// </summary>
    public virtual void Reset()
    {
        IsDisabledDueToError = false;
    }

    /// <summary>
    /// Disables the chore.
    /// </summary>
    public virtual void Disable()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Enables the chore.
    /// </summary>
    public virtual void Enable()
    {
        IsEnabled = true;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the field and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">Type of the field.</typeparam>
    /// <param name="field">Reference to the field.</param>
    /// <param name="value">New value of the field.</param>
    /// <param name="propertyName">Name of the property that changed.</param>
    /// <returns><see langword="true"/> if the value has changed; otherwise, <see langword="false"/>.</returns>
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

    public bool Equals(Chore? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Chore)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(Chore? left, Chore? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Chore? left, Chore? right)
    {
        return !Equals(left, right);
    }
}