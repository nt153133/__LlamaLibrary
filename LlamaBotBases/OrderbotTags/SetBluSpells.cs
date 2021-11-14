using System.ComponentModel;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.XmlEngine;
using LlamaLibrary.Helpers;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("SetBluActive")]
    public class SetBluSpells : LLProfileBehavior
    {
        private bool _isDone;
        [XmlAttribute("Spells")]
        public int[] Spells { get; set; }

        [XmlAttribute("Clear")]
        [DefaultValue(false)]
        public bool Clear { get; set; }

        public override bool HighPriority => true;

        public SetBluSpells() : base() { }

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
            return new ActionRunCoroutine(r => SetBlueActiveSpells(Spells));
        }

        private async Task SetBlueActiveSpells(int[] spells)
        {
            var newSpells = new uint[spells.Length];
            for (var i = 0; i < spells.Length; i++)
            {
                newSpells[i] = (uint)spells[i];
            }

            if (Clear)
            {
                await BlueMageSpellBook.SetAllSpells(newSpells);
            }
            else
            {
                await BlueMageSpellBook.SetSpells(newSpells);
            }

            await Coroutine.Sleep(100);
            _isDone = true;
        }

        public override bool IsDone => _isDone;
    }
}