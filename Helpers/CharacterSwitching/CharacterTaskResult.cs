namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Represents the result of a character task run.
/// </summary>
public sealed class CharacterTaskResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="CharacterTaskResult"/>.
    /// </summary>
    /// <param name="wasSuccessful">Whether the task completed successfully.</param>
    /// <param name="status">The outcome category of the run.</param>
    /// <param name="message">A user-facing message describing the outcome.</param>
    public CharacterTaskResult(bool wasSuccessful, CharacterTaskResultStatus status, string message)
    {
        WasSuccessful = wasSuccessful;
        Status = status;
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the task completed successfully.
    /// </summary>
    public bool WasSuccessful { get; }

    /// <summary>
    /// Gets the outcome category of the run.
    /// </summary>
    public CharacterTaskResultStatus Status { get; }

    /// <summary>
    /// Gets the user-facing message describing the outcome.
    /// </summary>
    public string Message { get; }
}

