using System;
using System.Linq;
using System.Text;
using ff14bot;
using LlamaLibrary.ClientDataHelpers;

namespace LlamaLibrary.Helpers;

/// <summary>Reads and writes the five player Triple Triad profile decks.</summary>
public static class TripleTriadDecks
{
    public const int DeckCount = 5;
    public const int CardCount = 5;

    private const int DeckArrayOffset = 0x48;
    private const int DeckSize = 0x3A;
    private const int DeckNameSize = 0x30;
    private const int DeckCardsOffset = 0x30;

    /// <summary>Writes a complete profile deck. Deck indices are zero-based.</summary>
    public static bool TryWrite(int deckIndex, ushort[] cardIds, string name, out string diagnostic)
    {
        if (deckIndex < 0 || deckIndex >= DeckCount)
        {
            diagnostic = $"Deck index {deckIndex} is outside 0..{DeckCount - 1}.";
            return false;
        }

        if (cardIds == null || cardIds.Length != CardCount || cardIds.Any(x => x == 0) || cardIds.Distinct().Count() != CardCount)
        {
            diagnostic = "A Triple Triad deck must contain five distinct, non-zero card IDs.";
            return false;
        }

        var module = UiManagerProxy.GoldSaucerModule;
        if (module == IntPtr.Zero)
        {
            diagnostic = "The GoldSaucerModule pointer is null.";
            return false;
        }

        var deck = module + DeckArrayOffset + deckIndex * DeckSize;
        var nameBytes = new byte[DeckNameSize];
        var encodedName = Encoding.UTF8.GetBytes((name ?? string.Empty).Trim());
        Array.Copy(encodedName, nameBytes, Math.Min(encodedName.Length, nameBytes.Length - 1));
        Core.Memory.WriteBytes(deck, nameBytes);

        for (var cardIndex = 0; cardIndex < CardCount; cardIndex++)
            Core.Memory.Write(deck + DeckCardsOffset + cardIndex * sizeof(ushort), cardIds[cardIndex]);

        var readBack = Read(deckIndex);
        var success = readBack.SequenceEqual(cardIds);
        diagnostic = success
            ? $"Wrote deck slot {deckIndex + 1}: {string.Join(", ", cardIds)}."
            : $"Deck slot {deckIndex + 1} read-back did not match the requested cards.";
        return success;
    }

    /// <summary>Reads the five card IDs from a zero-based player deck slot.</summary>
    public static ushort[] Read(int deckIndex)
    {
        if (deckIndex < 0 || deckIndex >= DeckCount) return Array.Empty<ushort>();
        var module = UiManagerProxy.GoldSaucerModule;
        if (module == IntPtr.Zero) return Array.Empty<ushort>();
        var cards = module + DeckArrayOffset + deckIndex * DeckSize + DeckCardsOffset;
        return Enumerable.Range(0, CardCount)
            .Select(x => Core.Memory.Read<ushort>(cards + x * sizeof(ushort)))
            .ToArray();
    }
}
