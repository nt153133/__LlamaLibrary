using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Directors
{
    public static class OutOnALimbDirector
    {
        

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

        public static IntPtr DirectorPointer => Core.Memory.Read<IntPtr>(OutOnALimbDirectorOffsets.ActiveDirectorPtr);

        public static IntPtr Pointer { get; private set; }

        public static int CurrentPayout => Core.Memory.NoCacheRead<int>(Pointer + OutOnALimbDirectorOffsets.CurrentPayout);

        public static int DoubleDownPayout => Core.Memory.NoCacheRead<int>(Pointer + OutOnALimbDirectorOffsets.DoubleDownPayout);

        public static SwingResultType SwingResult
        {
            get => Core.Memory.NoCacheRead<SwingResultType>(Pointer + OutOnALimbDirectorOffsets.SwingResult);
            set => Core.Memory.Write(Pointer + OutOnALimbDirectorOffsets.SwingResult, value);
        }

        public static int CurrentProgress => Core.Memory.NoCacheRead<byte>(Pointer + OutOnALimbDirectorOffsets.ProgressNeeded);

        public static int SwingsTaken => Core.Memory.NoCacheRead<byte>(Pointer + OutOnALimbDirectorOffsets.SwingsTaken);

        public static bool IsActive => Core.Memory.NoCacheRead<byte>(OutOnALimbDirectorOffsets.IsActiveByte + 1) == 1;

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

                return Core.Memory.CallInjectedWraper<int>(OutOnALimbDirectorOffsets.RemainingTimeFunction, OutOnALimbDirectorOffsets.ActiveDirectorPtr);
            }
        }

        public new static string ToString()
        {
            return $"CurrentPayout: {CurrentPayout}, DoubleDownPayout: {DoubleDownPayout}, SwingResult: {SwingResult}, CurrentProgress: {CurrentProgress}/{MaxProgress}, SwingsTaken: {SwingsTaken}/{MaxNumberOfSwings}, IsActive: {IsActive}, SecondsRemaining: {SecondsRemaining} ProgressOff: {OutOnALimbDirectorOffsets.ProgressNeeded}";
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