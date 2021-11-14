using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.XmlEngine;
using LlamaLibrary.Helpers;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("AutoLisbethEquip")]
    public class AutoLisbethEquip : LLProfileBehavior
    {
        private bool _isDone;

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public AutoLisbethEquip() : base() { }

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
            return new ActionRunCoroutine(r => LisbethEquipBest());
        }

        private async Task LisbethEquipBest()
        {
            if (_isDone)
            {
                await Coroutine.Yield();
                return;
            }

            await GeneralFunctions.StopBusy(leaveDuty: false, dismount: false);
            await Lisbeth.EquipOptimalGear();

            _isDone = true;
        }
    }
}