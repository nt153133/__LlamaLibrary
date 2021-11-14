using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.XmlEngine;
using ff14bot.Behavior;
using LlamaLibrary.Logging;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("LLLeaveDuty")]
    public class LeaveDuty : LLProfileBehavior
    {
        private bool _isDone;

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public LeaveDuty() : base() { }

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
            return new ActionRunCoroutine(r => LeaveDutyTask());
        }

        private async Task LeaveDutyTask()
        {
            ff14bot.Managers.DutyManager.LeaveActiveDuty();
            await Coroutine.Wait(20000, () => CommonBehaviors.IsLoading);
            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            _isDone = true;
        }
    }
}