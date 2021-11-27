using System.ComponentModel;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.XmlEngine;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using LlamaLibrary.Helpers;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("BuyWhiteScriptItem")]
    public class BuyWhiteScriptItem : LLProfileBehavior
    {
        private bool _isDone;

        [XmlAttribute("ItemId")]
        public int ItemId { get; set; }

        [XmlAttribute("SelectString")]
        [DefaultValue(0)]
        public int SelectStringLine { get; set; }

        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public BuyWhiteScriptItem() : base() { }

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
            return new ActionRunCoroutine(r => BuyWhiteScrip(ItemId));
        }

        private async Task BuyWhiteScrip(int itemId)
        {
            await Coroutine.Sleep(500);

            Navigator.NavigationProvider = Navigator.NavigationProvider ?? new ServiceNavigationProvider();
            Navigator.PlayerMover = Navigator.PlayerMover ?? new SlideMover();

            await LlamaLibrary.Helpers.IshgardHandin.BuyScripItem((uint)itemId, SelectStringLine);

            _isDone = true;
        }
    }
}