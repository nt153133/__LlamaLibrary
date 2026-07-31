namespace LlamaLibrary.Structs.Housing;

/// <summary>The auto-demolition state reported by the Estate timer data.</summary>
public enum EstateDemolitionState
{
    /// <summary>The client value was unavailable or could not be decoded safely.</summary>
    Unknown,

    /// <summary>The estate is not currently scheduled for auto-demolition.</summary>
    NotScheduled,

    /// <summary>The estate is currently scheduled for auto-demolition.</summary>
    Scheduled,
}
