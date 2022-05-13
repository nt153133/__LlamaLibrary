using System;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// RB's ActionManager won't do the map decipher action on an item so this reimplements DoAction just for that reason.
    /// </summary>
    public static class ActionHelper
    {
        private static readonly string Name = "ActionHelper";
        private static readonly Color LogColor = Colors.Gold;
        private static readonly LLogger Log = new LLogger(Name, LogColor);

        internal static class Offsets
        {
            [Offset("Search 44 89 44 24 ? 53 55 57 41 54")]
            [OffsetCN("E8 ? ? ? ? E9 ? ? ? ? 48 8B 4E ? 48 8B 01 FF 90 ? ? ? ? 48 8B C8 BA ? ? ? ? E8 ? ? ? ? 8B 93 ? ? ? ? 45 33 D2 Add 1 TraceRelative")]
            internal static IntPtr DoAction;

            [Offset("Search 48 8D 0D ? ? ? ? 44 8D 42 ? E8 ? ? ? ? 85 C0 Add 3 TraceRelative")]
            internal static IntPtr ActionManagerParam;

            //41 B8 ? ? ? ? 89 5C 24 ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 75 ?

            [Offset("Search 41 B8 ? ? ? ? 89 5C 24 ? E8 ? ? ? ? 84 C0 75 ? Add 2 Read32")]
            internal static int DecipherSpell;
        }

        public static bool DoActionDecipher(BagSlot slot)
        {
            if ((slot.Item.MyItemRole() != MyItemRole.Map) || HasMap())
            {
                return false;
            }

            Core.Memory.ClearCallCache();
            var result = Core.Memory.CallInjected64<byte>(Offsets.DoAction, new object[8]
            {
                Offsets.ActionManagerParam, //rcx
                (uint)ff14bot.Enums.ActionType.Spell, //rdx
                (uint)Offsets.DecipherSpell, //r8
                (long)Core.Player.ObjectId, //r9
                (int)(slot.Slot | ((int)slot.BagId << 16)), //a5 +0x28
                0, //a6 + 0x30
                0, //a7
                0 //a
            }) == 1;

            Core.Memory.ClearCallCache();

            return result;
        }

        public static bool HasMap()
        {
            var questMaps = new uint[] { 2001351, 2001705, 2001772, 200974 };
            return InventoryManager.GetBagByInventoryBagId(InventoryBagId.KeyItems).FilledSlots.Any(i => i.EnglishName.EndsWith("map", StringComparison.InvariantCultureIgnoreCase) && !questMaps.Contains(i.RawItemId));
        }

        public static void DiscardCurrentMap()
        {
            var map = CurrentMap();

            if (map != default(BagSlot))
            {
                map.Discard();
            }
        }

        public static BagSlot CurrentMap()
        {
            var questMaps = new uint[] { 2001351, 2001705, 2001772, 200974 };
            var map = InventoryManager.GetBagByInventoryBagId(InventoryBagId.KeyItems).FilledSlots.Where(i => i.EnglishName.EndsWith("map", StringComparison.InvariantCultureIgnoreCase) && !questMaps.Contains(i.RawItemId)).ToList();
            return map.Any() ? map.First() : default;
        }
    }
}