namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Represents the result of a character task run.
/// </summary>
public sealed class CharacterTaskResult
{
    public CharacterTaskResult(bool wasSuccessful, CharacterTaskResultStatus status, string message)
    {
        WasSuccessful = wasSuccessful;
        Status = status;
        Message = message;
    }

    public bool WasSuccessful { get; }

    public CharacterTaskResultStatus Status { get; }

    public string Message { get; }
}

