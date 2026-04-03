namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Represents the outcome category for a character task run.
/// </summary>
public enum CharacterTaskResultStatus
{
    Success,
    FailedCannotContinue,
    FailedUnavailable,
    FailedNavigation,
    FailedOther,
    FailedException
}

