using System.Windows.Media;
using Clio.XmlEngine;
using ff14bot;
using LlamaLibrary.Logging;
using TreeSharp;
using Action = TreeSharp.Action;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("LLStopBot")]
    [XmlElement("StopBot")]

    public class LLStopBotTag : LLProfileBehavior
    {
        private bool _done;

        public override bool IsDone => _done;

        public LLStopBotTag() : base() { }

        protected override Composite CreateBehavior()
        {
            return new PrioritySelector(
                new Decorator(
                    ret => TreeRoot.IsRunning,
                    new Action(r =>
                    {
                        TreeRoot.Stop();
                        _done = true;
                    })
                )
            );
        }

        /// <summary>
        /// This gets called when a while loop starts over so reset anything that is used inside the IsDone check.
        /// </summary>
        protected override void OnResetCachedDone()
        {
            _done = false;
        }

        protected override void OnDone()
        {
        }
    }
}