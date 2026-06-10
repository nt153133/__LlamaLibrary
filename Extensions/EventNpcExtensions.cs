using System;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Objects;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="GameObject"/> and <see cref="BattleCharacter"/> classes,
    /// specifically for event NPC and player interactions.
    /// </summary>
    public static class EventNpcExtensions
    {
        /// <summary>
        /// Retrieves the icon identifier for an event NPC.
        /// </summary>
        /// <param name="eventNpc">The game object instance (must be an <see cref="GameObjectType.EventNpc"/>).</param>
        /// <returns>The numeric icon ID, or 0 if the object is not an event NPC.</returns>
        public static uint IconId(this GameObject eventNpc)
        {
            return eventNpc.Type == GameObjectType.EventNpc ? Core.Memory.Read<uint>(eventNpc.Pointer + EventNpcExtensionsOffsets.IconID) : 0U;
        }

        /// <summary>
        /// Attempts to open the trade window with another player character.
        /// </summary>
        /// <param name="otherPlayer">The player character to trade with.</param>
        /// <returns>A status code from the game's internal trade window function, or -1 if the target is not a player.</returns>
        public static int OpenTradeWindow(this BattleCharacter otherPlayer)
        {
            return otherPlayer.Type == GameObjectType.Pc ? Core.Memory.CallInjectedWraper<IntPtr>(Offsets.OpenTradeWindow, Offsets.g_InventoryManager, otherPlayer.ObjectId).ToInt32() : -1;
        }
    }
}