using System;
using System.Collections.Generic;
using System.Linq;

namespace LlamaLibrary.Structs.Housing;

/// <summary>A validated snapshot of the Estate timer data cached by the client.</summary>
public sealed class EstateDemolitionSnapshot
{
    /// <summary>Creates a decoded snapshot of all estate demolition entries.</summary>
    /// <param name="entries">The decoded estate entries.</param>
    /// <param name="isValid">Whether the underlying client arrays were read successfully.</param>
    /// <param name="retrievedAtUtc">The UTC time at which the client data was read.</param>
    /// <param name="failureReason">An optional explanation of a complete or partial decoding failure.</param>
    public EstateDemolitionSnapshot(IReadOnlyList<EstateDemolitionEntry> entries, bool isValid, DateTime retrievedAtUtc, string? failureReason = null)
    {
        Entries = entries;
        IsValid = isValid;
        RetrievedAtUtc = retrievedAtUtc;
        FailureReason = failureReason;
    }

    /// <summary>Gets the decoded estate entries.</summary>
    public IReadOnlyList<EstateDemolitionEntry> Entries { get; }

    /// <summary>Gets whether the underlying client arrays were read successfully.</summary>
    public bool IsValid { get; }

    /// <summary>Gets the UTC time at which the client data was read.</summary>
    public DateTime RetrievedAtUtc { get; }

    /// <summary>Gets an explanation of a complete or partial decoding failure, when present.</summary>
    public string? FailureReason { get; }

    /// <summary>Gets whether any estate entry could not be decoded safely.</summary>
    public bool HasUnknownState => Entries.Any(entry => entry.State == EstateDemolitionState.Unknown);

    /// <summary>Gets the decoded entry for the requested estate, or null when it is absent.</summary>
    /// <param name="estateType">The estate to locate.</param>
    /// <returns>The matching decoded entry, or null if the snapshot does not contain it.</returns>
    public EstateDemolitionEntry? GetEntry(EstateType estateType) => Entries.FirstOrDefault(entry => entry.EstateType == estateType);
}
