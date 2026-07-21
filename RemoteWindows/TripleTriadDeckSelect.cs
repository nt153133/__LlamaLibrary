namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Triple Triad pre-match deck selection window.</summary>
    public sealed class TripleTriadDeckSelect : RemoteWindow<TripleTriadDeckSelect>
    {
        public TripleTriadDeckSelect() : base("TripleTriadSelDeck")
        {
        }

        public bool SelectRecommended()
        {
            return SelectRecommended(out _);
        }

        public bool SelectRecommended(out string diagnostic)
        {
            if (!IsOpen)
            {
                diagnostic = "TripleTriadSelDeck is not open";
                return false;
            }

            // Captured from the live addon callback:
            // TripleTriadSelDeck, updateState=true, (ValueType.Int, 0x5).
            SendAction(true, new Atk.AtkValue(5));
            diagnostic = "direct SendAction(updateState=true, Int=5) dispatched";
            return true;
        }

        /// <summary>Selects a saved Triple Triad deck using its zero-based index.</summary>
        public bool SelectDeck(int deckIndex)
        {
            return SelectDeck(deckIndex, out _);
        }

        /// <summary>Selects a saved Triple Triad deck using its zero-based index.</summary>
        public bool SelectDeck(int deckIndex, out string diagnostic)
        {
            if (!IsOpen)
            {
                diagnostic = "TripleTriadSelDeck is not open";
                return false;
            }

            if (deckIndex < 0 || deckIndex > 4)
            {
                diagnostic = $"Deck index {deckIndex} is outside 0..4";
                return false;
            }

            // Captured callbacks: Int=0 selects Deck 1, Int=1 selects Deck 2, etc.
            SendAction(true, new Atk.AtkValue(deckIndex));
            diagnostic = $"direct SendAction(updateState=true, Int={deckIndex}) dispatched";
            return true;
        }
    }
}
