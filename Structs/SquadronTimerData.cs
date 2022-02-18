using System;
using System.Runtime.InteropServices;
//Info for this struct pulled from https://github.com/Ottermandias/Accountant
namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Sequential, Size = 14)]
    public struct SquadronTimerData
    {
        public uint MissionEndTimeStamp;
        public uint TrainingEndTimeStamp;
        public ushort MissionId;
        public ushort TrainingId;
        public bool NewRecruits;

        public DateTime MissionEndTime => DateTimeOffset.FromUnixTimeSeconds(MissionEndTimeStamp).LocalDateTime;

        public DateTime TrainingEnd => DateTimeOffset.FromUnixTimeSeconds(TrainingEndTimeStamp).LocalDateTime;
    }
}