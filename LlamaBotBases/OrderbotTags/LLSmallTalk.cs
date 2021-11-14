using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.XmlEngine;
using LlamaLibrary.Logging;
using TreeSharp;
using static LlamaLibrary.Helpers.GeneralFunctions;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("LLSmallTalk")]
    public class LLSmallTalk : LLProfileBehavior
    {
        [XmlAttribute("WaitTime")]
        [XmlAttribute("waittime")]
        [DefaultValue(500)]
        private int WaitTime { get; set; }

        private bool _isDone;

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public LLSmallTalk() : base() { }

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
            return new ActionRunCoroutine(r => AwaitSmallTalk());
        }

        private async Task AwaitSmallTalk()
        {
            if (_isDone)
            {
                await Coroutine.Yield();
                return;
            }

            await SmallTalk(WaitTime);

            _isDone = true;
        }
    }
}