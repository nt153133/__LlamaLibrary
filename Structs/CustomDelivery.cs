using System;
using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    /// <summary>
    /// Represents the data for a single custom delivery (Satisfaction Supply) item for an NPC.
    /// Maps to a 0x3C byte structure in game memory containing collectability thresholds and tiered rewards.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x3C)]
    public unsafe struct CustomDelivery
    {
        /// <summary>The ID of the item to be delivered.</summary>
        [FieldOffset(0x00)]
        public uint DeliverableItemId;

        /// <summary>
        /// The three collectability thresholds required for different reward tiers.
        /// Index 0 is the minimum for acceptance, index 2 is the threshold for maximum rewards.
        /// </summary>
        [FieldOffset(0x04)]
        public fixed short CollectabilityTiers[3];

        /// <summary>The ID of the lower-tier scrip rewarded (e.g., White Scrips).</summary>
        [FieldOffset(0x0C)]
        public uint RewardScripLowItemId;

        /// <summary>The ID of the higher-tier scrip rewarded (e.g., Purple Scrips).</summary>
        [FieldOffset(0x10)]
        public uint RewardScripHighItemId;

        /// <summary>The amount of low-tier scrips rewarded for each of the three collectability tiers.</summary>
        [FieldOffset(0x14)]
        public fixed short ScripLow[3];

        /// <summary>The amount of high-tier scrips rewarded for each of the three collectability tiers.</summary>
        [FieldOffset(0x1A)]
        public fixed short ScripHigh[3];

        /// <summary>The amount of satisfaction points rewarded for each of the three collectability tiers.</summary>
        [FieldOffset(0x20)]
        public fixed short Satisfaction[3];

        /// <summary>The amount of Gil rewarded for each of the three collectability tiers.</summary>
        [FieldOffset(0x26)]
        public fixed short Gil[3];

        /// <summary>The amount of Experience points rewarded for each of the three collectability tiers.</summary>
        [FieldOffset(0x2C)]
        public fixed int Exp[3];

        /// <summary>Unknown internal value at offset 0x38.</summary>
        [FieldOffset(0x38)]
        public uint Unknown0x38;

        /// <summary>Gets the <see cref="ff14bot.Managers.Item"/> associated with <see cref="DeliverableItemId"/>.</summary>
        public Item? Item => DataManager.GetItem(DeliverableItemId);

        /// <summary>Gets the <see cref="ff14bot.Managers.Item"/> associated with <see cref="RewardScripLowItemId"/>.</summary>
        public Item? LowScripRewardItem => DataManager.GetItem(RewardScripLowItemId);

        /// <summary>Gets the <see cref="ff14bot.Managers.Item"/> associated with <see cref="RewardScripHighItemId"/>.</summary>
        public Item? HighScripRewardItem => DataManager.GetItem(RewardScripHighItemId);
    }
}