using System.Runtime.Serialization;

namespace LlamaLibrary.JsonObjects.Lisbeth
{
    public class Order
    {
        // The ID is assigned by Lisbeth to reference an order on other stuff. Must be unique within a list of orders.
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public int Group { get; set; }

        // The item's RAW ID.
        [DataMember(Order = 3)]
        public uint Item { get; set; }

        [DataMember(Order = 4)]
        public uint Amount { get; set; }

        [DataMember(Order = 5)]
        public bool Enabled { get; set; } = true;

        [DataMember(Order = 6)]
        public SourceType Type { get; set; }

        [DataMember(Order = 7)]
        public bool Collectable { get; set; }

        [DataMember(Order = 8)]
        public bool Hq { get; set; }

        // Consumable IDs are their TRUE ID (with HQ prefix).
        [DataMember(Order = 9)]
        public int Food { get; set; }

        // True item ID to account for HQ or NQ.
        [DataMember(Order = 10)]
        public int Medicine { get; set; }

        // True item ID to account for HQ or NQ.
        [DataMember(Order = 11)]
        public int Manual { get; set; }

        // By macro name.
        [DataMember(Order = 12)]
        public string Macro { get; set; }

        // Should be true for all orders you request. Only suborders are not primary.
        [DataMember(Order = 13)]
        public bool IsPrimary { get; set; }

        // Restock or Absolute
        [DataMember(Order = 14)]
        public AmountMode AmountMode { get; set; }

        [DataMember(Order = 15)]
        public bool QuickSynth { get; set; }

        // Leve order type.
        [DataMember(Order = 18)]
        public int LeveId { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 20)]
        public int FishingExpeditionSpotId { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 21)]
        public int FishingExpeditionBaitId { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 22)]
        public bool UseSnagging { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 23)]
        public bool UseFishEyes { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 24)]
        public string FishingExpeditionWeatherPatterns { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 25)]
        public int FishingExpeditionStartHour { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 26)]
        public int FishingExpeditionEndHour { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 27)]
        public HooksetOption HooksetOption { get; set; }

        // Assigned during execution. Used when pausing and resuming orders.
        [DataMember(Order = 28)]
        public int OriginalAmount { get; set; }

        // The order will stop execution once this level is reached while doing it.
        // If this is > 0 then amount mode is forced into absolute, makes no sense for a restock order.
        [DataMember(Order = 29)]
        public int ConditionalLevel { get; set; }

        // The job whose level gets checked while executing the order.
        [DataMember(Order = 30)]
        public Job ConditionalJob { get; set; }

        [DataMember(Order = 32)]
        public bool IsSideOrder { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 33)]
        public FishingMode FishingExpeditionMode { get; set; }

        // Fishing Expedition.
        [DataMember(Order = 34)]
        public bool UseChum { get; set; }

        [DataMember(Order = 35)]
        public bool SkipFinalItem { get; set; }
    }
}