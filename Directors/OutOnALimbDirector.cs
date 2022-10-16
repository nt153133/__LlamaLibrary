using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Directors
{
    public static class OutOnALimbDirector
    {
        internal static class Offsets
        {
            [Offset("48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B D0 48 8D 0D ? ? ? ? B8 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr ActiveDirectorPtr;

            [Offset("48 89 5C 24 ? 57 48 83 EC ? 48 8B 01 48 8B F9 48 8B 18 E8 ? ? ? ?")]
            internal static IntPtr RemainingTimeFunction;

            [Offset("89 87 ? ? ? ? 0F B6 46 ? 88 87 ? ? ? ? 8B 46 ? Add 2 Read32")]
            internal static int SwingResult;

            [Offset("89 87 ? ? ? ? 8B 46 ? 89 87 ? ? ? ? 0F B6 87 ? ? ? ? Add 2 Read32")]
            internal static int CurrentPayout;

            [Offset("89 87 ? ? ? ? 0F B6 87 ? ? ? ? 48 6B D0 ? Add 2 Read32")]
            internal static int DoubleDownPayout;

            [Offset("66 89 87 ? ? ? ? 8B 97 ? ? ? ? Add 3 Read32")]
            internal static int ProgressNeeded;

            [Offset("C6 87 ? ? ? ? ? 8B 46 ? Add 2 Read32")]
            internal static int SwingsTaken;

            [Offset("80 3D ? ? ? ? ? 0F 84 ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 74 ? Add 2 TraceRelative")]
            internal static IntPtr IsActiveByte;
        }

        public static bool SetPointer()
        {
            if (!IsActive)
            {
                Pointer = IntPtr.Zero;
                return false;
            }

            Pointer = DirectorPointer;
            return Pointer != IntPtr.Zero;
        }

        public static IntPtr DirectorPointer => Core.Memory.Read<IntPtr>(Offsets.ActiveDirectorPtr);

        public static IntPtr Pointer { get; private set; }

        public static int CurrentPayout => Core.Memory.NoCacheRead<int>(Pointer + Offsets.CurrentPayout);

        public static int DoubleDownPayout => Core.Memory.NoCacheRead<int>(Pointer + Offsets.DoubleDownPayout);

        public static SwingResultType SwingResult
        {
            get => Core.Memory.NoCacheRead<SwingResultType>(Pointer + Offsets.SwingResult);
            set => Core.Memory.Write(Pointer + Offsets.SwingResult, value);
        }

        public static int CurrentProgress => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.ProgressNeeded);

        public static int SwingsTaken => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.SwingsTaken);

        public static bool IsActive => Core.Memory.NoCacheRead<byte>(Offsets.IsActiveByte + 1) == 1;

        public static byte MaxProgress => 10;

        public static byte MaxNumberOfSwings => 10;

        public static int SwingsRemaining => MaxNumberOfSwings - SwingsTaken;

        public static int SecondsRemaining
        {
            get
            {
                if (!IsActive)
                {
                    return -1;
                }

                return Core.Memory.CallInjected64<int>(Offsets.RemainingTimeFunction, Offsets.ActiveDirectorPtr);
            }
        }

        public new static string ToString()
        {
            return $"CurrentPayout: {CurrentPayout}, DoubleDownPayout: {DoubleDownPayout}, SwingResult: {SwingResult}, CurrentProgress: {CurrentProgress}/{MaxProgress}, SwingsTaken: {SwingsTaken}/{MaxNumberOfSwings}, IsActive: {IsActive}, SecondsRemaining: {SecondsRemaining} ProgressOff: {Offsets.ProgressNeeded}";
        }
    }

    public enum SwingResultType : byte
    {
        TryAgain = 182,
        Good = 183,
        Great = 184,
        Excellent = 185
    }
}