using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Classes;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helper methods for navigating to FFXIV inn rooms and interacting with in-game objects.
    /// Centralizes inn keeper discovery, glamour dresser navigation, and NPC interaction logic.
    /// </summary>
    public static class NavigationHelper
    {
        private static readonly LLogger Log = new("NavigationHelper", Colors.MediumPurple);

        /// <summary>
        /// The three base-game inn keepers gated behind Main Scenario Quest (MSQ) progression:
        /// Antoinaut in New Gridania (The Roost), Mytesyn in Limsa Lominsa Upper Decks (Mizzenmast Inn),
        /// and Otopa Pottopa in Ul'dah - Steps of Nald (The Hourglass).
        /// At least one of these must have its quest completed before inn room access is available.
        /// </summary>
        public static List<Npc> StartingInnKeepers = new()
        {
            new Npc(1000102, 132, new Vector3(26.2571f, -8.000001f, 100.4172f), 65665), //Antoinaut (Innkeep) New Gridania - The Roost
            new Npc(1000974, 128, new Vector3(12.93333f, 39.99998f, 11.71152f), 66005), //Mytesyn (Innkeep) Limsa Lominsa Upper Decks - Mizzenmast Inn
            new Npc(1001976, 130, new Vector3(29.31324f, 6.999999f, -80.32259f), 65856) //Otopa Pottopa (Innkeep) Ul'dah - Steps of Nald - The Hourglass
        };

        /// <summary>
        /// Inn keepers added by FFXIV expansions, each requiring a specific quest completion:
        /// Bamponcet in Foundation (Cloud Nine, Heavensward), Ushitora in Kugane (Bokairo Inn, Stormblood),
        /// the Manager of Suites in The Crystarium (The Pendants, Shadowbringers), and
        /// Ojika Tsunjika in Old Sharlayan (The Baldesion Annex, Endwalker).
        /// </summary>
        public static List<Npc> InnKeepers = new()
        {
            new Npc(1011193, 418, new Vector3(84.43542f, 15.09468f, 33.54416f), 67116), //Bamponcet (Innkeep) Foundation - Cloud Nine
            new Npc(1018981, 628, new Vector3(-86.10562f, 19f, -198.8428f), 68005), //Ushitora (Innkeep) Kugane - Bokairo Inn
            new Npc(1027231, 819, new Vector3(62.32343f, 1.716012f, 249.2582f), 68838), //manager of suites The Crystarium - The Pendants
            new Npc(1037293, 962, new Vector3(-100.3702f, 3.933468f, 2.429576f), 69915) //Ojika Tsunjika (Annex Administrator) Old Sharlayan - The Baldesion Annex
        };

        /// <summary>
        /// Zone IDs corresponding to inn room interiors across all expansions.
        /// The player is considered to be "in an inn room" when <c>WorldManager.ZoneId</c> matches any of these values.
        /// </summary>
        /// <remarks>
        /// Zone IDs: 177 (Gridania), 178 (Limsa Lominsa), 179 (Ul'dah), 429 (Foundation/Ishgard),
        /// 629 (Kugane), 843 (The Crystarium), 990 (Old Sharlayan).
        /// </remarks>
        public static ushort[] InnRoomZones =
        {
            177, 178, 179, 429, 629, 843, 990
        };

        /// <summary>
        /// Gets a value indicating whether the local player is currently inside any FFXIV inn room,
        /// determined by checking <c>WorldManager.ZoneId</c> against <see cref="InnRoomZones"/>.
        /// </summary>
        /// <value><see langword="true"/> if the current zone is an inn room interior; otherwise <see langword="false"/>.</value>
        public static bool IsInInnRoom => InnRoomZones.Contains(WorldManager.ZoneId);

        /// <summary>
        /// Navigates to and opens the Glamour Dresser (NPC object ID 2009439) found inside inn rooms.
        /// If the Glamour Dresser UI window is already open, returns <see langword="true"/> immediately.
        /// If the dresser object is already nearby (e.g. already in an inn room), interacts with it directly.
        /// Otherwise, navigates to the best available inn — preferring the Grand Company barracks dresser
        /// if the player has an unlocked GC barracks — before interacting with the dresser.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the Glamour Dresser window was successfully opened; otherwise <see langword="false"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// if (!await NavigationHelper.GoToGlamourDresser())
        ///     Log.Error("Could not reach the Glamour Dresser.");
        /// </code>
        /// </example>
        public static async Task<bool> GoToGlamourDresser()
        {
            if (RaptureAtkUnitManager.GetWindowByName("MiragePrismPrismBox") != null)
            {
                return true;
            }

            var dresser = GameObjectManager.GetObjectByNPCId(2009439);
            if (dresser != null)
            {
                await InteractWithNpc(dresser);

                return true;
            }

            if (!StartingInnKeepers.Any(i => i.IsQuestCompleted))
            {
                Log.Information("Need to get further in the MSQ");
                return false;
            }

            var innKeeps = StartingInnKeepers.Concat(InnKeepers.Where(i => i.IsQuestCompleted && i.CanGetTo));
            if (Core.Me.GrandCompany != 0 && GrandCompanyHelper.BarracksNpc is { IsQuestCompleted: true })
            {
                innKeeps = innKeeps.Append(GrandCompanyHelper.BarracksNpc);
            }

            var bestInn = NpcHelper.GetClosestNpc(innKeeps);

            if (bestInn is null)
            {
                Log.Error("No Inn Keepers Found");
                return false;
            }

            if (bestInn.Equals(GrandCompanyHelper.BarracksNpc))
            {
                Log.Information("Going to Barracks");
                if (!await GrandCompanyHelper.GetToGCBarracks())
                {
                    Log.Error("Failed to get to Barracks");
                    return false;
                }
            }
            else
            {
                Log.Information($"Going to Inn {bestInn.Location.ZoneName}");
                if (!await GoToInnRoom(bestInn))
                {
                    Log.Error("Failed to get an Inn");
                    return false;
                }
            }

            dresser = GameObjectManager.GetObjectByNPCId(2009439);

            if (dresser == null)
            {
                Log.Error("Couldn't find the glamour dresser");
                return false;
            }

            await InteractWithNpc(dresser);

            return await Coroutine.Wait(10000, () => RaptureAtkUnitManager.GetWindowByName("MiragePrismPrismBox") != null);
        }

        /// <summary>
        /// Moves to and interacts with the specified in-game <see cref="GameObject"/>.
        /// If the object is already within interact range, targets and interacts immediately.
        /// Otherwise navigates via ground movement, falling back to off-mesh navigation if needed.
        /// </summary>
        /// <param name="obj">The game object to interact with, or <see langword="null"/> to return <see langword="false"/> immediately.</param>
        /// <returns>
        /// <see langword="true"/> if the interaction was attempted (does not guarantee a dialog opened);
        /// <see langword="false"/> if <paramref name="obj"/> is <see langword="null"/>.
        /// </returns>
        public static async Task<bool> InteractWithNpc(GameObject? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.IsWithinInteractRange)
            {
                obj.Target();
                obj.Interact();
                return true;
            }

            if (await Navigation.GroundMove(obj.Location, 1.5f))
            {
                obj.Target();
                obj.Interact();
            }
            else
            {
                await Navigation.OffMeshMoveInteract(obj);
            }

            return true;
        }

        /// <summary>
        /// Navigates to the best available FFXIV inn room, selecting from <see cref="StartingInnKeepers"/>
        /// and unlocked expansion entries in <see cref="InnKeepers"/> via <see cref="NpcHelper.GetClosestNpc"/>.
        /// Requires at least one entry in <see cref="StartingInnKeepers"/> to have its quest completed.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the player successfully enters an inn room; otherwise <see langword="false"/>.
        /// </returns>
        public static async Task<bool> GoToInnRoom()
        {
            if (!StartingInnKeepers.Any(i => i.IsQuestCompleted))
            {
                Log.Information("Need to get further in the MSQ");
                return false;
            }

            var innKeeps = StartingInnKeepers.Concat(InnKeepers.Where(i => i.IsQuestCompleted && i.CanGetTo));

            var bestInn = NpcHelper.GetClosestNpc(innKeeps);

            if (bestInn is null)
            {
                Log.Error("No Inn Keepers Found");
                return false;
            }

            Log.Information($"Going to Inn {bestInn.Location.ZoneName}");
            return await GoToInnRoom(bestInn);
        }

        /// <summary>
        /// Navigates to the inn room associated with a specific inn keeper NPC.
        /// Verifies quest completion requirements before attempting travel, interacts with the inn keeper
        /// to trigger the room entry dialog, and waits for the loading screen to complete.
        /// Returns immediately if the player is already in an inn room.
        /// </summary>
        /// <param name="npc">The inn keeper <see cref="Npc"/> whose inn room to enter.</param>
        /// <returns>
        /// <see langword="true"/> if the player is in an inn room after navigation; otherwise <see langword="false"/>.
        /// </returns>
        public static async Task<bool> GoToInnRoom(Npc npc)
        {
            if (IsInInnRoom)
            {
                Log.Information("Already in Inn Room");
                return true;
            }

            if (StartingInnKeepers.Contains(npc) && !StartingInnKeepers.Any(i => i.IsQuestCompleted))
            {
                Log.Information("Need to get further in the MSQ");
                return false;
            }

            if (InnKeepers.Contains(npc) && !npc.IsQuestCompleted)
            {
                Log.Information($"Need to finish quest {npc.QuestRequiredId}");
                return false;
            }

            if (!await Navigation.GetToInteractNpcSelectString(npc, 0))
            {
                Log.Error($"Failed to get to {npc.Name}");
                return false;
            }

            var time = PulseTimer.StartNew(new TimeSpan(0, 1, 0));
            while (!CommonBehaviors.IsLoading && !time.Completed)
            {
                if (Talk.DialogOpen)
                {
                    while (Talk.DialogOpen)
                    {
                        Talk.Next();
                        await Coroutine.Wait(100, () => !Talk.DialogOpen);
                        await Coroutine.Wait(100, () => Talk.DialogOpen);
                        await Coroutine.Sleep(100);
                    }
                }

                await Coroutine.Sleep(100);
            }

            if (time.Completed)
            {
                Log.Error("Timed out trying to get to Inn Room");
                return false;
            }

            await CommonTasks.HandleLoading();

            return IsInInnRoom;
        }
    }
}