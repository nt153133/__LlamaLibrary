namespace LlamaLibrary.Structs.Housing;

/// <summary>The result of checking whether an estate's demolition status was canceled.</summary>
public sealed class EstateDemolitionResult
{
    /// <summary>Creates the result of an estate demolition cancellation check.</summary>
    /// <param name="estateType">The estate that was checked.</param>
    /// <param name="canceled">Whether refreshed client data reports the timer as canceled.</param>
    /// <param name="snapshot">The final snapshot used to determine the result.</param>
    public EstateDemolitionResult(EstateType estateType, bool canceled, EstateDemolitionSnapshot snapshot)
    {
        EstateType = estateType;
        Canceled = canceled;
        Snapshot = snapshot;
    }

    /// <summary>Gets the estate that was checked.</summary>
    public EstateType EstateType { get; }

    /// <summary>Gets whether refreshed client data reports the timer as canceled.</summary>
    public bool Canceled { get; }

    /// <summary>Gets the final snapshot used to determine the result.</summary>
    public EstateDemolitionSnapshot Snapshot { get; }
}
