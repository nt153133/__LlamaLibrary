using System.ComponentModel;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.XmlEngine;
using TreeSharp;
using static LlamaLibrary.Helpers.GeneralFunctions;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("AutoInventoryEquip")]
    public class AutoInventoryEquip : LLProfileBehavior
    {
        private bool _isDone;

        [XmlAttribute("UpdateGearSet")]
        [XmlAttribute("updategearset")]
        [DefaultValue(true)]
        private bool UpdateGearSet { get; set; }

        [XmlAttribute("RecommendEquip")]
        [XmlAttribute("recommendequip")]
        [DefaultValue(true)]
        private bool UseRecommendEquip { get; set; }

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public AutoInventoryEquip() : base() { }

        protected override void OnStart()
        {
        }

        protected override void OnDone()
        {
        }

        protected override void OnResetCachedDone()
        {
            _isDone = false;
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => RunEquip());
        }

        private async Task RunEquip()
        {
            if (_isDone)
            {
                await Coroutine.Yield();
                return;
            }

            await InventoryEquipBest(UpdateGearSet, UseRecommendEquip);

            _isDone = true;
        }
    }
}