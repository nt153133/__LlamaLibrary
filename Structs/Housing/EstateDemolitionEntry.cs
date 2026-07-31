using System;

namespace LlamaLibrary.Structs.Housing;

/// <summary>A decoded auto-demolition entry for one estate.</summary>
public sealed class EstateDemolitionEntry
{
    /// <summary>Creates a decoded estate demolition entry.</summary>
    /// <param name="estateType">The estate represented by this entry.</param>
    /// <param name="state">The decoded auto-demolition state.</param>
    /// <param name="demolitionDeadlineUtc">The validated UTC demolition deadline, when scheduled.</param>
    /// <param name="canCurrentCharacterCancel">Whether entering this estate can cancel its timer for the current character.</param>
    public EstateDemolitionEntry(EstateType estateType, EstateDemolitionState state, DateTime? demolitionDeadlineUtc, bool canCurrentCharacterCancel)
    {
        EstateType = estateType;
        State = state;
        DemolitionDeadlineUtc = demolitionDeadlineUtc;
        CanCurrentCharacterCancel = canCurrentCharacterCancel;
    }

    /// <summary>Gets the estate represented by this entry.</summary>
    public EstateType EstateType { get; }

    /// <summary>Gets the decoded auto-demolition state.</summary>
    public EstateDemolitionState State { get; }

    /// <summary>Gets the validated UTC demolition deadline, or null when unavailable or not scheduled.</summary>
    public DateTime? DemolitionDeadlineUtc { get; }

    /// <summary>Gets whether entering this estate can cancel its timer for the current character.</summary>
    public bool CanCurrentCharacterCancel { get; }
}
