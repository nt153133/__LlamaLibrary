using System;
using System.Linq;
using ff14bot;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Board result, reward and exit controls.</summary>
    public sealed class XBMResult : CrucibleWindow<XBMResult>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMResult() : base("XBMResult")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(ReadVisibleValues());

        /// <summary>Requests result progression through the agent; some clients leave the local page unchanged.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool Advance()
        {
            return Callback(3, 0);
        }

        private IntPtr resultButton;
        private bool resultButtonUnavailable;
        private object? resolvedMemory;
        private int resolvedProcessId;
        private IntPtr resolvedModule;

        /// <summary>Invokes the validated native Next/Exit button; unsupported builds return false.</summary>
        /// <returns>Whether the button was invoked. Use Advance with a bounded retry if unavailable.</returns>
        public bool TryProgressButton()
        {
            // Singleton wrappers survive reattachment; native addresses and failed scans do not.
            var module = Core.Memory.Process.MainModule;
            if (module == null)
            {
                return false;
            }
            if (!ReferenceEquals(resolvedMemory, Core.Memory) || resolvedProcessId != Core.Memory.Process.Id || resolvedModule != module.BaseAddress)
            {
                resultButton = IntPtr.Zero;
                resultButtonUnavailable = false;
                resolvedMemory = Core.Memory;
                resolvedProcessId = Core.Memory.Process.Id;
                resolvedModule = module.BaseAddress;
            }

            var window = WindowByName;
            if (window == null || !window.IsVisible || resultButtonUnavailable)
            {
                return false;
            }
            try
            {
                if (resultButton == IntPtr.Zero)
                {
                    // ReceiveEvent's event-kind/page-flag checks, with Ghidra operand masks.
                    // One match on Global 2026.09.01 and two prior builds; none on TC.
                    using (var finder = new GreyMagic.PatternFinder(Core.Memory))
                    {
                        bool dontRebase = false;
                        var matches = finder.FindMany("Search 66 83 FA ? 75 ? 53 48 83 EC ? 80 B9 ? ? ? ? ?", ref dontRebase);
                        if (dontRebase || matches == null || matches.Length != 1)
                        {
                            throw new NotSupportedException("Result button signature is missing or ambiguous.");
                        }

                        // FindMany returns an RVA for a pattern without extraction commands.
                        resultButton = module.BaseAddress + checked((int)matches[0].ToInt64());
                    }
                }
                // AtkEventListener slot 2 must match before ButtonClick (25).
                // This implementation does not read event payloads.
                var listener = Core.Memory.Read<IntPtr>(window.Pointer);
                if (Core.Memory.Read<IntPtr>(listener + 2 * IntPtr.Size) != resultButton)
                {
                    throw new InvalidOperationException("Result addon event implementation changed.");
                }
                Core.Memory.CallInjected64<ulong>(resultButton, window.Pointer, (uint)25, (uint)0, IntPtr.Zero, IntPtr.Zero);
                return true;
            }
            catch (Exception ex)
            {
                resultButtonUnavailable = true;
                ff14bot.Helpers.Logging.Write("[XBMResult] Native button unavailable: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>Detached board result, reward and exit controls data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 169, "XBMResult");
                BoardName = Text(data, 0);
                DifficultyName = Text(data, 1);
                Completion = Text(data, 3);
                BossScore = data[10].UInt;
                RewardItem = data[48].UInt;
                RewardQuantity = data[63].UInt;
                Score = data[68].UInt;
                Rank = Text(data, 69);
                PetCount = data[72].UInt;
                PetIcons = Enumerable.Range(73, 15).Select(i => data[i].UInt).ToArray();
            }

            /// <summary>Localized board name.</summary>
            public string BoardName { get; internal set; } = string.Empty;

            /// <summary>Localized difficulty.</summary>
            public string DifficultyName { get; internal set; } = string.Empty;

            /// <summary>Formatted completion percentage.</summary>
            public string Completion { get; internal set; } = string.Empty;

            /// <summary>Boss-score component.</summary>
            public uint BossScore { get; internal set; }

            /// <summary>Permanent reward item ID.</summary>
            public uint RewardItem { get; internal set; }

            /// <summary>Permanent reward quantity.</summary>
            public uint RewardQuantity { get; internal set; }

            /// <summary>Total score.</summary>
            public uint Score { get; internal set; }

            /// <summary>Localized rank label.</summary>
            public string Rank { get; internal set; } = string.Empty;

            /// <summary>Reported roster size.</summary>
            public uint PetCount { get; internal set; }

            /// <summary>Fifteen familiar icon slots, including unused entries.</summary>
            public uint[] PetIcons { get; internal set; } = Array.Empty<uint>();
        }
    }
}
