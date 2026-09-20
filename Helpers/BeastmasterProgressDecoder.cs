using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    internal static class BeastmasterProgressDecoder
    {
        // The native getters accept species 1..50; two adjacent byte arrays
        // hold ranks then EXP. Missing owned values invalidate the whole snapshot.
        internal const int SpeciesCount = 50;

        internal static bool TryDecode(byte[]? bytes, IEnumerable<uint>? ownedSpecies,
            [NotNullWhen(true)] out IReadOnlyDictionary<uint, BeastmasterProgress>? progress, out string reason)
        {
            progress = null;
            reason = "The permanent pet progression snapshot is incomplete.";
            if (bytes == null || bytes.Length != SpeciesCount * 2 || ownedSpecies == null)
            {
                return false;
            }
            uint[] owned = ownedSpecies.ToArray();
            if (owned.Length == 0 || owned.Distinct().Count() != owned.Length ||
                owned.Any(id => id < 1 || id > SpeciesCount))
            {
                reason = "The unlocked pet collection is not ready or its layout changed.";
                return false;
            }
            var result = new Dictionary<uint, BeastmasterProgress>();
            foreach (uint id in owned)
            {
                int index = checked((int)id - 1);
                int rank = bytes[index], exp = bytes[SpeciesCount + index];
                if (rank < 1 || rank > BeastmasterProgress.MaximumRank || exp > 100)
                {
                    reason = "Permanent rank/EXP is not ready or invalid for species " + id + ".";
                    return false;
                }
                result.Add(id, new BeastmasterProgress(rank, exp));
            }
            // Unowned rows may be zero. Ownership comes from RB's unlock state,
            // never from treating a nonzero/stale rank as proof of a capture.
            progress = new ReadOnlyDictionary<uint, BeastmasterProgress>(result);
            reason = string.Empty;
            return true;
        }
    }
}