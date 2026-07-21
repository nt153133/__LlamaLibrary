using System;
using System.Runtime.InteropServices;

// This is the 14-byte Squadron block beginning at PlayerState + 0x604.
// Field layout is also documented by FFXIVClientStructs PlayerState.
namespace LlamaLibrary.Structs
{
    /// <summary>
    /// Represents the memory-mapped structure for Grand Company Squadron mission and training timers.
    /// Values are typically read from the game's internal squadron status offsets.
    /// </summary>
    /// <remarks>
    /// Information for this struct was originally referenced from the Accountant project and
    /// is independently documented by FFXIVClientStructs in Client.Game.UI.PlayerState.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Size = 14)]
    public struct SquadronTimerData
    {
        /// <summary>
        /// The Unix timestamp (seconds since epoch) when the current squadron mission will end.
        /// </summary>
        public uint MissionEndTimeStamp;

        /// <summary>
        /// The Unix timestamp (seconds since epoch) when the current squadron training session will end.
        /// </summary>
        public uint TrainingEndTimeStamp;

        /// <summary>
        /// The unique identifier of the currently active squadron mission.
        /// </summary>
        public ushort MissionId;

        /// <summary>
        /// The unique identifier of the currently active squadron training session.
        /// </summary>
        public ushort TrainingId;

        /// <summary>
        /// A value indicating whether there are new recruits waiting to be reviewed.
        /// </summary>
        public bool NewRecruits;

        /// <summary>
        /// Gets the local date and time when the current squadron mission completes,
        /// converted from the <see cref="MissionEndTimeStamp"/>.
        /// </summary>
        public DateTime MissionEndTime => DateTimeOffset.FromUnixTimeSeconds(MissionEndTimeStamp).LocalDateTime;

        /// <summary>
        /// Gets the local date and time when the current squadron training session completes,
        /// converted from the <see cref="TrainingEndTimeStamp"/>.
        /// </summary>
        public DateTime TrainingEnd => DateTimeOffset.FromUnixTimeSeconds(TrainingEndTimeStamp).LocalDateTime;
    }
}
