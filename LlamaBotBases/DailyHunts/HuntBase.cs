using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Service_Navigation;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;
using TreeSharp;
using Action = TreeSharp.Action;

namespace LlamaBotBases.DailyHunts
{
    public class HuntBase : AsyncBotBase
    {
        private static readonly LLogger Log = new LLogger("Daily Hunts", Colors.Pink);

        private static readonly List<uint> Blacklist = new List<uint>();
        private static List<BagSlot> _playerItems;
        public static float PostCombatDelay = 0f;

        internal static bool Bool0;

        public static readonly InventoryBagId[] PlayerInventoryBagIds =
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4
        };

        private Composite _root;

        public HuntBase()
        {
            OffsetManager.Init();
        }

        public override string Name => @"Daily Hunts";
        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;
        public override Composite Root => null;

        public override bool WantButton { get; } = false;
        internal static bool InFight => GameObjectManager.Attackers.Any();
        internal static BattleCharacter FirstAttacker => GameObjectManager.Attackers.FirstOrDefault();

        static int[] dailyOrderTypes = { 0, 1, 2, 3, 6, 7, 8, 10, 11, 12 };

        public override async Task AsyncRoot()
        {
            await Hunts.DoHunts(dailyOrderTypes);

            //TreeRoot.Stop($"Stop Requested");

        }


        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
        }

        public override void Stop()
        {
            (Navigator.NavigationProvider as IDisposable)?.Dispose();
            Navigator.NavigationProvider = null;
        }

    }
}