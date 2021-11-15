using System.Threading.Tasks;
using System.Windows.Media;
using Clio.XmlEngine;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("LLGoHome")]
    public class LLGoHome : LLProfileBehavior
    {
        private bool _isDone;

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        protected override Color LogColor => Colors.Chartreuse;

        public LLGoHome() : base() { }

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
            return new ActionRunCoroutine(r => LLGoHomeTask());
        }

        private async Task LLGoHomeTask()
        {
            Log.Information("Going Home");
            await LlamaLibrary.Helpers.GeneralFunctions.GoHome();

            _isDone = true;
        }
    }
}