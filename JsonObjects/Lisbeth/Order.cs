using System.Collections.Generic;

namespace LlamaLibrary.JsonObjects.Lisbeth
{
    public class Order
    {
        public int Id { get; set; }

        public int Group { get; set; }

        public uint Item { get; set; }

        public uint Amount { get; set; }

        public bool Enabled { get; set; } = true;

        public SourceType Type { get; set; }

        public bool Collectable { get; set; }

        public bool Hq { get; set; }

        // Consumable IDs are their TRUE ID (with HQ prefix).
        public int Food { get; set; }

        public int Medicine { get; set; }

        public int Manual { get; set; }

        public string Macro { get; set; }

        public bool IsPrimary { get; set; }

        public AmountMode AmountMode { get; set; }

        public bool QuickSynth { get; set; }

        public bool IsForTurnIn { get; set; }

        public string FishingRecord { get; set; }

        public int LeveId { get; set; }

        //public LisScripType ExchangeCostScrip;

        public int FishingExpeditionSpotId { get; set; }

        public int FishingExpeditionBaitId { get; set; }

        public bool UseSnagging { get; set; }

        public bool UseFishEyes { get; set; }

        public string FishingExpeditionWeatherPatterns { get; set; }

        public int FishingExpeditionStartHour { get; set; }

        public int FishingExpeditionEndHour { get; set; }

        //public HooksetOption HooksetOption;

        public int OriginalAmount { get; set; }

        public int ConditionalLevel { get; set; }

        //public Job ConditionalJob;

        public int CraftCollectable { get; set; }

        public bool IsSideOrder { get; set; }

        public bool SkipFinalItem { get; set; }

        public HashSet<uint> TrashExclusionItems { get; set; }
    }
}