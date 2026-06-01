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
                Log.Error($"NavGraph.GetPathAsync failed with an exception");
                Log.Error(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Navigates to a specific <see cref="Location"/> (zone and coordinates).
        /// </summary>
        /// <param name="location">The destination location.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetTo(Location location)
        {
            return await GetTo(location.ZoneId, location.Coordinates);
        }

        /// <summary>
        /// Navigates to a specific location in a zone using Lisbeth's travel system if possible,
        /// falling back to standard navigation if Lisbeth is unavailable or fails.
        /// </summary>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToWithLisbeth(uint ZoneId, double x, double y, double z)
        {
            return await GetToWithLisbeth(ZoneId, new Vector3((float)x, (float)y, (float)z));
        }

        /// <summary>
        /// Navigates to a specific location in a zone using Lisbeth's travel system if possible,
        /// falling back to standard navigation if Lisbeth is unavailable or fails.
        /// </summary>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="XYZ">The destination coordinates.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToWithLisbeth(uint ZoneId, Vector3 XYZ)
        {
            if (!await Lisbeth.TravelToZones(ZoneId, XYZ))
            {
                return await GetTo(ZoneId, XYZ);
            }

            return true;
        }

        /// <summary>
        /// Navigates to a specific location in a zone using coordinates.
        /// </summary>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetTo(uint ZoneId, double x, double y, double z)
        {
            return await GetTo(ZoneId, new Vector3((float)x, (float)y, (float)z));
        }

        /// <summary>
        /// Navigates to a specific <see cref="Location"/> on a specific <see cref="World"/>.
        /// Handles cross-world travel if necessary.
        /// </summary>
        /// <param name="world">The destination world.</param>
        /// <param name="location">The destination location.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetTo(World world, Location location)
        {
            return await GetTo(world, location.ZoneId, location.Coordinates);
        }

        /// <summary>
        /// Navigates to a specific <see cref="WorldLocation"/>.
        /// Handles cross-world travel if necessary.
        /// </summary>
        /// <param name="worldLocation">The destination world and location.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetTo(WorldLocation worldLocation)
        {
            return await GetTo(worldLocation.World, worldLocation.Location);
        }

        /// <summary>
        /// Navigates to a specific location in a zone on a specific <see cref="World"/>.
        /// Handles cross-world travel if necessary.
        /// </summary>
        /// <param name="world">The destination world.</param>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="XYZ">The destination coordinates.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetTo(World world, uint ZoneId, Vector3 XYZ)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetTo(ZoneId, XYZ);
        }

        /// <summary>
        /// Navigates to a specific location in a zone using the navigation graph or flight.
        /// </summary>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="XYZ">The destination coordinates.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        /// <example>
        /// <code>
        /// await Navigation.GetTo(148, new Vector3(199.5991f, -32.04532f, 324.2699f));
        /// </code>
        /// </example>
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
                Log.Information($"Using housing traveler to get to residential area {ZoneId}");
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
        /// <summary>
        /// Navigates to a specific location in a zone, arriving within a specified distance tolerance.
        /// </summary>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="XYZ">The destination coordinates.</param>
        /// <param name="distance">The distance tolerance for arrival.</param>
        /// <returns><see langword="true"/> if the destination was reached within the tolerance; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetTo(uint ZoneId, Vector3 XYZ, float distance)
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
                Log.Information($"Using housing traveler to get to residential area {ZoneId}");
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

            var newPath = path.ToList();

            var lastNode = newPath.Last();
            var lastNodeIndex = newPath.IndexOf(lastNode);
            newPath.RemoveAt(lastNodeIndex);
            newPath.Reverse();
            var newQueue = new Queue<NavGraph.INode>();
            newQueue.Enqueue(new TestNode(lastNode.Id, (ushort)ZoneId, XYZ, distance));
            foreach (var node in newPath)
                newQueue.Enqueue(node);

            var composite = NavGraph.NavGraphConsumer(j => newQueue);

            while (newQueue.Count > 0)
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

            Log.Information($"Distance to {XYZ} on {ZoneId}: {Core.Me.Location.Distance(XYZ)}");

            return Navigator.InPosition(Core.Me.Location, XYZ, 3);
        }

        internal class TestNode : NavGraph.CustomLogicNode
        {
            private readonly float distance;
            private MoveToParameters moveToParameters;
            private MoveResult lastMoveResult = MoveResult.GeneratingPath;

            public TestNode(uint id, ushort zone, Vector3 loc, float distance)
            {
                Id = id;
                ZoneId = zone;
                EndZone = zone;
                Location = loc;
                EndLocation = loc;
                this.distance = Math.Max(distance, 1f);
                moveToParameters = new MoveToParameters(loc);
                moveToParameters.DistanceTolerance = this.distance;
                moveToParameters.MapId = zone;
            }

            public override bool ShouldPop =>
                WorldManager.ZoneId == ZoneId &&
                Core.Me.Location.Distance2D(Location) <= distance &&
                !MovementManager.IsMoving;

            public override async Task<bool> Logic()
            {
                if (WorldManager.ZoneId != ZoneId)
                    return true;

                if (Core.Me.Location.Distance2D(Location) <= distance)
                {
                    Navigator.Stop();
                    await Coroutine.Wait(1000, () => !MovementManager.IsMoving);
                    return true;
                }

                await CommonTasks.MoveTo(moveToParameters);
                return true;
            }
        }

        /// <summary>
        /// Moves towards a target location without using the navigation mesh.
        /// Stops if it takes longer than the <see cref="WaitTimer_0"/> duration or reaches within 4 yalms.
        /// </summary>
        /// <param name="_target">The destination coordinates.</param>
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

        /// <summary>
        /// Moves towards a target object without using the navigation mesh until it is within interact range.
        /// </summary>
        /// <param name="_target">The target game object to move towards.</param>
        /// <returns><see langword="true"/> if the object was reached within interact range; otherwise <see langword="false"/>.</returns>
        /// <example>
        /// <code>
        /// await Navigation.OffMeshMoveInteract(unit);
        /// </code>
        /// </example>
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

        /// <summary>
        /// Navigates to an NPC and uses them to transition between zones (e.g., using a ferry or talk-to-travel NPC).
        /// Handles the interaction, dialog, and loading screen.
        /// </summary>
        /// <param name="oldzone">The current zone ID.</param>
        /// <param name="transition">The coordinates of the NPC or transition point.</param>
        /// <param name="npcId">The NPC ID to interact with.</param>
        /// <param name="dialogOption">The index of the dialog option to select.</param>
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

        /// <summary>
        /// Specifically handles navigation to Map 399 (The Diadem) from the Foundation entrance.
        /// </summary>
        /// <returns><see langword="true"/> if the transition to Map 399 was successful; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Uses Flightor to move to a destination, returning true if successfully reached.
        /// </summary>
        /// <param name="loc">The destination coordinates.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
        /// <example>
        /// <code>
        /// await Navigation.FlightorMove(target);
        /// </code>
        /// </example>
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

        /// <summary>
        /// Uses Flightor to move to a destination, stopping if the destination is reached or if the <paramref name="stopCondition"/> is met.
        /// </summary>
        /// <param name="loc">The destination coordinates.</param>
        /// <param name="stopCondition">A function that returns <see langword="true"/> when movement should stop.</param>
        /// <returns><see langword="true"/> if the destination was reached or the stop condition was met; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Uses Flightor to move to a destination, arriving within a specified distance tolerance.
        /// </summary>
        /// <param name="loc">The destination coordinates.</param>
        /// <param name="distance">The distance tolerance for arrival.</param>
        /// <returns><see langword="true"/> if the destination was reached within the tolerance; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Uses Flightor to move to a FATE location, stopping if reached or if the FATE ends.
        /// </summary>
        /// <param name="fate">The FATE data containing the destination location.</param>
        /// <returns><see langword="true"/> if the FATE location was reached; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Uses the standard navigator to move to a destination on the ground, arriving within a specified distance tolerance.
        /// Includes stuck detection and will attempt to recover or fail if progress is not made.
        /// </summary>
        /// <param name="loc">The destination coordinates.</param>
        /// <param name="distance">The distance tolerance for arrival.</param>
        /// <returns><see langword="true"/> if the destination was reached within the tolerance; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Navigates to a specific Aetheryte by its NPC ID.
        /// </summary>
        /// <example>
        /// <code>
        /// if (WorldManager.ZoneId != 144 &amp;&amp; await Navigation.GetToAE(62) == default)
        /// {
        ///     Log.Error("Could not reach aetheryte.");
        /// }
        /// </code>
        /// </example>
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

        /// <summary>
        /// Specifically handles navigation to the Isles of Umbra (Western La Noscea),
        /// using the ferry NPC if necessary.
        /// </summary>
        /// <returns><see langword="true"/> if the player is at the Isles of Umbra; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Navigates to a specific <see cref="Npc"/>, handling housing travel or zone travel as needed.
        /// Moves within interact range of the NPC.
        /// </summary>
        /// <param name="npc">The NPC to travel to.</param>
        /// <returns><see langword="true"/> if the NPC was reached; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Navigates to a specific <see cref="Npc"/> using flight, handling zone travel via teleportation if needed.
        /// Moves within interact range of the NPC.
        /// </summary>
        /// <param name="npc">The NPC to fly to.</param>
        /// <returns><see langword="true"/> if the NPC was reached; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Navigates to a specific location in a zone using flight, handling teleportation if the player is in a different zone.
        /// </summary>
        /// <param name="ZoneId">The ID of the destination zone.</param>
        /// <param name="XYZ">The destination coordinates.</param>
        /// <returns><see langword="false"/> (always returns false based on current implementation logic, but completes movement).</returns>
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

        /// <summary>
        /// Navigates to an NPC and interacts with them until a specific remote window is open.
        /// </summary>
        /// <example>
        /// <code>
        /// if (!await Navigation.GetToInteractNpc(vendor, ShopProxy.Instance))
        /// {
        ///     Log.Error("Could not reach and interact with vendor.");
        /// }
        /// </code>
        /// </example>
        public static async Task<bool> GetToInteractNpc(Npc? npc, RemoteWindow window)
        {
            if (npc == null)
            {
                return false;
            }

            return await GetToInteractNpc(npc.NpcId, npc.Location.ZoneId, npc.Location.Coordinates, window);
        }

        /// <summary>
        /// Navigates to a specific NPC and interacts with them until a specific remote window is open.
        /// </summary>
        /// <param name="npcId">The NPC ID to interact with.</param>
        /// <param name="zoneId">The ID of the zone where the NPC is located.</param>
        /// <param name="location">The coordinates of the NPC.</param>
        /// <param name="window">The remote window expected to open.</param>
        /// <returns><see langword="true"/> if the window is open; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Navigates to an NPC, interacts with them, and selects an option from the SelectString window.
        /// </summary>
        /// <example>
        /// <code>
        /// if (!await LlamaLibrary.Helpers.Navigation.GetToInteractNpcSelectString(jalzahn))
        /// {
        ///     Log.Error("Could not navigate and select string.");
        /// }
        /// </code>
        /// </example>
        public static async Task<bool> GetToInteractNpcSelectString(Npc npc, int option = -1)
        {
            return await GetToInteractNpcSelectString(npc.NpcId, npc.Location.ZoneId, npc.Location.Coordinates, option);
        }

        /// <summary>
        /// Navigates to an NPC, interacts with them, and selects an option from the SelectString window,
        /// then waits for a specific remote window to open.
        /// </summary>
        /// <param name="npc">The NPC to interact with.</param>
        /// <param name="option">The index of the dialog option to select.</param>
        /// <param name="window">The remote window expected to open after selection.</param>
        /// <returns><see langword="true"/> if the window is open; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToInteractNpcSelectString(Npc npc, int option, RemoteWindow window)
        {
            return await GetToInteractNpcSelectString(npc.NpcId, npc.Location.ZoneId, npc.Location.Coordinates, option, window);
        }

        /// <summary>
        /// Navigates to an NPC, interacts with them, and optionally selects an option from the SelectString window.
        /// Can also wait for a subsequent window to open.
        /// </summary>
        /// <param name="npcId">The NPC ID to interact with.</param>
        /// <param name="zoneId">The ID of the zone where the NPC is located.</param>
        /// <param name="location">The coordinates of the NPC.</param>
        /// <param name="selectStringIndex">The index of the dialog option to select, or -1 to skip selection.</param>
        /// <param name="nextWindow">An optional remote window to wait for after selection.</param>
        /// <returns><see langword="true"/> if the interaction was successful or the target window is open; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Finds the primary aetheryte ID for a given zone and location.
        /// If the zone has multiple aetherytes, it returns the one closest to the location.
        /// If the location is near an aethernet shard, it resolves the primary aetheryte for that shard's network.
        /// </summary>
        /// <param name="zoneId">The zone ID to search.</param>
        /// <param name="location">The coordinates near which to search.</param>
        /// <returns>The ID of the primary aetheryte, or 0 if none found.</returns>
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

        /// <summary>
        /// Finds the primary aetheryte ID for a given zone.
        /// If the zone has multiple aetherytes, it returns the first one found.
        /// If the zone only has aethernet shards, it resolves the primary aetheryte for their network.
        /// </summary>
        /// <param name="zoneId">The zone ID to search.</param>
        /// <returns>The ID of the primary aetheryte, or 0 if none found.</returns>
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

        /// <summary>
        /// Generates a random <see cref="Location"/> at a specific distance and heading from the player,
        /// with a random angular offset applied.
        /// </summary>
        /// <param name="range">The max angular offset range (in tenths of a radian).</param>
        /// <param name="distance">The distance from the player.</param>
        /// <param name="heading">The base heading to offset from.</param>
        /// <returns>A new <see cref="Location"/> object.</returns>
        public static Location GetRandomPoint(int range, float distance, float heading)
        {
            var point = Random.Next(-range, range) / 10f;
            return new Location(WorldManager.ZoneId, MathEx.GetPointAt(Core.Me.Location, distance, heading + point));
        }

        /// <summary>
        /// Generates a random <see cref="Location"/> at a specific distance from a game object,
        /// using the object's heading as the base.
        /// </summary>
        /// <param name="range">The max angular offset range (in tenths of a radian).</param>
        /// <param name="distance">The distance from the object.</param>
        /// <param name="gameObject">The source game object.</param>
        /// <returns>A new <see cref="Location"/> object.</returns>
        public static Location GetRandomPoint(int range, float distance, GameObject gameObject)
        {
            return GetRandomPoint(range, distance, gameObject.Heading);
        }

        /// <summary>
        /// Navigates to a spot roughly in front of a specific NPC.
        /// Uses the navigation graph to get close, then moves to a random point relative to the NPC.
        /// </summary>
        /// <param name="zoneId">The zone ID.</param>
        /// <param name="location">The NPC's coordinates.</param>
        /// <param name="npcId">The NPC ID.</param>
        /// <param name="distance">The desired distance from the NPC.</param>
        /// <param name="range">The angular range for the random offset.</param>
        /// <returns><see langword="true"/> if reached within interact range of the NPC; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Navigates to a spot roughly in front of a specific <see cref="Npc"/>.
        /// </summary>
        /// <param name="npc">The NPC.</param>
        /// <param name="distance">The desired distance from the NPC.</param>
        /// <param name="range">The angular range for the random offset.</param>
        /// <returns><see langword="true"/> if reached within interact range of the NPC; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToSpotInFrontOf(Npc npc, float distance = 4f, int range = 6)
        {
            return await GetToSpotInFrontOf(npc.Location.ZoneId, npc.Location.Coordinates, npc.NpcId, distance, range);
        }

        /// <summary>
        /// Navigates to a spot roughly in front of an NPC at a specific <see cref="Location"/>.
        /// </summary>
        /// <param name="location">The NPC's location.</param>
        /// <param name="npcId">The NPC ID.</param>
        /// <param name="distance">The desired distance from the NPC.</param>
        /// <param name="range">The angular range for the random offset.</param>
        /// <returns><see langword="true"/> if reached within interact range of the NPC; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToSpotInFrontOf(Location location, uint npcId, float distance = 4f, int range = 6)
        {
            return await GetToSpotInFrontOf(location.ZoneId, location.Coordinates, npcId, distance, range);
        }

        /// <summary>
        /// Navigates to a destination using the navigation graph if available,
        /// falling back to a direct ground move if no path is found.
        /// </summary>
        /// <param name="location">The destination coordinates.</param>
        /// <returns><see langword="true"/> if the destination was reached; otherwise <see langword="false"/>.</returns>
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