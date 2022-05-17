using System;
using ff14bot;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Helpers
{
    public static class MinionHelper
    {
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

        public static void SummonMinion(ushort minionId)
        {
            var currentMinion = MinionId;

            if (minionId == currentMinion)
            {
                return;
            }

            ActionHelper.UseAction((ff14bot.Enums.ActionType)8, minionId);
        }

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