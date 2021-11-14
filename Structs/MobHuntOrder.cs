using System.Runtime.InteropServices;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Sequential, Size = 0x6)]
    public struct MobHuntOrder
    {
        //[FieldOffset(0)]
        public short MobHuntTarget;

        // [FieldOffset(4)]
        public byte NeededKills;

        // [FieldOffset(8)]
        public MobHuntTypeARR Type;

        //  [FieldOffset(10)]
        public byte Rank;

        //  [FieldOffset(11)]
        public byte MobHuntReward;

        //public Item Item => DataManager.GetItem(EventItem);
        public override string ToString()
        {
            return $"{MobHuntTarget} Kills: {NeededKills} {Type} {Rank} {MobHuntReward}";
        }
    }
}