using System;
using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x3C)]
    public unsafe struct CustomDelivery // Note the 'unsafe' keyword
    {
        [FieldOffset(0x00)]
        public uint DeliverableItemId;

        [FieldOffset(0x04)]
        public fixed short CollectabilityTiers[3]; // True inline memory, no GC object!

        // The padding gap at 0x0A is naturally ignored

        [FieldOffset(0x0C)]
        public uint RewardScripLowItemId;

        [FieldOffset(0x10)]
        public uint RewardScripHighItemId;

        [FieldOffset(0x14)]
        public fixed short ScripLow[3];

        [FieldOffset(0x1A)]
        public fixed short ScripHigh[3];

        [FieldOffset(0x20)]
        public fixed short Satisfaction[3];

        [FieldOffset(0x26)]
        public fixed short Gil[3];

        [FieldOffset(0x2C)]
        public fixed int Exp[3];

        [FieldOffset(0x38)]
        public uint Unknown0x38;

        public Item? Item => DataManager.GetItem(DeliverableItemId);
        public Item? LowScripRewardItem => DataManager.GetItem(RewardScripLowItemId);
        public Item? HighScripRewardItem => DataManager.GetItem(RewardScripHighItemId);
    }
}