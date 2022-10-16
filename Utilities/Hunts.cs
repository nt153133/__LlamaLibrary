using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Service_Navigation;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using TreeSharp;
using Action = TreeSharp.Action;

namespace LlamaLibrary.Utilities
{
    public class Hunts
    {
        private static readonly LLogger Log = new("Hunts Utility", Colors.Pink);

        private static readonly List<uint> Blacklist = new();
        private static readonly List<BagSlot> _playerItems;
        public static float PostCombatDelay = 0f;

        internal static bool Bool0;

        internal static bool InFight => GameObjectManager.Attackers.Any();
        internal static BattleCharacter FirstAttacker => GameObjectManager.Attackers.FirstOrDefault();

        private static readonly int[] DailyOrderTypes = { 0, 1, 2, 3, 6, 7, 8, 10, 11, 12 };

        public static async Task DoHunts(int[] huntTypes)
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            ff14bot.Settings.CharacterSettings.Instance.UseMount = true;

            var hadOld = await GetHuntBills(huntTypes);
            await CompleteHunts(huntTypes);

            if (hadOld)
            {
                await GetHuntBills(huntTypes);
                await CompleteHunts(huntTypes);
            }

            if (WorldManager.CanTeleport())
            {
                WorldManager.TeleportById(Core.Me.HomePoint.Id);
                await Coroutine.Sleep(5000);
                await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
                if (CommonBehaviors.IsLoading)
                {
                    await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                }
            }
        }

        public static async Task<bool> GetHuntBills(int[] huntTypes)
        {
            var statues = HuntHelper.GetDailyStatus(huntTypes);

            foreach ((var orderType, var huntOrderStatus) in statues)
            {
                var orderTypeObj = HuntHelper.GetMobHuntOrderType((int)orderType);

                switch (huntOrderStatus)
                {
                    case HuntOrderStatus.OnlyFatesLeft:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Only Fates left for today's dailies so done");
                        break;
                    case HuntOrderStatus.OnlyFatesLeftOld:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Only Fates left for old dailies so should yeet them and get new ones");
                        HuntHelper.DiscardMobHuntType(orderType);
                        await HuntHelper.GetHuntsByOrderType(orderType);
                        break;
                    case HuntOrderStatus.NotAccepted:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Have not accepted today's hunts");
                        await HuntHelper.GetHuntsByOrderType(orderType);
                        break;
                    case HuntOrderStatus.NotAcceptedOld:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Not Accepted and last accepted were old and so should get new ones");
                        await HuntHelper.GetHuntsByOrderType(orderType);
                        break;
                    case HuntOrderStatus.Complete:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Finished today's dailies");
                        break;
                    case HuntOrderStatus.CompleteOld:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Finished  old dailies");
                        await HuntHelper.GetHuntsByOrderType(orderType);
                        break;
                    case HuntOrderStatus.Unfinished:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Unfinished current dailies");
                        break;
                    case HuntOrderStatus.UnFinishedOld:
                        Log.Information($"{orderTypeObj.Item.CurrentLocaleName} - Unfinished old dailies");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return statues.Any(i => i.HuntOrderStatus == HuntOrderStatus.UnFinishedOld);
        }

        public static async Task CompleteHunts()
        {
            await CompleteHunts(DailyOrderTypes);
        }

        public static async Task CompleteHunts(int[] orderTypes)
        {
            const int flytoHunt = 418;
            int[] umbra = { 107, 247 };
            var hunts = new List<DailyHuntOrder>();
            foreach (var dailyOrderType in orderTypes)
            {
                hunts.AddRange(HuntHelper.GetAcceptedDailyHunts(dailyOrderType).Where(i => !i.IsFinished));
            }

            foreach (var hunt in hunts.OrderBy(i => i.MapId).ThenBy(j => j.Location.X))
            {
                Log.Information($"hunt: {hunt}");
                while (Core.Me.InCombat)
                {
                    var target = GameObjectManager.Attackers.FirstOrDefault(i => i.InCombat && i.IsAlive);
                    if (target != default(BattleCharacter) && target.IsValid && target.IsAlive)
                    {
                        await Lisbeth.Kill(target);

                        //await Navigation.GetTo(WorldManager.ZoneId, target.Location);
                        //await KillMob(target);
                    }
                }

                /*
                if (WorldManager.ZoneId == 401 && hunt.MapId == 401)
                {
                    var AE = WorldManager.AetheryteIdsForZone(hunt.MapId).OrderBy(i => i.Item2.DistanceSqr(hunt.Location)).First();
                    WorldManager.TeleportById(AE.Item1);
                    await Coroutine.Wait(20000, () => WorldManager.ZoneId == AE.Item1);
                    await Coroutine.Sleep(2000);
                }

                if (WorldManager.ZoneId == 402 && hunt.MapId == 402)
                {
                    var AE = WorldManager.AetheryteIdsForZone(hunt.MapId).OrderBy(i => i.Item2.DistanceSqr(hunt.Location)).First();
                    WorldManager.TeleportById(AE.Item1);
                    await Coroutine.Wait(20000, () => WorldManager.ZoneId == AE.Item1);
                    await Coroutine.Sleep(2000);
                }*/

                if (hunt.HuntTarget == flytoHunt)
                {
                    var AE = WorldManager.AetheryteIdsForZone(hunt.MapId).OrderBy(i => i.Item2.DistanceSqr(hunt.Location)).First();
                    WorldManager.TeleportById(AE.Item1);
                    await Coroutine.Wait(20000, () => WorldManager.ZoneId == AE.Item1);
                    await Coroutine.Sleep(2000);
                    await Navigation.FlightorMove(new Vector3(196.902f, -163.4457f, 113.3596f));

                    //
                    Navigator.PlayerMover.MoveTowards(new Vector3(208.712f, -165.4754f, 128.228f));

                    await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading);
                    Navigator.Stop();
                    await Coroutine.Sleep(1000);

                    if (CommonBehaviors.IsLoading)
                    {
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                    }

                    while (!hunt.IsFinished)
                    {
                        if (await FindAndKillMob(hunt.NpcID))
                        {
                            Log.Information($"Killed a {hunt.MobName}");
                            await Coroutine.Sleep(1000);
                            if (!Core.Me.InCombat)
                            {
                                if (Core.Me.IsMounted)
                                {
                                    ActionManager.Dismount();
                                }

                                await Coroutine.Sleep(3000);
                            }
                        }
                        else
                        {
                            Log.Warning("None found, sleeping 10 sec.");
                            await Coroutine.Sleep(10000);
                        }
                    }
                }
                else if (umbra.Contains(hunt.HuntTarget))
                {
                    await Navigation.GetToIslesOfUmbra();
                    if (await Navigation.GetTo(hunt.MapId, hunt.Location))
                    {
                        while (!hunt.IsFinished)
                        {
                            if (await FindAndKillMob(hunt.NpcID))
                            {
                                Log.Information($"Killed a {hunt.MobName}");
                                await Coroutine.Sleep(1000);
                                if (!Core.Me.InCombat)
                                {
                                    await Coroutine.Sleep(3000);
                                }
                            }
                            else
                            {
                                Log.Warning("None found, sleeping 10 sec.");
                                await Coroutine.Sleep(10000);
                            }
                        }
                    }
                }
                else if (hunt.MapId >= 399)
                {
                    if (!await Lisbeth.TravelToZones(hunt.MapId, hunt.Location))
                    {
                        await Navigation.GetTo(hunt.MapId, hunt.Location);
                    }

                    if (Core.Me.Location.Distance2DSqr(hunt.Location) <= 10f)
                    {
                        while (!hunt.IsFinished)
                        {
                            if (await FindAndKillMob(hunt.NpcID))
                            {
                                Log.Information($"Killed a {hunt.MobName}");
                                await Coroutine.Sleep(1000);
                                if (!Core.Me.InCombat)
                                {
                                    await Coroutine.Sleep(3000);
                                }
                            }
                            else
                            {
                                Log.Error("None found, sleeping 10 sec.");
                                await Coroutine.Sleep(10000);
                            }
                        }
                    }
                }
                else if (await Lisbeth.TravelToZones(hunt.MapId, hunt.Location))
                {
                    while (!hunt.IsFinished)
                    {
                        if (await FindAndKillMob(hunt.NpcID))
                        {
                            Log.Information($"Killed a {hunt.MobName}");
                            await Coroutine.Sleep(1000);
                            if (!Core.Me.InCombat)
                            {
                                await Coroutine.Sleep(3000);
                            }
                        }
                        else
                        {
                            Log.Warning("None found, sleeping max 10 sec.");
                            await Coroutine.Wait(10000, () => FindMob(hunt.NpcID));
                        }
                    }
                }
                else if (await Navigation.GetTo(hunt.MapId, hunt.Location))
                {
                    var lastChance = DateTime.Now.AddMinutes(10);
                    while (!hunt.IsFinished)
                    {
                        if (Blacklist.Contains(hunt.NpcID))
                        {
                            Log.Error($"{hunt.MobName} is blacklisted so skipping");
                            break;
                        }

                        if (await FindAndKillMob(hunt.NpcID))
                        {
                            lastChance = DateTime.Now.AddMinutes(5);
                            Log.Information($"Killed a {hunt.MobName}");
                            await Coroutine.Sleep(1000);
                            if (!Core.Me.InCombat)
                            {
                                await Coroutine.Sleep(3000);
                            }
                        }
                        else
                        {
                            if (DateTime.Now > lastChance)
                            {
                                Log.Warning("It's been 5 minutes, giving up");
                                break;
                            }

                            Log.Warning("None found, sleeping max 10 sec.");
                            await Coroutine.Wait(10000, () => FindMob(hunt.NpcID));
                        }
                    }
                }

                Log.Information($"Done: {hunt}");
                Blacklist.Clear();
                await Coroutine.Sleep(1000);
            }

            /*foreach (var orderType in dailyOrderTypes)
            {
                var dailies = HuntHelper.GetAcceptedDailyHunts(orderType).OrderBy(i=> i.MapId).ThenBy(j => j.Location.X);
                if (dailies.Any(i => !i.IsFinished))
                {
                    foreach (var hunt in dailies.Where(i => !i.IsFinished))
                    {

                }
            }*/

            while (Core.Me.InCombat)
            {
                var target = GameObjectManager.Attackers.FirstOrDefault(i => i.InCombat);
                if (target != default(BattleCharacter) && target.IsValid && target.IsAlive)
                {
                    //await Navigation.GetTo(WorldManager.ZoneId, target.Location);
                    await Lisbeth.Kill(target);
                }
            }
        }

        public static bool FindMob(uint npcId)
        {
            uint[] fateExmpt = { 252, 339, 375 };
            var mob = GameObjectManager.GetObjectsOfType<BattleCharacter>(true).Where(i => i.NpcId == npcId && i.IsValid && i.IsAlive && !(i.IsFate && !fateExmpt.Contains(npcId)) && !Blacklist.Contains(i.ObjectId))
                .OrderBy(r => r.Distance()).FirstOrDefault();
            if (FirstAttacker != null && (Core.Me.InCombat && GameObjectManager.Attackers.FirstOrDefault() != null && FirstAttacker.IsAlive))
            {
                return true;
            }

            if (mob == default(BattleCharacter))
            {
                Log.Error($"Couldn't find mob with NPCID {npcId}");
                return false;
            }
            else
            {
                return true;
            }
        }

        public static async Task<bool> FindAndKillMob(uint npcId)
        {
            uint[] fateExmpt = { 252, 339, 375 };
            while (FirstAttacker != null && (Core.Me.InCombat && GameObjectManager.Attackers.FirstOrDefault() != null && FirstAttacker.IsAlive))
            {
                var target = GameObjectManager.Attackers.FirstOrDefault();
                if (target != default(BattleCharacter) && target.IsValid && target.IsAlive)
                {
                    //await Navigation.GetTo(WorldManager.ZoneId, target.Location);
                    Bool0 = false;

                    //await KillMob(target);
                    await Lisbeth.Kill(target);
                }
            }

            var mob = GameObjectManager.GetObjectsOfType<BattleCharacter>(true).Where(i => i.NpcId == npcId && i.IsValid && i.IsAlive && !(i.IsFate && !fateExmpt.Contains(npcId)) && !Blacklist.Contains(i.ObjectId))
                .OrderBy(r => r.Distance()).FirstOrDefault();

            if (mob == default(BattleCharacter))
            {
                Log.Error($"Couldn't find mob with NPCID {npcId}");
                return false;
            }
            else
            {
                Log.Information($"Found mob {mob}");
                if ((Core.Me.Distance(mob) + 1 >= RoutineManager.Current.PullRange) && await Navigation.GenerateNodes(WorldManager.ZoneId, mob.Location) == null)
                {
                    Log.Warning("Can't get to, blacklisting mob");
                    Blacklist.Add(mob.ObjectId);
                    return false;
                }

                //await KillMob(mob);
                await Lisbeth.Kill(mob);
                Log.Debug($"Did we kill it? {!(mob.IsValid && mob.IsAlive)}");
                Navigator.PlayerMover.MoveStop();
                return !(mob.IsValid && mob.IsAlive);
            }
        }

        public static async Task KillMob2(BattleCharacter mob)
        {
            if (!mob.IsValid || !mob.IsAlive)
            {
                return;
            }

            var test = new Poi(mob, PoiType.Kill);
            Poi.Current = test;
            while (mob.IsValid && mob.IsAlive && Poi.Current != null && Poi.Current.Unit != null)
            {
                //("Looping combat");
                if (!await CombatCoroutine().ExecuteCoroutine())
                {
                    Log.Debug("Looping combat Composite False");
                    break;
                }

                GameObjectManager.Update();
                await Coroutine.Yield();
                if (mob.IsValid && mob.IsAlive && Poi.Current != null && Poi.Current.Unit != null)
                {
                    Log.Debug($"Looping combat {mob} {Poi.Current} {mob.IsValid && mob.IsAlive && Poi.Current != null && Poi.Current.Unit != null} ");
                    await Coroutine.Sleep(500);
                    Log.Debug($"Looping combat {mob.IsValid && mob.IsAlive && Poi.Current != null && Poi.Current.Unit != null} ");
                }
                else
                {
                    Log.Information("Combat Done");
                    break;
                }

                await Coroutine.Yield();
            }
        }

        private static Composite CombatCoroutine()
        {
            return new PrioritySelector(
                new Decorator(
                    object0 => !InFight && !Core.Me.IsDead,
                    new PrioritySelector(
                        new HookExecutor("Rest", "", new ActionAlwaysFail()),
                        new HookExecutor("PreCombatBuff", "", new ActionAlwaysFail()))),
                new Decorator(
                    object0 => Core.Me.IsDead || Poi.Current == null || Poi.Current.BattleCharacter == null,
                    new Action(context =>
                    {
                        Poi.Clear("Invalid Combat Poi");
                    })),
                new Decorator(
                    object0 => !Poi.Current.Unit.IsValid || Poi.Current.BattleCharacter.IsDead || Poi.Current.Unit.IsFateGone,
                    new Action(context =>
                    {
                        Poi.Clear("Targeted unit is dead, clearing Poi and carrying on!");
                        return RunStatus.Failure;
                    })),
                new Decorator(
                    object0 => Poi.Current != null && Poi.Current.Unit != null && Poi.Current.Unit.IsValid && Poi.Current.Unit.Pointer != Core.Me.PrimaryTargetPtr && Poi.Current.Unit.Distance() < 30f,
                    new Action(context =>
                    {
                        Poi.Current.Unit.Target();
                    })),
                new Decorator(
                    object0 => Core.Me.PrimaryTargetPtr == IntPtr.Zero,
                    new Action(context =>
                    {
                        Poi.Clear("Targeted unit is zero, clearing Poi and carrying on!");
                        return RunStatus.Failure;
                    })),
                new HookExecutor("PreCombatLogic"),
                new Decorator(
                    object0 => Core.Me.PrimaryTargetPtr != IntPtr.Zero,
                    new PrioritySelector(
                        new Decorator(
                            object0 => Core.Player.IsMounted && Core.Target.Location.Distance2D(Core.Player.Location) < Core.Me.CombatReach + RoutineManager.Current.PullRange,
                            new Action(context =>
                            {
                                ActionManager.Dismount();
                                Navigator.Stop();
                            })),
                        new Decorator(
                            object0 => !Core.Me.InCombat || (!Core.Me.InCombat && !Poi.Current.BattleCharacter.InCombat),
                            new HookExecutor("Pull", "Run when pulling a mob to kill.", RoutineManager.Current.PullBehavior)),
                        new Decorator(
                            object0 => Core.Me.InCombat,
                            new PrioritySelector(
                                new Decorator(
                                    object0 => PostCombatDelay > 0f && !Bool0,
                                    new Action(context =>
                                    {
                                        Bool0 = true;
                                        return RunStatus.Failure;
                                    })),
                                new Decorator(
                                    object0 => !Poi.Current.BattleCharacter.InCombat && Poi.Current.TimeSet + TimeSpan.FromSeconds(10.0) < DateTime.Now,
                                    new Action(context =>
                                    {
                                        Poi.Clear("I'm in combat, but POI isn't and it has been at least 10 seconds. Clearing POI and picking up a new target.");
                                    })),
                                new Decorator(
                                    object0 => Poi.Current.BattleCharacter.IsDead,
                                    new Action(context =>
                                    {
                                        Poi.Clear("I'm in combat, but POI is dead. Clearing POI and picking up a new target.");
                                    })),
                                new HookExecutor("RoutineCombat", "Executes the current routine's Combat behavior", BrainBehavior.CombatLogic))))),
                new Action(object0 => RunStatus.Success));
        }
    }
}