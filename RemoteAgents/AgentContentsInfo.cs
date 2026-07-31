using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs.Housing;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Contents Info / Timers interface.
    /// </summary>
    public class AgentContentsInfo : AgentInterface<AgentContentsInfo>, IAgent
    {
        private const int EstateSlotCount = 5;
        private const int ScheduledForDemolitionStatus = 4;

        //TODO: Get these hardcoded offsets out of here

        // AgentContentsTimer tail layout. These values are deliberately kept
        // behind the agent wrapper so consumers never depend on client layout.
#if RB_TC
        /// <summary>Byte offset of the five-element estate status array in AgentContentsTimer.</summary>
        internal const int EstateStatusArray = 0x17DC;

        /// <summary>Byte offset of the five-element Unix deadline array in AgentContentsTimer.</summary>
        internal const int EstateDeadlineArray = 0x17F0;
#else
        /// <summary>Byte offset of the five-element estate status array in AgentContentsTimer.</summary>
        internal const int EstateStatusArray = 0x17DC;

        /// <summary>Byte offset of the five-element Unix deadline array in AgentContentsTimer.</summary>
        internal const int EstateDeadlineArray = 0x17F0;
#endif

        // AgentContentsTimer stores FC, private, and shared estates in fixed slots.
        // Slot 1 is not an actionable estate entry and is intentionally omitted.
        private static readonly (int Slot, EstateType EstateType, bool CanCancel)[] EstateSlots =
        {
            (0, EstateType.FreeCompany, true),
            (2, EstateType.Private, true),
            (3, EstateType.SharedEstate1, false),
            (4, EstateType.SharedEstate2, false),
        };

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentContentsInfoOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentContentsInfo"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentContentsInfo(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>Reads and decodes the estate timer arrays cached by the client.</summary>
        /// <returns>A validated snapshot containing each supported estate's demolition state.</returns>
        public EstateDemolitionSnapshot ReadEstateDemolitionSnapshot()
        {
            var now = DateTime.UtcNow;
            if (Pointer == IntPtr.Zero)
            {
                return UnknownSnapshot(now, "The Contents Info agent is unavailable.");
            }

            var statuses = Core.Memory.ReadArray<int>(Pointer + EstateStatusArray, EstateSlotCount);
            var deadlines = Core.Memory.ReadArray<ulong>(Pointer + EstateDeadlineArray, EstateSlotCount);
            if (statuses.Length != EstateSlotCount || deadlines.Length != EstateSlotCount)
            {
                return UnknownSnapshot(now, "The estate timer arrays could not be read completely.");
            }

            string? failureReason = null;
            var entries = new List<EstateDemolitionEntry>(EstateSlots.Length);
            foreach (var (slot, estateType, canCancel) in EstateSlots)
            {
                var rawStatus = statuses[slot];
                var state = DecodeState(rawStatus);
                var deadline = state == EstateDemolitionState.Scheduled
                    ? DecodeDeadline(deadlines[slot], now)
                    : null;

                if (state == EstateDemolitionState.Scheduled && deadline == null)
                {
                    state = EstateDemolitionState.Unknown;
                    failureReason ??= $"The {estateType} demolition deadline was invalid.";
                }
                else if (state == EstateDemolitionState.Unknown)
                {
                    failureReason ??= $"The {estateType} demolition status value ({rawStatus}) was not recognized.";
                }

                entries.Add(new EstateDemolitionEntry(estateType, state, deadline, canCancel));
            }

            // The read itself is valid even if one entry contains a future status
            // value. Consumers can still act on other positively decoded entries.
            return new EstateDemolitionSnapshot(entries, true, now, failureReason);
        }

        private static EstateDemolitionState DecodeState(int rawStatus) => rawStatus switch
        {
            ScheduledForDemolitionStatus => EstateDemolitionState.Scheduled,
            >= 0 and < ScheduledForDemolitionStatus => EstateDemolitionState.NotScheduled,
            _ => EstateDemolitionState.Unknown,
        };

        private static DateTime? DecodeDeadline(ulong rawDeadline, DateTime now)
        {
            if (rawDeadline == 0 || rawDeadline > long.MaxValue)
            {
                return null;
            }

            try
            {
                var deadline = DateTimeOffset.FromUnixTimeSeconds((long)rawDeadline).UtcDateTime;
                return deadline >= now.AddDays(-1) && deadline <= now.AddDays(60) ? deadline : null;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private static EstateDemolitionSnapshot UnknownSnapshot(DateTime retrievedAtUtc, string failureReason)
        {
            var entries = new[]
            {
                new EstateDemolitionEntry(EstateType.FreeCompany, EstateDemolitionState.Unknown, null, true),
                new EstateDemolitionEntry(EstateType.Private, EstateDemolitionState.Unknown, null, true),
                new EstateDemolitionEntry(EstateType.SharedEstate1, EstateDemolitionState.Unknown, null, false),
                new EstateDemolitionEntry(EstateType.SharedEstate2, EstateDemolitionState.Unknown, null, false),
            };

            return new EstateDemolitionSnapshot(entries, false, retrievedAtUtc, failureReason);
        }
    }
}
