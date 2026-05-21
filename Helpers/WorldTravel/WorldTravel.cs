using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Helpers.WorldTravel
{
    /// <summary>
    /// Static helper that orchestrates cross-world travel via the in-game World Travel aetheryte menu.
    /// </summary>
    /// <remarks>
    /// The player must be located at Limsa Lominsa, Ul'dah, or Gridania to open the world-travel UI.
    /// <see cref="OpenWorldTravelMenu"/> handles the teleport automatically when the player is elsewhere.
    /// </remarks>
    public static class WorldTravel
    {
        private static readonly LLogger Log = new(nameof(WorldTravel), Colors.Chocolate);

        private const TravelCity DefaultStart = TravelCity.Cheapest;

        private readonly static ushort[] ValidZones = { 129, 130, 132 };

        private static bool InValidZone => ValidZones.Contains(WorldManager.ZoneId);

        /// <summary>
        /// Opens the World Travel selection menu, teleporting to a valid city first if necessary.
        /// </summary>
        /// <param name="travelCity">
        /// The preferred city to travel to in order to access the world-travel aetheryte.
        /// Defaults to <see cref="TravelCity.Cheapest"/>, which selects the cheapest available
        /// city aetheryte automatically.
        /// </param>
        /// <remarks>
        /// If the player is already in a valid zone (Limsa Lominsa, Ul'dah, or Gridania) the
        /// teleport step is skipped.  The method opens <c>AgentWorldTravelSelect</c> and waits
        /// up to 5 seconds for the window to become ready.
        /// </remarks>
        /// <example>
        /// <code>
        /// await WorldTravel.OpenWorldTravelMenu(TravelCity.Limsa);
        /// </code>
        /// </example>
        public static async Task OpenWorldTravelMenu(TravelCity travelCity = DefaultStart)
        {
            if (WorldTravelSelect.Instance.IsOpen)
            {
                Log.Information("World Travel Already Open");
                return;
            }

            if (!InValidZone)
            {
                Log.Information($"Travel city: {travelCity}");
                travelCity = (TravelCity)WorldManager.ZoneId switch
                {
                    TravelCity.Limsa    => TravelCity.Limsa,
                    TravelCity.Uldah    => TravelCity.Uldah,
                    TravelCity.Gridania => TravelCity.Gridania,
                    _                   => travelCity
                };

                if (travelCity == TravelCity.Cheapest)
                {
                    var cheapest = WorldManager.AvailableLocations.Where(i => ValidZones.Contains(i.ZoneId)).OrderBy(i => i.GilCost).ToList();

                    if (cheapest.Count != 0)
                    {
                        travelCity = (TravelCity)cheapest.First().ZoneId;
                        Log.Information($"Cheapest Zone Found: {travelCity}");
                    }
                    else
                    {
                        Log.Error("No valid zones found");
                        return;
                    }
                }

                var ae = travelCity switch
                {
                    TravelCity.Limsa    => 8,
                    TravelCity.Uldah    => 9,
                    TravelCity.Gridania => 2,
                    _                   => throw new ArgumentOutOfRangeException(nameof(travelCity), travelCity, null)
                };

                Log.Information($"Traveling to {travelCity}. Calling Teleport {ae}");
                //var result = await CommonTasks.Teleport((uint)ae);
                var result = await TeleportHelper.TeleportByIdTicket((uint)ae);
                Log.Information($"Result from teleport: {result}");
                if (!result)
                {
                    Log.Error("Unable to teleport");
                    return;
                }
            }

            if (!InValidZone)
            {
                Log.Error("Not in a valid zone");
                return;
            }

            AgentWorldTravelSelect.Instance.Toggle();

            await Coroutine.Wait(5000, () => WorldTravelSelect.Instance.IsOpen && AgentWorldTravelSelect.Instance.ChoicesPointer != IntPtr.Zero);
            await Coroutine.Sleep(500);
        }

        /// <summary>
        /// Travels to the specified <see cref="WorldLocation"/>, switching worlds if required and
        /// then navigating to the location's coordinates.
        /// </summary>
        /// <param name="worldLocation">The target world and in-game location.</param>
        /// <param name="travelCity">The preferred city aetheryte used for world travel.</param>
        /// <returns>
        /// <see langword="true"/> if the player successfully reaches the target location;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static async Task<bool> GetTo(WorldLocation worldLocation, TravelCity travelCity = TravelCity.Uldah)
        {
            if (WorldHelper.CurrentWorldId != (int)worldLocation.World && !await GoToWorld(worldLocation.World, travelCity))
            {
                return false;
            }

            if (WorldHelper.CurrentWorldId == (int)worldLocation.World && Navigator.AtLocation(worldLocation.Location.Coordinates))
            {
                return true;
            }

            return await Navigation.GetTo(worldLocation.Location);
        }

        /// <summary>
        /// Travels to the specified world using the aetheryte World Travel menu.
        /// </summary>
        /// <param name="world">The target <see cref="World"/> server.</param>
        /// <param name="travelCity">The preferred city aetheryte used to access the world-travel UI.</param>
        /// <returns>
        /// <see langword="true"/> if the player is on <paramref name="world"/> after the operation;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// await WorldTravel.GoToWorld(ViewModel.Instance.WorldToTravel);
        /// </code>
        /// </example>
        public static async Task<bool> GoToWorld(World world, TravelCity travelCity = DefaultStart)
        {
            return await GoToWorld((ushort)world, travelCity);
        }

        /// <summary>
        /// Travels to the specified world by world ID using the aetheryte World Travel menu.
        /// </summary>
        /// <param name="worldId">The numeric world ID of the target server.</param>
        /// <param name="travelCity">The preferred city aetheryte used to access the world-travel UI.</param>
        /// <returns>
        /// <see langword="true"/> if the player is on the requested world after the operation;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The method handles party management (leaving non-cross-realm parties), opens the world travel
        /// UI, selects the target world, confirms the dialogue, and waits for the loading screen to finish.
        /// </remarks>
        public static async Task<bool> GoToWorld(ushort worldId, TravelCity travelCity = DefaultStart)
        {
            if (!WorldHelper.CheckDC((World)worldId))
            {
                Log.Error("Not on the same DC");
                return false;
            }

            if (WorldHelper.CurrentWorldId == worldId)
            {
                Log.Information("Already on the same world");
                return true;
            }

            if (PartyManager.IsInParty && !PartyManager.CrossRealm)
            {
                Log.Information("Getting out of party");
                ChatManager.SendChat("/pcmd leave");
                if (!await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                {
                    Log.Error("Could not leave party...RIP");
                    return false;
                }

                SelectYesno.Yes();

                if (!await Coroutine.Wait(5000, () => PartyManager.IsInParty && !PartyManager.CrossRealm))
                {
                    Log.Error("Could not leave party...RIP");
                    return false;
                }
            }

            Core.Me.SetRun();

            if (travelCity == TravelCity.Cheapest)
            {
            }

            await OpenWorldTravelMenu(travelCity);

            if (!WorldTravelSelect.Instance.IsOpen)
            {
                Log.Error("World Travel Menu did not open");
                return false;
            }

            var Choices = AgentWorldTravelSelect.Instance.Choices;

            if (AgentWorldTravelSelect.Instance.CurrentWorld == worldId)
            {
                Log.Information($"Already on {((World)worldId).WorldName()}");
                WorldTravelSelect.Instance.Close();
                await Coroutine.Sleep(500);
                return true;
            }

            for (var i = 0; i < Choices.Length; i++)
            {
                if (Choices[i].WorldID != worldId)
                {
                    continue;
                }

                Log.Information($"Going to: {((World)Choices[i].WorldID).WorldName()} test");
                WorldTravelSelect.Instance.SelectWorld(i);
                if (!await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                {
                    Log.Error("Select Yesno did not open");
                    WorldTravelSelect.Instance.Close();
                    return false;
                }

                if (!SelectYesno.IsOpen)
                {
                    Log.Error("Select Yesno did not open");
                    WorldTravelSelect.Instance.Close();
                    return false;
                }

                Log.Information("Selecting Yes");
                SelectYesno.Yes();
                if (!await Coroutine.Wait(5000, () => !SelectYesno.IsOpen))
                {
                    Log.Error("Select Yesno did not close");
                    WorldTravelSelect.Instance.Close();
                    return false;
                }

                if (!await Coroutine.Wait(12_000, () => WorldTravelFinderReady.Instance.IsOpen))
                {
                    Log.Error("WorldTravelFinderReady did not open");
                    WorldTravelSelect.Instance.Close();
                    await Coroutine.Wait(5000, () => !WorldTravelSelect.Instance.IsOpen);
                    return false;
                }

                if (WorldTravelFinderReady.Instance.IsOpen)
                {
                    Log.Information("WorldTravelFinderReady is open");
                    await Coroutine.Wait(-1, () => !WorldTravelFinderReady.Instance.IsOpen);
                    Log.Information("WorldTravelFinderReady is closed");
                    await Coroutine.Sleep(2000);
                    if (CommonBehaviors.IsLoading)
                    {
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                    }

                    await Coroutine.Sleep(2000);
                    //Log.Information("Waiting for ping to update");
                    await PingChecker.UpdatePing();
                    Log.Information($"CurrentWorld: {WorldHelper.CurrentWorld.WorldName()} Ping: {PingChecker.CurrentPing}");
                }
                else
                {
                    Log.Error("WorldTravelFinderReady did not open");
                    return false;
                }

                break;
            }

            if (WorldTravelSelect.Instance.IsOpen)
            {
                WorldTravelSelect.Instance.Close();
                await Coroutine.Sleep(500);
            }

            if (WorldHelper.IsOnHomeWorld)
            {
                HousingHelper.UpdateResidenceArray();
            }

            await Coroutine.Sleep(500);
            return WorldHelper.CurrentWorldId == worldId;
        }

        /// <summary>
        /// Ensures the player is on their home world, travelling there if necessary.
        /// </summary>
        /// <param name="travelCity">The preferred city aetheryte used for world travel.</param>
        /// <returns>
        /// <see langword="true"/> if the player is on the home world after the operation;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static async Task<bool> MakeSureHome(TravelCity travelCity = DefaultStart)
        {
            if (WorldHelper.CurrentWorldId == WorldHelper.HomeWorldId)
            {
                return true;
            }

            return await GoToWorld(WorldHelper.HomeWorldId, travelCity);
        }

        /// <summary>
        /// Navigates the player to the specified city aetheryte game object.
        /// </summary>
        /// <param name="travelCity">The city whose main aetheryte to navigate to.</param>
        /// <returns>
        /// The aetheryte <see cref="GameObject"/> if navigation succeeded; otherwise <see langword="null"/>.
        /// </returns>
        public static async Task<GameObject?> GetAE(TravelCity travelCity = DefaultStart)
        {
            uint id;
            id = travelCity switch
            {
                TravelCity.Limsa    => 8,
                TravelCity.Uldah    => 9,
                TravelCity.Gridania => 2,
                _                   => throw new ArgumentOutOfRangeException(nameof(travelCity), travelCity, null),
            };
            return await Navigation.GetToAE(id);
        }

        /// <summary>
        /// Selects the "Visit Another World Server" option from the aetheryte conversation menu.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the option was found and selected;
        /// <see langword="false"/> if the option was not present in the conversation list.
        /// </returns>
        public static bool SelectWorldVisit()
        {
            var test = Conversation.GetConversationList.TakeWhile(line => !line.Contains(Translator.VisitAnotherWorldServer)).Count();

            if (test == Conversation.GetConversationList.Count)
            {
                return false;
            }

            Conversation.SelectLine((uint)test);
            return true;

        }
    }
}