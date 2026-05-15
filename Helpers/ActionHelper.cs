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
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// RB's ActionManager won't do the map decipher action on an item so this re-implements DoAction just for that reason.
    /// </summary>
    public static class ActionHelper
    {
        private static readonly LLogger Log = new(nameof(ActionHelper), Colors.Gold);

        

        /// <summary>
        /// Calls the game's native <c>DoAction</c> function to use any action type.
        /// </summary>
        /// <param name="actionType">The category of action (spell, item, etc.).</param>
        /// <param name="actionID">The numeric ID of the action to use.</param>
        /// <param name="targetID">Object ID of the target; defaults to the player's own object.</param>
        /// <param name="a4">Additional parameter passed to <c>DoAction</c> (slot info when using items).</param>
        /// <param name="a5">Additional parameter passed to <c>DoAction</c>.</param>
        /// <param name="a6">Additional parameter passed to <c>DoAction</c>.</param>
        /// <param name="a7">Additional parameter passed to <c>DoAction</c>.</param>
        /// <returns><see langword="true"/> if the action was accepted by the game; otherwise <see langword="false"/>.</returns>
        public static bool UseAction(ff14bot.Enums.ActionType actionType, uint actionID, long targetID = 0xE000_0000, uint a4 = 0, uint a5 = 0, uint a6 = 0, uint a7 = 0)
        {
            Core.Memory.ClearCallCache();
            var result = Core.Memory.CallInjectedWraper<byte>(ActionHelperOffsets.DoAction,
            ActionHelperOffsets.ActionManagerParam,
            (uint)actionType,
            actionID,
            targetID,
            a4,
            a5,
            a6,
            a7) == 1;

            Core.Memory.ClearCallCache();

            return result;
        }

        /// <summary>
        /// Attempts to decipher the treasure map in the given inventory slot by casting the Decipher action on it.
        /// Returns <see langword="false"/> if the slot is not a treasure map item or if a map is already active.
        /// </summary>
        /// <param name="slot">The inventory slot containing the treasure map item to decipher.</param>
        /// <returns><see langword="true"/> if the decipher action was accepted; otherwise <see langword="false"/>.</returns>
        public static bool DoActionDecipher(BagSlot slot)
        {
            if ((slot.Item.MyItemRole() != MyItemRole.Map) || HasMap())
            {
                return false;
            }

            Core.Memory.ClearCallCache();
            var result = Core.Memory.CallInjectedWraper<byte>(ActionHelperOffsets.DoAction,
            ActionHelperOffsets.ActionManagerParam,
            (uint)ff14bot.Enums.ActionType.Spell,
            (uint)ActionHelperOffsets.DecipherSpell,
            (long)Core.Player.ObjectId,
            slot.Slot | ((int)slot.BagId << 16),
            0,
            0,
            0) == 1;

            Core.Memory.ClearCallCache();

            return result;
        }

        /// <summary>
        /// Checks whether the player currently has an active (non-quest) treasure map in their key-item bag.
        /// </summary>
        /// <returns><see langword="true"/> if a treasure map is present; otherwise <see langword="false"/>.</returns>
        public static bool HasMap()
        {
            var questMaps = new uint[] { 2001351, 2001705, 2001772, 200974 };
            return InventoryManager.GetBagByInventoryBagId(InventoryBagId.KeyItems).FilledSlots.Any(i => i.EnglishName.EndsWith("map", StringComparison.InvariantCultureIgnoreCase) && !questMaps.Contains(i.RawItemId));
        }

        /// <summary>
        /// Discards the currently held treasure map from the player's key-item bag, if one exists.
        /// </summary>
        public static void DiscardCurrentMap()
        {
            var map = CurrentMap();

            if (map != default(BagSlot))
            {
                map.Discard();
            }
        }

        /// <summary>
        /// Returns the first non-quest treasure map found in the player's key-item bag.
        /// </summary>
        /// <returns>The <see cref="BagSlot"/> of the treasure map, or <see langword="null"/> if none is found.</returns>
        public static BagSlot? CurrentMap()
        {
            var questMaps = new uint[] { 2001351, 2001705, 2001772, 200974 };
            var map = InventoryManager.GetBagByInventoryBagId(InventoryBagId.KeyItems).FilledSlots.Where(i => i.EnglishName.EndsWith("map", StringComparison.InvariantCultureIgnoreCase) && !questMaps.Contains(i.RawItemId)).ToList();
            return map.Count != 0 ? map.First() : default;
        }
    }
}