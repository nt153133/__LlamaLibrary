using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Directors
{
    /// <summary>
    /// Provides read-only access to FFXIV's internal director state for the <c>Out on a Limb</c> and <c>The Finer Miner</c> mini-game
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mini-game works as follows: a cursor swings back and forth along a curved gauge. The player
    /// presses a button to stop the cursor and take a "swing" at the log. The closer the cursor lands
    /// to the invisible sweet spot, the better the result (TryAgain → Good → Great → Excellent). Each
    /// swing depletes the log's HP. The player may collect their payout at any time, or risk it in
    /// a "Double Down" to multiply their winnings using whatever time remains.
    /// </para>
    /// <para>
    /// All static properties here are backed by direct memory reads. No cached reads are used for
    /// <see cref="IsActive"/> or <see cref="Pointer"/> since stale data could lead to stale game state
    /// being acted upon. Most other values are <c>NoCacheRead</c> to avoid stale offsets in hot loops.
    /// </para>
    /// </remarks>
    public static class OutOnALimbDirector
    {
        /// <summary>
        /// Refreshes <see cref="Pointer"/> to the current director. Returns <see langword="false"/> if
        /// the director is not active, in which case <see cref="Pointer"/> is set to <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the director is active and a valid pointer was obtained;
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Call this before reading any game-state properties to ensure <see cref="Pointer"/> is valid.
        /// </remarks>
        public static bool SetPointer()
        {
            if (!IsActive)
            {
                Pointer = IntPtr.Zero;
                return false;
            }

            Pointer = DirectorPointer;
            CursorAddress = CursorAddressRaw;
            return Pointer != IntPtr.Zero && CursorAddress != IntPtr.Zero;
        }

        /// <summary>
        /// Gets the raw memory pointer to the active <c>Out on a Limb</c> director by reading the
        /// pointer stored at the active-director offset.
        /// </summary>
        public static IntPtr DirectorPointer => Core.Memory.Read<IntPtr>(OutOnALimbDirectorOffsets.ActiveDirectorPtr);

        /// <summary>
        /// Gets the raw memory pointer for the current director session. Returns <see cref="IntPtr.Zero"/>
        /// when the mini-game is not active.
        /// </summary>
        public static IntPtr Pointer { get; private set; }

        public static IntPtr CursorAddressRaw
        {
            get
            {
                if (!IsActive)
                {
                    return IntPtr.Zero;
                }

                var numArray = AtkArrayDataHolder.NumberArray(104);

                if (numArray == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                return Core.Memory.Read<IntPtr>(numArray + RetainerHistoryOffsets.NumberArrayData_IntArray);
            }
        }

        public static IntPtr CursorAddress { get; private set; }

        /// <summary>
        /// Gets or sets the current cursor position. Cursor must be locked via RemoteWindows.MiniGameBotanist.Instance.PauseCursor() before setting this value to ensure the game accepts the input.
        /// </summary>
        /// <value>
        /// A value between <c>0</c> and <c>1000</c> (inclusive).
        /// Returns <c>0</c> if the director is not active and <see cref="CursorAddress"/> is zero.
        /// </value>
        public static int CursorLocation
        {
            get
            {
                if (CursorAddress == IntPtr.Zero)
                {
                    return 0;
                }
                return Core.Memory.Read<int>(CursorAddress);
            }

            set
            {
                if (CursorAddress == IntPtr.Zero)
                {
                    return;
                }
                Core.Memory.Write(CursorAddress, value);
            }
        }

        /// <summary>
        /// The current MGP payout accumulated from successful swings.
        /// Updated after each swing resolves.
        /// </summary>
        /// <value>
        /// The raw MGP value stored at the director's current-payout offset.
        /// Returns <c>0</c> if the director is not active and <see cref="Pointer"/> is zero.
        /// </value>
        public static int CurrentPayout => Core.Memory.NoCacheRead<int>(Pointer + OutOnALimbDirectorOffsets.CurrentPayout);

        /// <summary>
        /// The MGP payout that will be awarded if the player opts for "Double Down" and succeeds.
        /// This is typically a fixed multiplier of <see cref="CurrentPayout"/>.
        /// </summary>
        /// <value>
        /// The raw MGP value stored at the director's double-down-payout offset.
        /// Returns <c>0</c> if the director is not active and <see cref="Pointer"/> is zero.
        /// </value>
        public static int DoubleDownPayout => Core.Memory.NoCacheRead<int>(Pointer + OutOnALimbDirectorOffsets.DoubleDownPayout);

        public static byte SwingResultRaw
        {
            get => Core.Memory.NoCacheRead<byte>(Pointer + OutOnALimbDirectorOffsets.SwingResult);
            set => Core.Memory.Write(Pointer + OutOnALimbDirectorOffsets.SwingResult, value);
        }

        /// <summary>
        /// The result of the last swing. Determines how much progress the log receives per swing
        /// (Excellent = most damage, TryAgain = least or none).
        /// </summary>
        /// <value>
        /// One of the values in <see cref="SwingResultType"/>. Defaults to <c>TryAgain</c> before the
        /// first swing resolves.
        /// </value>
        public static SwingResultType SwingResult
        {
            get
            {
                var result = SwingResultRaw;

                return result switch
                {
                    182 or 177 => SwingResultType.TryAgain,
                    183 or 178 => SwingResultType.Good,
                    184 or 179 => SwingResultType.Great,
                    185 or 180 => SwingResultType.Excellent,
                    0 => 0,
                    _          => LogUnexpectedAndDefault(result)
                };
            }
            // Explicitly cast to byte so you are writing the primitive value, not the enum token
            set => Core.Memory.Write(Pointer + OutOnALimbDirectorOffsets.SwingResult, (byte)value);
        }

        // Out-of-line helper to keep the property getter clean
        private static SwingResultType LogUnexpectedAndDefault(byte result)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic($"Unexpected swing result value: {result}. Defaulting to TryAgain.");
            return SwingResultType.TryAgain;
        }

        /// <summary>
        /// The current progress (HP) of the log. Depleting this to zero wins the round. Each swing reduces this
        /// by an amount determined by <see cref="SwingResult"/>. The log starts with 10 HP at the beginning of each round.
        /// </summary>
        /// <value>
        /// The raw HP value stored at the director's progress-needed offset.
        /// Returns <c>10</c> at the start of each round and decreases with each swing. Returns <c>0</c> when the log is fully depleted.
        /// </value>
        public static int CurrentProgress => Core.Memory.NoCacheRead<byte>(Pointer + OutOnALimbDirectorOffsets.ProgressNeeded);

        /// <summary>
        /// The number of swings already taken by the player this round.
        /// The player has up to 10 swings to deplete the log; exceeding this limit fails the round.
        /// </summary>
        /// <value>A value between <c>0</c> and <c>10</c> (inclusive).</value>
        public static int SwingsTaken => Core.Memory.NoCacheRead<byte>(Pointer + OutOnALimbDirectorOffsets.SwingsTaken);

        /// <summary>
        /// Indicates whether the <c>Out on a Limb</c> director is currently active (i.e., the player
        /// is inside the mini-game round).
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the director is active; <see langword="false"/> otherwise.
        /// </value>
        public static bool IsActive => Core.Memory.NoCacheRead<byte>(OutOnALimbDirectorOffsets.IsActiveByte + 1) == 1;

        /// <summary>
        /// The maximum progress (HP) the log can have. Fixed at 10 hits.
        /// </summary>
        /// <value>Fixed value: <c>10</c>.</value>
        public static byte MaxProgress => 10;

        /// <summary>
        /// The maximum number of swings allowed per round. Exceeding this limit results in a failed round. 10 for out on a limb, 4-6 for The Finer Miner depending on difficulty
        /// </summary>
        /// <value>Fixed value: <c>10</c>.</value>
        public static byte MaxNumberOfSwings => Core.Memory.Read<byte>(Pointer + OutOnALimbDirectorOffsets.MaxSwings);

        /// <summary>
        /// The number of swings remaining before the round is lost (too many misses).
        /// Computed as <c>MaxNumberOfSwings - SwingsTaken</c>.
        /// </summary>
        /// <value>
        /// Returns <c>0</c> when all swings have been taken; negative values indicate a programming error
        /// (should never occur in normal use).
        /// </value>
        public static int SwingsRemaining => MaxNumberOfSwings - SwingsTaken;

        /// <summary>
        /// The number of seconds remaining on the round timer.
        /// </summary>
        /// <value>
        /// The remaining time in seconds, retrieved by calling the game's injected remaining-time function.
        /// Returns <c>-1</c> if the director is not active (game time does not apply outside the round).
        /// </value>
        public static int SecondsRemaining
        {
            get
            {
                if (!IsActive)
                {
                    return -1;
                }

                return Core.Memory.CallInjectedWraper<int>(OutOnALimbDirectorOffsets.RemainingTimeFunction, OutOnALimbDirectorOffsets.ActiveDirectorPtr);
            }
        }

        public new static string ToString()
        {
            return $"CurrentPayout: {CurrentPayout}, DoubleDownPayout: {DoubleDownPayout}, SwingResult: {SwingResult}, CurrentProgress: {CurrentProgress}/{MaxProgress}, SwingsTaken: {SwingsTaken}/{MaxNumberOfSwings}, IsActive: {IsActive}, SecondsRemaining: {SecondsRemaining} ProgressOff: {OutOnALimbDirectorOffsets.ProgressNeeded}";
        }
    }
}
/// <summary>
/// Represents the quality of a swing result in the <c>Out on a Limb</c> mini-game.
/// Better results deal more progress damage to the log.
/// </summary>
/// <remarks>
/// The sweet-spot alignment determines which value the cursor resolves to. Excellent results
/// from hitting close to the sweet spot will most quickly deplete the log's HP within the time limit.
/// All values are backed by raw in-memory byte values from the game.
/// </remarks>
public enum SwingResultType : byte
{
    /// <summary>Cursor landed far from the sweet spot — minimal or no progress dealt to the log.</summary>
    TryAgain = 182,

    /// <summary>Cursor landed in a decent range — normal progress damage.</summary>
    Good = 183,

    /// <summary>Cursor landed close to the sweet spot — good progress damage.</summary>
    Great = 184,

    /// <summary>Cursor landed exactly on the sweet spot — maximum progress damage.</summary>
    Excellent = 185
}