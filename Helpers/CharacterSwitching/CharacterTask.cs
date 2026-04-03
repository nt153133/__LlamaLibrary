using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.Helpers.CharacterSwitching;

public abstract class CharacterTask
{
	private bool _isRunning;
	private DateTime? _lastRun;
	private TimeSpan _lastRunDuration;
	private CharacterTaskResultStatus? _lastRunStatus;
	private string _lastRunStatusMessage = string.Empty;
	private bool _lastRunWasSuccessful;

	/// <summary>
	/// Gets the display name of the task.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// Gets the user-facing description of the task.
	/// </summary>
	public abstract string Description { get; }

	/// <summary>
	/// Gets the name of the botbase that provides this task.
	/// </summary>
	public abstract string ProvidingBotbaseName { get; }

	/// <summary>
	/// Gets a value indicating whether this task is currently running.
	/// </summary>
	public bool IsRunning
	{
		get => _isRunning;
		private set => SetField(ref _isRunning, value);
	}

	/// <summary>
	/// Gets a value indicating whether the last run completed successfully.
	/// </summary>
	public bool LastRunWasSuccessful
	{
		get => _lastRunWasSuccessful;
		private set => SetField(ref _lastRunWasSuccessful, value);
	}

	/// <summary>
	/// Gets the local time when the task last finished running.
	/// </summary>
	public DateTime? LastRun
	{
		get => _lastRun;
		private set => SetField(ref _lastRun, value);
	}

	/// <summary>
	/// Gets the status for the last run.
	/// </summary>
	public CharacterTaskResultStatus? LastRunStatus
	{
		get => _lastRunStatus;
		private set => SetField(ref _lastRunStatus, value);
	}

	/// <summary>
	/// Gets the user-facing message from the last run.
	/// </summary>
	public string LastRunStatusMessage
	{
		get => _lastRunStatusMessage;
		private set => SetField(ref _lastRunStatusMessage, value);
	}

	/// <summary>
	/// Gets the duration of the last completed run.
	/// </summary>
	public TimeSpan LastRunDuration
	{
		get => _lastRunDuration;
		private set => SetField(ref _lastRunDuration, value);
	}

	/// <summary>
	/// Checks whether the task can run right now.
	/// </summary>
	/// <returns>
	/// A tuple where <c>canRun</c> indicates availability and <c>reason</c> explains unavailability when false.
	/// </returns>
	public abstract Task<(bool canRun, string reason)> CheckAvailabilityAsync();

	/// <summary>
	/// Runs the task-specific implementation.
	/// </summary>
	protected abstract Task<CharacterTaskResult> ExecuteAsync();

	/// <summary>
	/// Runs the full task flow: availability check, execution, and status tracking.
	/// </summary>
	/// <returns>The final run result.</returns>
	public virtual async Task<CharacterTaskResult> RunAsync()
	{
		IsRunning = true;
		var startTime = DateTime.Now;

		try
		{
			var (canRun, reason) = await CheckAvailabilityAsync();
			if (!canRun)
			{
				var statusMessage = string.IsNullOrWhiteSpace(reason)
					? Translator.GetText(TranslationKey.CharacterTaskUnavailable)
					: reason;

				var unavailableResult = new CharacterTaskResult(false, CharacterTaskResultStatus.FailedUnavailable, statusMessage);
				LastRunWasSuccessful = unavailableResult.WasSuccessful;
				LastRunStatus = unavailableResult.Status;
				LastRunStatusMessage = unavailableResult.Message;
				return unavailableResult;
			}

			var result = await ExecuteAsync();
			var endTime = DateTime.Now;

			LastRun = endTime;
			LastRunDuration = endTime - startTime;
			LastRunWasSuccessful = result.WasSuccessful;
			LastRunStatus = result.Status;
			LastRunStatusMessage = result.Message;

			return result;
		}
		catch (Exception ex)
		{
			ff14bot.Helpers.Logging.WriteException(ex);

			var endTime = DateTime.Now;
			var statusMessage = Translator.GetText(TranslationKey.CharacterTaskGenericError);
			var exceptionResult = new CharacterTaskResult(false, CharacterTaskResultStatus.FailedException, statusMessage);

			LastRun = endTime;
			LastRunDuration = endTime - startTime;
			LastRunWasSuccessful = false;
			LastRunStatus = exceptionResult.Status;
			LastRunStatusMessage = exceptionResult.Message;

			return exceptionResult;
		}
		finally
		{
			IsRunning = false;
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
}