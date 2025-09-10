using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    public static class BlueMageSpellBook
    {
        private static readonly LLogger Log = new(typeof(BlueMageSpellBook).Name, Colors.CornflowerBlue);

        

        public static uint[] ActiveSpells => Core.Memory.ReadArray<uint>(SpellLocation, BlueMageSpellBookOffsets.MaxActive + 1);

        public static IntPtr SpellLocation => BlueMageSpellBookOffsets.ActionManager + BlueMageSpellBookOffsets.BluSpellActiveOffset;

        public static void SetSpell(int index, uint spellId)
        {
            Core.Memory.CallInjectedWraper<IntPtr>(BlueMageSpellBookOffsets.SetSpell,
            BlueMageSpellBookOffsets.ActionManager,
            index,
            spellId);
        }

        public static async Task SetAllSpells(uint[] spells)
        {
            if (spells.Length > BlueMageSpellBookOffsets.MaxActive)
            {
                return;
            }

            var index = 0;
            for (var i = 0; i < spells.Length; i++)
            {
                SetSpell(i, spells[i]);
                await Coroutine.Sleep(200);
                index++;
            }

            for (var i = index; i < BlueMageSpellBookOffsets.MaxActive + 1; i++)
            {
                SetSpell(i, 0);
                await Coroutine.Sleep(200);
            }
        }

        public static async Task SetSpells(uint[] spells)
        {
            if (spells.Length > BlueMageSpellBookOffsets.MaxActive)
            {
                return;
            }

            var currentSpells = ActiveSpells;

            if (spells.All(i => currentSpells.Contains(i)))
            {
                return;
            }

            var spellsToAdd = spells.Except(currentSpells);

            var spellsToModify = new List<(int, uint)>();

            foreach (var spell in spellsToAdd)
            {
                for (var i = 0; i < currentSpells.Length; i++)
                {
                    if (currentSpells[i] == 0)
                    {
                        currentSpells[i] = spell;
                        spellsToModify.Add((i, spell));
                        break;
                    }

                    if (!spells.Contains(spell))
                    {
                        currentSpells[i] = spell;
                        spellsToModify.Add((i, spell));
                        break;
                    }
                }
            }

            foreach (var pair in spellsToModify)
            {
                SetSpell(pair.Item1, pair.Item2);
                await Coroutine.Sleep(300);
            }
        }
    }
}