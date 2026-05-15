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
    /// <summary>
    /// Manages the Blue Mage active spellbook, allowing automation of spell slot assignment.
    /// Blue Mage (BLU) is a limited job that can equip up to 24 learned spells at a time.
    /// </summary>
    public static class BlueMageSpellBook
    {
        private static readonly LLogger Log = new(typeof(BlueMageSpellBook).Name, Colors.CornflowerBlue);

        

        /// <summary>Gets the array of spell IDs currently in the Blue Mage active spellbook (up to <c>MaxActive + 1</c> entries).</summary>
        public static uint[] ActiveSpells => Core.Memory.ReadArray<uint>(SpellLocation, BlueMageSpellBookOffsets.MaxActive + 1);

        /// <summary>Gets the memory address of the first active Blue Mage spell slot in the ActionManager.</summary>
        public static IntPtr SpellLocation => BlueMageSpellBookOffsets.ActionManager + BlueMageSpellBookOffsets.BluSpellActiveOffset;

        /// <summary>
        /// Sets a single Blue Mage spell slot to the given spell ID via the game's injected function.
        /// </summary>
        /// <param name="index">Zero-based index of the spell slot to update (0 through <c>MaxActive</c>).</param>
        /// <param name="spellId">The spell ID to place in the slot, or <c>0</c> to clear it.</param>
        public static void SetSpell(int index, uint spellId)
        {
            Core.Memory.CallInjectedWraper<IntPtr>(BlueMageSpellBookOffsets.SetSpell,
            BlueMageSpellBookOffsets.ActionManager,
            index,
            spellId);
        }

        /// <summary>
        /// Replaces every active spell slot with the provided array, clearing any remaining slots.
        /// Silently returns without changes if <paramref name="spells"/> exceeds the maximum slot count.
        /// </summary>
        /// <param name="spells">Array of spell IDs to assign; must not exceed <c>MaxActive</c> in length.</param>
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

        /// <summary>
        /// Adds any spells from <paramref name="spells"/> not already in the active set, placing them into
        /// empty or unwanted slots. Existing matching spells are not moved.
        /// Silently returns without changes if <paramref name="spells"/> exceeds the maximum slot count.
        /// </summary>
        /// <param name="spells">Array of spell IDs to ensure are active.</param>
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