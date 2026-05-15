namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Represents the outcome category for a character task run.
/// </summary>
public enum CharacterTaskResultStatus
{
    /// <summary>The task completed successfully.</summary>
    Success,
    /// <summary>The task failed in a way that prevents further character-switching iterations from continuing.</summary>
    FailedCannotContinue,
    /// <summary>The task was skipped because a prerequisite condition was not met (e.g., required in-game content is unavailable).</summary>
    FailedUnavailable,
    /// <summary>The task failed because the character could not navigate to the required in-game location.</summary>
    FailedNavigation,
    /// <summary>The task failed for a reason not covered by the other status values.</summary>
    FailedOther,
    /// <summary>The task failed because an unhandled exception was thrown during execution.</summary>
    FailedException
}

