using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Common;
using Clio.Utilities;
using Clio.Utilities.Helpers;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Helpers.WorldTravel;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaLibrary.Helpers
{
    public static class Navigation
    {
        private static readonly LLogger Log = new("NavigationHelper", Colors.MediumPurple);

        public static readonly WaitTimer WaitTimer_0 = new(new TimeSpan(0, 0, 0, 15));

        public static readonly Random Random = new();

        internal static async Task<Queue<NavGraph.INode>?> GenerateNodes(uint ZoneId, Vector3 xyz)
        {
            Log.Information($"Getpath {ZoneId} {xyz}");
            try
            {
                return await NavGraph.GetPathAsync(ZoneId, xyz);
            }
            catch (Exception ex)
            {
                Log.Error("NavGraph.GetPathAsync failed with an exception");
                return null;
            }
        }

        public static async Task<bool> GetTo(Location location)
        {
            return await GetTo(location.ZoneId, location.Coordinates);
        }

        public static async Task<bool> GetToWithLisbeth(uint ZoneId, double x, double y, double z)
        {
            return await GetToWithLisbeth(ZoneId, new Vector3((float)x, (float)y, (float)z));
        }

        public static async Task<bool> GetToWithLisbeth(uint ZoneId, Vector3 XYZ)
        {
            if (!await Lisbeth.TravelToZones(ZoneId, XYZ))
            {
                return await GetTo(ZoneId, XYZ);
            }

            return true;
        }

        public static async Task<bool> GetTo(uint ZoneId, double x, double y, double z)
        {
            return await GetTo(ZoneId, new Vector3((float)x, (float)y, (float)z));
        }

        public static async Task<bool> GetTo(World world, Location location)
        {
            return await GetTo(world, location.ZoneId, location.Coordinates);
        }

        public static async Task<bool> GetTo(WorldLocation worldLocation)
        {
            return await GetTo(worldLocation.World, worldLocation.Location);
        }

        public static async Task<bool> GetTo(World world, uint ZoneId, Vector3 XYZ)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetTo(ZoneId, XYZ);
        }

        public static async Task<bool> GetTo(uint ZoneId, Vector3 XYZ)
        {
            if (Navigator.NavigationProvider == null)
            {
                Navigator.PlayerMover = new SlideMover();
                Navigator.NavigationProvider = new ServiceNavigationProvider();
            }

            /*if (ZoneId == 620)
            {
                var AE = WorldManager.AetheryteIdsForZone(ZoneId).OrderBy(i => i.Item2.DistanceSqr(XYZ)).First();
                Log.Debug("Can teleport to AE");
                WorldManager.TeleportById(AE.Item1);
                await Coroutine.Wait(20000, () => WorldManager.ZoneId == AE.Item1);
                await Coroutine.Sleep(2000);
                return await FlightorMove(XYZ);
            }*/

            if (ZoneId is 534 or 535 or 536 && WorldManager.ZoneId != ZoneId && !await GrandCompanyHelper.GetToGCBarracks())
            {
                return false;
            }

            if (HousingTraveler.HousingZoneIds.Contains((ushort)ZoneId))
            {
                var ward = 1;

                if (HousingHelper.IsInHousingArea && WorldManager.ZoneId == ZoneId)
                {
                    ward = HousingHelper.HousingPositionInfo.Ward;
                }

                return await HousingTraveler.GetToResidential((ushort)ZoneId, XYZ, ward);
            }

            if (ZoneId == 401 && WorldManager.ZoneId == ZoneId)
            {
                return await FlightorMove(XYZ);
            }

            var path = await GenerateNodes(ZoneId, XYZ);

            if (ZoneId == 399 && path == null && WorldManager.ZoneId != ZoneId)
            {
                await GetToMap399();
            }

            if (path == null && WorldManager.ZoneId != ZoneId)
            {
                if (WorldManager.AetheryteIdsForZone(ZoneId).Length >= 1)
                {
                    var AE = WorldManager.AetheryteIdsForZone(ZoneId).OrderBy(i => i.Item2.DistanceSqr(XYZ)).First();

                    Log.Verbose("Can teleport to AE");
                    await Coroutine.Sleep(1000);
                    await TeleportHelper.TeleportByIdTicket(AE.Item1);
                    await Coroutine.Sleep(1000);
                    return await GetTo(ZoneId, XYZ);
                }

                return false;
            }

            if (path == null)
            {
                var result = await FlightorMove(XYZ);
                Navigator.Stop();
                return result;
            }

            if (path.Count < 1)
            {
                Log.Error($"Couldn't get a path to {XYZ} on {ZoneId}, Stopping.");
                return false;
            }

            var object_0 = new object();
            var composite = NavGraph.NavGraphConsumer(j => path);

            while (path.Count > 0)
            {
                composite.Start(object_0);
                await Coroutine.Yield();
                while (composite.Tick(object_0) == RunStatus.Running)
                {
                    await Coroutine.Yield();
                }

                composite.Stop(object_0);
                await Coroutine.Yield();
            }

            Navigator.Stop();

            return Navigator.InPosition(Core.Me.Location, XYZ, 3);
        }

        public static async Task OffMeshMove(Vector3 _target)
        {
            WaitTimer_0.Reset();
            Navigator.PlayerMover.MoveTowards(_target);
            while (_target.Distance2D(Core.Me.Location) >= 4 && !WaitTimer_0.IsFinished)
            {
                Navigator.PlayerMover.MoveTowards(_target);
                await Coroutine.Sleep(100);
            }

            Navigator.PlayerMover.MoveStop();
        }

        public static async Task<bool> OffMeshMoveInteract(GameObject _target)
        {
            WaitTimer_0.Reset();
            Navigator.PlayerMover.MoveTowards(_target.Location);
            while (!_target.IsWithinInteractRange && !WaitTimer_0.IsFinished)
            {
                Navigator.PlayerMover.MoveTowards(_target.Location);
                await Coroutine.Sleep(100);
            }

            Navigator.PlayerMover.MoveStop();
            return _target.IsWithinInteractRange;
        }

        public static async Task UseNpcTransition(uint oldzone, Vector3 transition, uint npcId, uint dialogOption)
        {
            await GetTo(oldzone, transition);

            var unit = GameObjectManager.GetObjectByNPCId(npcId);

            if (!unit.IsWithinInteractRange)
            {
                await OffMeshMoveInteract(unit);
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => SelectIconString.IsOpen || DialogOpen);

            if (DialogOpen)
            {
                Next();
            }

            if (SelectIconString.IsOpen)
            {
                SelectIconString.ClickSlot(dialogOption);

                await Coroutine.Wait(5000, () => DialogOpen || SelectYesno.IsOpen);
            }

            if (DialogOpen)
            {
                Next();
            }

            await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
            }

            await Coroutine.Wait(3000, () => !SelectYesno.IsOpen);
            await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(10000, () => !CommonBehaviors.IsLoading);
            }
        }

        public static async Task<bool> GetToMap399()
        {
            await GetTo(478, new Vector3(74.39938f, 205f, 140.4551f));
            Navigator.PlayerMover.MoveTowards(new Vector3(73.36626f, 205f, 142.026f));

            await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading);
            Navigator.Stop();
            await Coroutine.Sleep(1000);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            return WorldManager.ZoneId == 399;
        }

        public static async Task<bool> FlightorMove(Vector3 loc)
        {
            var moving = MoveResult.GeneratingPath;
            var target = new FlyToParameters(loc);
            while (moving is not (MoveResult.Done or
                   MoveResult.ReachedDestination or
                   MoveResult.Failed or
                   MoveResult.Failure or
                   MoveResult.PathGenerationFailed))
            {
                moving = Flightor.MoveTo(target);

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            Navigator.NavigationProvider.ClearStuckInfo();
            Navigator.Stop();
            await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
            return moving == MoveResult.ReachedDestination;
        }

        public static async Task<bool> FlightorMove(Vector3 loc, Func<bool> stopCondition)
        {
            var moving = MoveResult.GeneratingPath;
            var target = new FlyToParameters(loc);
            while (!(moving == MoveResult.Done ||
                     moving == MoveResult.ReachedDestination ||
                     moving == MoveResult.Failed ||
                     moving == MoveResult.Failure ||
                     moving == MoveResult.PathGenerationFailed || stopCondition()))
            {
                moving = Flightor.MoveTo(target);

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            Navigator.NavigationProvider.ClearStuckInfo();
            Navigator.Stop();
            await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
            return moving == MoveResult.ReachedDestination || stopCondition();
        }

        public static async Task<bool> FlightorMove(Vector3 loc, float distance)
        {
            var moving = MoveResult.GeneratingPath;
            var target = new FlyToParameters(loc);
            target.GroundNavParameters.DistanceTolerance = distance;
            while (!(moving == MoveResult.Done ||
                     moving == MoveResult.ReachedDestination ||
                     moving == MoveResult.Failed ||
                     moving == MoveResult.Failure ||
                     moving == MoveResult.PathGenerationFailed || Core.Me.Distance(loc) < distance))
            {
                moving = Flightor.MoveTo(target);

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            Navigator.NavigationProvider.ClearStuckInfo();
            Navigator.Stop();
            await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
            return Core.Me.Distance(loc) <= distance;
        }

        public static async Task<bool> FlightorMove(FateData? fate)
        {
            if (fate == null)
            {
                return false;
            }

            var moving = MoveResult.GeneratingPath;
            var target = new FlyToParameters(fate.Location);
            while ((!(moving == MoveResult.Done ||
                      moving == MoveResult.ReachedDestination ||
                      moving == MoveResult.Failed ||
                      moving == MoveResult.Failure ||
                      moving == MoveResult.PathGenerationFailed)) && FateManager.ActiveFates.Any(i => i.Id == fate.Id && i.IsValid))
            {
                moving = Flightor.MoveTo(target);

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            Navigator.NavigationProvider.ClearStuckInfo();
            Navigator.Stop();
            await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
            return moving == MoveResult.ReachedDestination;
        }

        public static async Task<bool> GroundMove(Vector3 loc, float distance)
        {
            Log.Information("Using Ground Move");
            if (Navigator.NavigationProvider == null)
            {
                Navigator.PlayerMover = new SlideMover();
                Navigator.NavigationProvider = new ServiceNavigationProvider();
            }

            var moving = MoveResult.GeneratingPath;
            var target = new MoveToParameters(loc);
            target.DistanceTolerance = distance;

            var lastLoc = Core.Me.Location;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (!(moving == MoveResult.Done ||
                     moving == MoveResult.ReachedDestination ||
                     moving == MoveResult.Failed ||
                     moving == MoveResult.Failure ||
                     moving == MoveResult.PathGenerationFailed || Core.Me.Distance(loc) < distance))
            {
                moving = Navigator.MoveTo(target);

                if (moving == MoveResult.Moved && sw.ElapsedMilliseconds > 5000)
                {
                    sw.Restart();
                    if (Core.Me.Location.Distance(lastLoc) < 1 || Core.Me.Location.Distance(loc) < distance)
                    {
                        Log.Error($"Seems like we're stuck, trying to move to {loc}");
                        moving = Core.Me.Location.Distance(loc) <= distance ? MoveResult.ReachedDestination : MoveResult.Failed;
                        break;
                    }

                    lastLoc = Core.Me.Location;
                    //return false;
                }

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            Navigator.NavigationProvider.ClearStuckInfo();
            Navigator.Stop();
            await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
            return Core.Me.Location.Distance(loc) <= distance;
        }

        public static async Task<GameObject?> GetToAE(uint id)
        {
            var AE = GameObjectManager.GetObjectsOfType<Aetheryte>().FirstOrDefault(i => i.NpcId == id);

            if (AE == default(Aetheryte))
            {
                if (!await Coroutine.Wait(5000, WorldManager.CanTeleport))
                {
                    Log.Information("After 5 seconds CanTeleport is still false");
                    return null;
                }

                if (!await CommonTasks.Teleport(id))
                {
                    Log.Error($"Couldn't teleport to AE {id}");
                    return default;
                }

                await Coroutine.Wait(5000, () => GameObjectManager.GetObjectByNPCId(id) != null);

                await Coroutine.Sleep(200);

                AE = GameObjectManager.GetObjectsOfType<Aetheryte>().FirstOrDefault(i => i.NpcId == id);
            }

            if (AE != null && !AE.IsWithinInteractRange)
            {
                Log.Information("Using flightor to get closer");
                await FlightorMove(AE.Location, 6);
            }

            if (AE != null && !AE.IsWithinInteractRange)
            {
                Log.Information("Using offmesh to get closer");
                await OffMeshMoveInteract(AE);
            }

            return AE;
        }

        public static async Task<bool> GetToIslesOfUmbra()
        {
            if (WorldManager.ZoneId == 138 && (WorldManager.SubZoneId == 461 || WorldManager.SubZoneId == 228))
            {
                return true;
            }

            await GetTo(138, new Vector3(317.4333f, -36.325f, 352.8649f));

            await UseNpcTransition(138, new Vector3(317.4333f, -36.325f, 352.8649f), 1003584, 2);

            await Coroutine.Sleep(1000);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            await Coroutine.Sleep(1000);
            return WorldManager.ZoneId == 138 && (WorldManager.SubZoneId == 461 || WorldManager.SubZoneId == 228);
        }

        public static async Task<bool> GetToNpc(Npc npc)
        {
            if (npc.CanGetTo)
            {
                if (npc.IsHousingZoneNpc)
                {
                    if (await HousingTraveler.GetToResidential(npc))
                    {
                        var unit = npc.GameObject;

                        if (unit != default && !unit.IsWithinInteractRange)
                        {
                            await OffMeshMoveInteract(unit);
                        }

                        return unit != null && unit.IsWithinInteractRange;
                    }
                }
                else if (await GetTo(npc.Location.ZoneId, npc.Location.Coordinates))
                {
                    var unit = GameObjectManager.GetObjectByNPCId(npc.NpcId);

                    if (unit != default && !unit.IsWithinInteractRange)
                    {
                        await OffMeshMoveInteract(unit);
                    }

                    return unit != null && unit.IsWithinInteractRange;
                }
            }

            return false;
        }

        public static async Task<bool> FlyToNpc(Npc npc)
        {
            if (WorldManager.ZoneId != npc.Location.ZoneId)
            {
                if (WorldManager.AetheryteIdsForZone(npc.Location.ZoneId).Length >= 1)
                {
                    var AE = WorldManager.AetheryteIdsForZone(npc.Location.ZoneId).OrderBy(i => i.Item2.DistanceSqr(npc.Location.Coordinates)).First();

                    Log.Information("Can teleport to AE");
                    await TeleportHelper.TeleportByIdTicket(AE.Item1);
                }
            }

            if (await FlightorMove(npc.Location.Coordinates))
            {
                var unit = GameObjectManager.GetObjectByNPCId(npc.NpcId);

                if (unit != default && !unit.IsWithinInteractRange)
                {
                    await OffMeshMoveInteract(unit);
                }

                return unit != null && unit.IsWithinInteractRange;
            }

            return false;
        }

        public static async Task<bool> FlyToWithZone(uint ZoneId, Vector3 XYZ)
        {
            if (WorldManager.ZoneId != ZoneId)
            {
                if (WorldManager.AetheryteIdsForZone(ZoneId).Length >= 1)
                {
                    var AE = WorldManager.AetheryteIdsForZone(ZoneId).OrderBy(i => i.Item2.DistanceSqr(XYZ)).First();

                    Log.Information("Can teleport to AE");
                    //await CommonTasks.Teleport(AE.Item1);
                    await TeleportHelper.TeleportByIdTicket(AE.Item1);
                }
            }

            await FlightorMove(XYZ);

            return false;
        }

        public static async Task<bool> GetToInteractNpc(Npc? npc, RemoteWindow window)
        {
            if (npc == null)
            {
                return false;
            }
            return await GetToInteractNpc(npc.NpcId, npc.Location.ZoneId, npc.Location.Coordinates, window);
        }

        public static async Task<bool> GetToInteractNpc(uint npcId, ushort zoneId, Vector3 location, RemoteWindow window)
        {
            var unit = GameObjectManager.GetObjectByNPCId(npcId);

            if (unit == default || !unit.IsWithinInteractRange)
            {
                if (!await GetTo(zoneId, location))
                {
                    return false;
                }
            }
            else if (window.IsOpen)
            {
                return true;
            }

            unit = GameObjectManager.GetObjectByNPCId(npcId);

            if (unit != default)
            {
                if (!unit.IsWithinInteractRange)
                {
                    await OffMeshMoveInteract(unit);
                }

                unit.Target();
                unit.Interact();

                await Coroutine.Wait(5000, () => window.IsOpen || DialogOpen);

                if (window.IsOpen && !DialogOpen)
                {
                    return true;
                }

                await Coroutine.Wait(20000, () => DialogOpen);
                if (DialogOpen)
                {
                    while (DialogOpen)
                    {
                        Next();
                        await Coroutine.Wait(500, () => !DialogOpen);
                        await Coroutine.Wait(500, () => DialogOpen);
                        await Coroutine.Yield();
                    }

                    await Coroutine.Wait(5000, () => window.IsOpen);
                }
            }

            return window.IsOpen;
        }

        public static async Task<bool> GetToInteractNpcSelectString(Npc npc, int option = -1)
        {
            return await GetToInteractNpcSelectString(npc.NpcId, npc.Location.ZoneId, npc.Location.Coordinates, option);
        }

        public static async Task<bool> GetToInteractNpcSelectString(Npc npc, int option, RemoteWindow window)
        {
            return await GetToInteractNpcSelectString(npc.NpcId, npc.Location.ZoneId, npc.Location.Coordinates, option, window);
        }

        public static async Task<bool> GetToInteractNpcSelectString(uint npcId, ushort zoneId, Vector3 location, int selectStringIndex = -1, RemoteWindow? nextWindow = null)
        {
            if (await GetTo(zoneId, location))
            {
                var unit = GameObjectManager.GetObjectByNPCId(npcId);

                if (unit != default)
                {
                    if (!unit.IsWithinInteractRange)
                    {
                        await OffMeshMoveInteract(unit);
                    }

                    unit.Target();
                    unit.Interact();

                    await Coroutine.Wait(5000, () => Conversation.IsOpen || DialogOpen);

                    if (DialogOpen)
                    {
                        while (DialogOpen)
                        {
                            Next();
                            await Coroutine.Wait(100, () => !DialogOpen);
                            await Coroutine.Wait(100, () => DialogOpen);
                            await Coroutine.Yield();
                        }

                        await Coroutine.Wait(5000, () => Conversation.IsOpen);
                    }
                }
            }

            if (selectStringIndex >= 0)
            {
                if (Conversation.IsOpen)
                {
                    Conversation.SelectLine((uint)selectStringIndex);
                    await Coroutine.Wait(5000, () => !Conversation.IsOpen || DialogOpen);

                    if (nextWindow != null)
                    {
                        await Coroutine.Wait(5000, () => nextWindow.IsOpen || DialogOpen);
                        if (DialogOpen)
                        {
                            while (DialogOpen)
                            {
                                Next();
                                await Coroutine.Wait(100, () => !DialogOpen);
                                await Coroutine.Wait(100, () => DialogOpen);
                                await Coroutine.Yield();
                            }

                            await Coroutine.Wait(5000, () => nextWindow.IsOpen);
                        }

                        return nextWindow.IsOpen;
                    }

                    return true;
                }
            }

            return Conversation.IsOpen;
        }

        public static uint GetPrimaryAetheryte(ushort zoneId, Vector3 location)
        {
            var aeList = DataManager.AetheryteCache.Values.Where(i => i.ZoneId == zoneId).ToList();

            if (aeList.Count == 0)
            {
                return 0;
            }

            if (aeList.Any(i => i.IsAetheryte))
            {
                return aeList.Where(i => i.IsAetheryte).OrderBy(i => i.Position.Distance2DSqr(location)).First().Id;
            }

            var group = aeList.First().AethernetGroup;

            var ae = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.IsAetheryte && i.AethernetGroup == group);

            return ae?.Id ?? 0;
        }

        public static uint GetPrimaryAetheryte(ushort zoneId)
        {
            var aeList = DataManager.AetheryteCache.Values.Where(i => i.ZoneId == zoneId).ToList();

            if (aeList.Count == 0)
            {
                return 0;
            }

            if (aeList.Any(i => i.IsAetheryte))
            {
                return aeList.First(i => i.IsAetheryte).Id;
            }

            var group = aeList.First().AethernetGroup;
            var ae = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.IsAetheryte && i.AethernetGroup == group);

            return ae?.Id ?? 0;
        }

        public static Location GetRandomPoint(int range, float distance, float heading)
        {
            var point = Random.Next(-range, range) / 10f;
            return new Location(WorldManager.ZoneId, MathEx.GetPointAt(Core.Me.Location, distance, heading + point));
        }

        public static Location GetRandomPoint(int range, float distance, GameObject gameObject)
        {
            return GetRandomPoint(range, distance, gameObject.Heading);
        }

        public static async Task<bool> GetToSpotInFrontOf(ushort zoneId, Vector3 location, uint npcId, float distance = 4f, int range = 6)
        {
            var path = await NavGraph.GetPathAsync(zoneId, location);

            if (path != null && path.Count != 0)
            {
                var object0 = new object();
                var composite = NavGraph.NavGraphConsumer(j => path);
                bool stop = false;
                while (path.Count > 0)
                {
                    composite.Start(object0);
                    await Coroutine.Yield();
                    while (composite.Tick(object0) == RunStatus.Running)
                    {
                        await Coroutine.Yield();
                        if (stop)
                        {
                            break;
                        }
                    }

                    composite.Stop(object0);
                    await Coroutine.Yield();

                    if (stop)
                    {
                        break;
                    }

                    if (GameObjectManager.GetObjectByNPCId(npcId) != null && (path.Count > 0 && path.Peek().Location.Distance3D(location) < distance * 2))
                    {
                        //Navigator.Stop();
                        //MovementManager.MoveStop();
                        Log.Information($"Found NPC {path.Count} {path.Peek().DynamicString()}");
                        stop = true;
                        //break;
                    }
                }

                Navigator.Stop();
            }
            else
            {
                if (WorldManager.ZoneId != zoneId && WorldManager.AetheryteIdsForZone(zoneId).Length >= 1)
                {
                    var AE = WorldManager.AetheryteIdsForZone(zoneId).OrderBy(i => i.Item2.DistanceSqr(location)).First();
                    Log.Verbose("Can teleport to AE");
                    await Coroutine.Sleep(1000);
                    await TeleportHelper.TeleportByIdTicket(AE.Item1);
                    await Coroutine.Sleep(1000);
                }
                else
                {
                    return false;
                }

                await GroundMove(location, 20f);
            }

            if (GameObjectManager.GetObjectByNPCId(npcId) == null)
            {
                Log.Error("Failed to find NPC");
                return false;
            }

            await Coroutine.Sleep(1000);

            var newLocation = GetRandomPoint(range, distance, GameObjectManager.GetObjectByNPCId(npcId));

            Log.Information($"Moving to spot in front of NPC {newLocation}");

            if (await NavGraphOrGround(newLocation.Coordinates))
            {
                Log.Information("Moved to spot in front of NPC");
            }
            else
            {
                Log.Error("Failed to move to spot in front of NPC");
            }

            var npc = GameObjectManager.GetObjectByNPCId(npcId);

            if (npc != null && !npc.IsWithinInteractRange)
            {
                await OffMeshMoveInteract(npc);
            }

            if (Core.Me.IsMounted)
            {
                await CommonTasks.StopAndDismount();
                await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
            }

            return npc != null && npc.IsWithinInteractRange;
        }

        public static async Task<bool> GetToSpotInFrontOf(Npc npc, float distance = 4f, int range = 6)
        {
            return await GetToSpotInFrontOf(npc.Location.ZoneId, npc.Location.Coordinates, npc.NpcId, distance, range);
        }

        public static async Task<bool> GetToSpotInFrontOf(Location location, uint npcId, float distance = 4f, int range = 6)
        {
            return await GetToSpotInFrontOf(location.ZoneId, location.Coordinates, npcId, distance, range);
        }

        public static async Task<bool> NavGraphOrGround(Vector3 location)
        {
            var path = await NavGraph.GetPathAsync(WorldManager.ZoneId, location);

            if (path == null || path.Count < 2)
            {
                Log.Information("No path found, using ground move");
                return await GroundMove(location, 1f);
            }

            var object0 = new object();
            var composite = NavGraph.NavGraphConsumer(j => path);

            if (path.Count > 1)
            {
                if (path.Peek().Location.Distance3D(Core.Me.Location) < 1)
                {
                    path.Dequeue();
                }
            }

            while (path.Count > 0)
            {
                composite.Start(object0);
                await Coroutine.Yield();
                while (composite.Tick(object0) == RunStatus.Running)
                {
                    await Coroutine.Yield();
                }

                composite.Stop(object0);
                await Coroutine.Yield();
            }

            Navigator.Stop();

            return Navigator.InPosition(Core.Me.Location, location, 3);
        }
    }
}