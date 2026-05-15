using System;
using ff14bot;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helpers for summoning and dismissing companion minions via the game action system.
    /// </summary>
    public static class MinionHelper
    {
        /// <summary>
        /// Gets whether the local player currently has a minion summoned.
        /// Reads the minion pointer and checks the active minion ID field in game memory.
        /// </summary>
        public static bool IsMinionSummoned
        {
            get
            {
                var ptr = Core.Me.MinionPtr();

                if (ptr == IntPtr.Zero)
                {
                    return false;
                }

                return Core.Memory.Read<ushort>(ptr + 0x80) != 0;
            }
        }

        /// <summary>
        /// Gets the <c>Companion</c> sheet row ID of the currently summoned minion, or 0 if none is active.
        /// </summary>
        public static ushort MinionId
        {
            get
            {
                var ptr = Core.Me.MinionPtr();

                if (ptr == IntPtr.Zero)
                {
                    return 0;
                }

                return Core.Memory.Read<ushort>(ptr + 0x80);
            }
        }

        /// <summary>
        /// Summons the specified minion. Does nothing if the requested minion is already active.
        /// </summary>
        /// <param name="minionId">The <c>Companion</c> sheet row ID of the minion to summon.</param>
        public static void SummonMinion(ushort minionId)
        {
            var currentMinion = MinionId;

            if (minionId == currentMinion)
            {
                return;
            }

            ActionHelper.UseAction((ff14bot.Enums.ActionType)8, minionId);
        }

        /// <summary>
        /// Dismisses the currently active minion. Does nothing if no minion is summoned.
        /// </summary>
        public static void DismissMinion()
        {
            var currentMinion = MinionId;

            if (currentMinion == 0)
            {
                return;
            }

            ActionHelper.UseAction((ff14bot.Enums.ActionType)8, currentMinion);
        }
    }
}