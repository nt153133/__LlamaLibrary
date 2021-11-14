using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.XmlEngine;
using ff14bot;
using LlamaLibrary.Logging;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("LLToast")]
    public class LLToast : LLProfileBehavior
    {
        private bool _isDone;

        [XmlAttribute("Message")]
        [XmlAttribute("message")]
        public string Message { get; set; }

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public LLToast() : base() { }

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
            return new ActionRunCoroutine(r => SendToast(Message));
        }

        private Task SendToast(string message)
        {
            Core.OverlayManager.AddToast(() => "" + message, TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(29, 213, 226), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));

            _isDone = true;
            return Task.CompletedTask;
        }
    }
}