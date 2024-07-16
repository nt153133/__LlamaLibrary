using System.Runtime.Serialization;

namespace LlamaLibrary.JsonObjects.Lisbeth
{
    public class Order
    {
        // The ID is assigned by Lisbeth to reference an order on other stuff. Must be unique within a list of orders.
        [DataMember(Order = 1)]
        public int Id;

        [DataMember(Order = 2)]
        public int Group;

        // The item's RAW ID.
        [DataMember(Order = 3)]
        public int Item;

        [DataMember(Order = 4)]
        public int Amount;

        [DataMember(Order = 5)]
        public bool Enabled;

        [DataMember(Order = 6)]
        public SourceType Type;

        [DataMember(Order = 7)]
        public bool Collectable;

        [DataMember(Order = 8)]
        public bool Hq;

        // Consumable IDs are their TRUE ID (with HQ prefix).
        [DataMember(Order = 9)]
        public int Food;

        // True item ID to account for HQ or NQ.
        [DataMember(Order = 10)]
        public int Medicine;

        // True item ID to account for HQ or NQ.
        [DataMember(Order = 11)]
        public int Manual;

        // By macro name.
        [DataMember(Order = 12)]
        public string Macro;

        // Should be true for all orders you request. Only suborders are not primary.
        [DataMember(Order = 13)]
        public bool IsPrimary;

        // Restock or Absolute
        [DataMember(Order = 14)]
        public AmountMode AmountMode;

        [DataMember(Order = 15)]
        public bool QuickSynth;

        // Leve order type.
        [DataMember(Order = 18)]
        public int LeveId;

        // Fishing Expedition.
        [DataMember(Order = 20)]
        public int FishingExpeditionSpotId;

        // Fishing Expedition.
        [DataMember(Order = 21)]
        public int FishingExpeditionBaitId;

        // Fishing Expedition.
        [DataMember(Order = 22)]
        public bool UseSnagging;

        // Fishing Expedition.
        [DataMember(Order = 23)]
        public bool UseFishEyes;

        // Fishing Expedition.
        [DataMember(Order = 24)]
        public string FishingExpeditionWeatherPatterns;

        // Fishing Expedition.
        [DataMember(Order = 25)]
        public int FishingExpeditionStartHour;

        // Fishing Expedition.
        [DataMember(Order = 26)]
        public int FishingExpeditionEndHour;

        // Fishing Expedition.
        [DataMember(Order = 27)]
        public HooksetOption HooksetOption;

        // Assigned during execution. Used when pausing and resuming orders.
        [DataMember(Order = 28)]
        public int OriginalAmount;

        // The order will stop execution once this level is reached while doing it.
        // If this is > 0 then amount mode is forced into absolute, makes no sense for a restock order.
        [DataMember(Order = 29)]
        public int ConditionalLevel;

        // The job whose level gets checked while executing the order.
        [DataMember(Order = 30)]
        public Job ConditionalJob;

        [DataMember(Order = 32)]
        public bool IsSideOrder;

        // Fishing Expedition.
        [DataMember(Order = 33)]
        public FishingMode FishingExpeditionMode;

        // Fishing Expedition.
        [DataMember(Order = 34)]
        public bool UseChum;

        [DataMember(Order = 35)]
        public bool SkipFinalItem;
    }
}