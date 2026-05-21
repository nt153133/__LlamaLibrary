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
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;
// ReSharper disable MemberCanBeProtected.Global

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    /// <summary>
    /// Generic base class that provides a lazily-initialised singleton pattern for each
    /// concrete residential district.
    /// </summary>
    /// <typeparam name="T">The concrete district type (e.g. <see cref="Mist"/>).</typeparam>
    public class ResidentialDistrict<T> : ResidentialDistrict
        where T : ResidentialDistrict<T>, new()
    {
        private static T? _instance;

        /// <summary>Gets the singleton instance, creating it on first access.</summary>
        public static T Instance => _instance ??= new T();

        protected ResidentialDistrict()
        {
        }
    }

    /// <summary>
    /// Abstract base class for all FFXIV residential-district helpers.
    /// </summary>
    /// <remarks>
    /// Provides shared logic for ward selection, aethernet navigation within a district,
    /// transition to the housing entrance, and opening the ward-selection window.
    /// Concrete subclasses supply district-specific data (zone ID, aetheryte positions, NPC IDs, etc.).
    /// </remarks>
    public abstract class ResidentialDistrict
    {
        private static readonly LLogger Log = new(nameof(ResidentialDistrict), Colors.Pink);
        private Npc? _aetheryteNpc;
        /// <summary>Gets the display name of this residential district (e.g. "Mist").</summary>
        public virtual string Name { get; } = "";

        /// <summary>Gets the zone ID of this residential district's outdoor area.</summary>
        public virtual ushort ZoneId => 0;

        /// <summary>Gets the aetheryte ID for the nearest city aetheryte used to reach this district.</summary>
        public virtual uint TownAetheryteId { get; }

        /// <summary>Gets the quest ID that must be completed to use the aetheryte shortcut to this district.</summary>
        public virtual int RequiredQuest { get; }

        /// <summary>
        /// Gets the NPC reference for the town aetheryte used to enter this district.
        /// </summary>
        public Npc AetheryteNpc
        {
            get => _aetheryteNpc ?? new Npc(TownAetheryteId, ZoneId, TownAetheryteLocation, RequiredQuest);
            set => _aetheryteNpc = value;
        }

        /// <summary>Gets the 3-D world position of the town aetheryte NPC.</summary>
        public virtual Vector3 TownAetheryteLocation { get; } = Vector3.Zero;

        /// <summary>
        /// Gets a value indicating whether this district requires off-mesh navigation paths.
        /// </summary>
        public virtual bool OffMesh { get; } = false;

        /// <summary>Gets the list of housing-district aetheryte shards for this district.</summary>
        public virtual List<HousingAetheryte> Aetherytes => new();

        /// <summary>
        /// Gets the world-space positions from which the player should approach the ward-transition NPC
        /// or trigger zone (used when <see cref="TransitionNpcs"/> is empty).
        /// </summary>
        public virtual List<Vector3> TransitionStartLocations => new();

        /// <summary>
        /// Gets the NPCs that serve as the ward-transition interaction point for this district.
        /// </summary>
        public virtual List<Npc> TransitionNpcs => new();

        /// <summary>
        /// Gets the world-space positions the player must move toward to trigger the ward-selection
        /// conversation (used when <see cref="TransitionNpcs"/> is empty).
        /// </summary>
        public virtual List<Vector3> TransitionEndLocations => new();

        /// <summary>
        /// Gets a value indicating whether the player has attuned to the town aetheryte for this district.
        /// </summary>
        public bool HasAetheryte => ConditionParser.HasAetheryte(TownAetheryteId);

        /// <summary>
        /// Gets a value indicating whether the player has completed the required unlock quest
        /// for aetheryte access to this district.
        /// </summary>
        public bool QuestComplete => ConditionParser.IsQuestCompleted(RequiredQuest);

        /// <summary>
        /// Opens the housing ward-selection window, navigating to the district entrance first if necessary.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the ward-selection window (<c>HousingSelectBlock</c>) is open;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// if (!await district.OpenWardSelection())
        /// {
        ///     Log.Error("Could not open ward selection.");
        /// }
        /// </code>
        /// </example>
        public async Task<bool> OpenWardSelection()
        {
            if (WorldManager.ZoneId == ZoneId)
            {
                if (!await GetToWardTransition())
                {
                    Log.Error("Failed GetToWardTransition");
                    return false;
                }
            }
            else if (QuestComplete)
            {
                if (!await GetToResidentialAe())
                {
                    Log.Error("Failed GetToResidentialAe");
                    return false;
                }
            }
            else
            {
                if (!await WalkToResidential())
                {
                    return false;
                }
            }

            if (!SelectString.IsOpen)
            {
                Log.Error("Select string not open");
                return false;
            }

            return await SelectHousing();
        }

        /// <summary>
        /// Selects the specified ward in the housing ward-selection window, loading into it.
        /// </summary>
        /// <param name="ward">The 1-based ward number (1–30).</param>
        /// <returns>
        /// <see langword="true"/> if the player successfully enters the specified ward;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// if (!await district.SelectWard(ward))
        /// {
        ///     Log.Error($"Can't get to ward {ward} of {district.Name}");
        /// }
        /// </code>
        /// </example>
        public async Task<bool> SelectWard(int ward)
        {
            if (ward is < 1 or > 30)
            {
                Log.Error($"Invalid ward {ward}");
                return false;
            }

            if (HousingHelper.IsInHousingArea && ZoneId == WorldManager.ZoneId && (HousingHelper.CurrentWard + 1) == ward)
            {
                Log.Information($"Already in correct ward {ward}");
                return true;
            }

            if (!HousingSelectBlock.Instance.IsOpen)
            {
                //Log.Information($"Not open");
                if (!await OpenWardSelection())
                {
                    Log.Error($"Can't open ward selection {ward}");
                    return false;
                }
            }

            //Log.Information($"Selecting ward {ward}");
            HousingSelectBlock.Instance.SelectWard(ward - 1);

            await Coroutine.Sleep(500);

            if (HousingSelectBlock.Instance.WindowByName == null)
            {
                return false;
            }

            HousingSelectBlock.Instance.WindowByName.SendAction(2, 3, 0, 0, 0x10);

            if (!await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
            {
                return false;
            }

            SelectYesno.Yes();

            if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
            {
                return false;
            }

            return await Coroutine.Wait(new TimeSpan(0, 2, 30), () => !CommonBehaviors.IsLoading);
        }

        /// <summary>
        /// Closes the ward-selection window, handling the confirmation dialogue and any loading screen.
        /// </summary>
        /// <returns><see langword="true"/> when the window is no longer open.</returns>
        /// <example>
        /// <code>
        /// await district.CloseWardSelection();
        /// </code>
        /// </example>
        public async Task<bool> CloseWardSelection()
        {
            if (QuestComplete)
            {
                return await CloseHousingWardsNoLoad();
            }

            return await CloseHousingWards();
        }

        /// <summary>
        /// Walks to the district entrance without using aetheryte teleportation (for players who have
        /// not yet completed the unlock quest).
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the ward-selection conversation is open;
        /// otherwise <see langword="false"/>.  Base implementation always returns <see langword="false"/>.
        /// </returns>
        public virtual Task<bool> WalkToResidential()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Navigates to the ward-transition NPC or trigger zone and opens the ward-selection conversation.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when a conversation window is open at the transition point;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public virtual async Task<bool> GetToWardTransition()
        {
            if (TransitionNpcs.Count != 0)
            {
                var npc = NpcHelper.GetClosestNpc(TransitionNpcs);

                if (npc == null)
                {
                    Log.Error("Failed to find npc");
                    return false;
                }

                if (!await TravelWithinZone(npc.Location.Coordinates))
                {
                    Log.Error($"Could not get to transition npc {npc}");
                    return false;
                }

                var gameObject = npc.GameObject;

                if (gameObject == null)
                {
                    Log.Error($"Could not find transition npc {npc}");
                    return false;
                }

                gameObject.Target();
                gameObject.Interact();

                if (!await Coroutine.Wait(5000, () => Talk.DialogOpen || Conversation.IsOpen))
                {
                    Log.Error($"Could not interact with npc {npc}");
                    return false;
                }

                await DealWithTalk();

                if (!await Coroutine.Wait(5000, () => Conversation.IsOpen))
                {
                    Log.Error($"Could not interact with npc (post talk) {npc}");
                    return false;
                }
            }
            else
            {
                var meLocation = Core.Me.Location;
                var closestTransition = TransitionStartLocations.OrderBy(i => i.Distance2DSqr(meLocation)).First();

                if (!await TravelWithinZone(closestTransition))
                {
                    Log.Error($"Could not get to transition location {closestTransition}");
                    return false;
                }

                while (!Conversation.IsOpen)
                {
                    Navigator.PlayerMover.MoveTowards(TransitionEndLocations.OrderBy(i => i.Distance2DSqr(closestTransition)).First());
                    await Coroutine.Sleep(50);

                    //Navigator.PlayerMover.MoveStop();
                }

                Navigator.PlayerMover.MoveStop();
                if (!await Coroutine.Wait(5000, () => Conversation.IsOpen))
                {
                    Log.Error("Could not get to select string");
                    return false;
                }
            }

            return Conversation.IsOpen;
        }

        /// <summary>
        /// Teleports to the town aetheryte and opens the aethernet residential-district menu.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the ward-selection string window is open;
        /// otherwise <see langword="false"/> (e.g. the player lacks the required aetheryte attunement).
        /// </returns>
        public async Task<bool> GetToResidentialAe()
        {
            if (!HasAetheryte)
            {
                return false;
            }

            var unit = await Navigation.GetToAE(TownAetheryteId);

            if (unit is null)
            {
                return false;
            }

            unit.Target();
            unit.Interact();

            if (!await Coroutine.Wait(5000, () => SelectString.IsOpen))
            {
                unit = await Navigation.GetToAE(TownAetheryteId);
                if (unit is null)
                {
                    return false;
                }

                unit.Target();
                unit.Interact();
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }

            if (!SelectString.IsOpen)
            {
                return false;
            }

            SelectString.ClickLineContains(Translator.ResidentialDistrictAethernet);

            return await Coroutine.Wait(5000, () => SelectString.IsOpen && SelectString.Lines().Any(i => i.Contains(Translator.SelectWard)));

            //return await Coroutine.Wait(5000, () => SelectString.IsOpen);
        }

        private static async Task<bool> SelectHousing()
        {
            if (!SelectString.IsOpen)
            {
                Log.Error("No Select String open ward");
                return false;
            }

            if (!SelectString.ClickLineContains(Translator.SelectWard))
            {
                Log.Error($"Could not select line {Translator.SelectWard}");
                return false;
            }

            return await Coroutine.Wait(5000, () => HousingSelectBlock.Instance.IsOpen);
        }

        /// <summary>
        /// Closes the housing ward-selection window when the transition requires a zone load.
        /// </summary>
        /// <returns><see langword="true"/> when the window is closed; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> CloseHousingWards()
        {
            if (!HousingSelectBlock.Instance.IsOpen)
            {
                return true;
            }

            HousingSelectBlock.Instance.Close();

            await Coroutine.Wait(5000, () => Conversation.IsOpen);

            if (Conversation.IsOpen)
            {
                Conversation.SelectQuit();
                await Coroutine.Wait(5000, () => !Conversation.IsOpen);
            }

            await Coroutine.Sleep(500);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            return !HousingSelectBlock.Instance.IsOpen;
        }

        private static async Task<bool> CloseHousingWardsNoLoad()
        {
            if (!HousingSelectBlock.Instance.IsOpen)
            {
                return true;
            }

            HousingSelectBlock.Instance.Close();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            if (Conversation.IsOpen)
            {
                Conversation.SelectQuit();
                await Coroutine.Wait(5000, () => !Conversation.IsOpen);
            }

            return !HousingSelectBlock.Instance.IsOpen;
        }

        /// <summary>
        /// Returns the housing-district aetheryte shard closest to the specified world position.
        /// </summary>
        /// <param name="location">The reference position to measure from.</param>
        /// <returns>The nearest <see cref="HousingAetheryte"/>, or <see langword="null"/> when none exist.</returns>
        public HousingAetheryte? ClosestHousingAetheryte(Vector3 location)
        {
            return Aetherytes.OrderBy(i => i.Location.Distance2DSqr(location)).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether an aethernet teleport should be used to reach the specified destination.
        /// </summary>
        /// <param name="location">The destination position within the district.</param>
        /// <returns>
        /// <see langword="true"/> when the closest aetheryte to the destination differs from the
        /// one closest to the player (i.e. an aethernet hop is beneficial); otherwise <see langword="false"/>.
        /// </returns>
        public bool ShouldUseAethernet(Vector3 location)
        {
            if (WorldManager.ZoneId != ZoneId)
            {
                return false;
            }

            if (!HousingHelper.IsInHousingArea || HousingHelper.IsInsideHouse)
            {
                return false;
            }

            /*
            Log.Information($"ClosestHousingAetheryte dist: {ClosestHousingAetheryte(Core.Me.Location).Location.Distance(Core.Me.Location)}");

            Log.Information($"location dist: {location.Distance(Core.Me.Location)}");

            Log.Information($"ClosestHousingAetheryte Location dist: {ClosestHousingAetheryte(location).Location.Distance(Core.Me.Location)}");
            */
            var closestAetheryte = ClosestHousingAetheryte(location);
            var closestAetheryteMe = ClosestHousingAetheryte(Core.Me.Location);

            if (closestAetheryte is null || closestAetheryteMe is null)
            {
                return false;
            }

            if (closestAetheryte.Subdivision != closestAetheryteMe.Subdivision)
            {
                return true;
            }

            if (!closestAetheryte.Equals(closestAetheryteMe))
            {
                return closestAetheryteMe.Location.Distance(Core.Me.Location) < location.Distance(Core.Me.Location);
            }

            return false;
        }

        /// <summary>
        /// Moves the player to the specified destination within the current district zone,
        /// using an aethernet shard shortcut when beneficial.
        /// </summary>
        /// <param name="destination">The 3-D destination coordinates inside this district.</param>
        /// <param name="stopDistance">Stop distance in yalms (default 3).</param>
        /// <returns>
        /// <see langword="true"/> if the player is within 5 yalms of the destination after navigation;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// return await district.TravelWithinZone(location, distance);
        /// </code>
        /// </example>
        public async Task<bool> TravelWithinZone(Vector3 destination, float stopDistance = 3)
        {
            if (WorldManager.ZoneId != ZoneId)
            {
                return false;
            }

            if (ShouldUseAethernet(destination))
            {
                var closestAe = ClosestHousingAetheryte(Core.Me.Location);

                if (closestAe is null)
                {
                    return false;
                }

                //Log.Information($"{closestAe}");

                Log.Information($"Using Aetheryte {closestAe.Name}");
                try
                {
                    if (!await Navigation.GroundMove(closestAe.Location, 1f))
                    {
                        Log.Error("Couldn't get to ae");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                    return false;
                }

                var ae = GameObjectManager.GetObjectByNPCId(closestAe.NpcId);

                if (ae == null)
                {
                    Log.Error($"Couldn't find ae {closestAe}");
                    return false;
                }

                ae.Target();
                ae.Interact();
                if (!await Coroutine.Wait(5000, () => TelepotTown.IsOpen))
                {
                    Log.Error($"Open ae window {closestAe}");
                    return false;
                }

                AgentTelepotTown.Instance.TeleportByAetheryteId(ClosestHousingAetheryte(destination)!.Key);

                await Coroutine.Wait(-1, () => CommonBehaviors.IsLoading);
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                await Coroutine.Sleep(1000);
            }

            Log.Information($"Moving to destination within distance {stopDistance}");
            await Navigation.GroundMove(destination, stopDistance);

            return Core.Me.Location.Distance(destination) < 5;
        }

        /*
        public async Task<List<PlotForSale>> HousingWards(int wardStart = 1, int wardEnd = 24)
        {
            var output = new List<PlotForSale>();
            if (HousingSelectBlock.Instance.IsOpen)
            {
                for (var i = wardStart-1; i < wardEnd; i++)
                {
                    HousingSelectBlock.Instance.SelectWard(i);

                    await Coroutine.Sleep(500);

                    //Log.Information($"Ward {AgentHousingSelectBlock.Instance.WardNumber + 1}");
                    var plotStatus = AgentHousingSelectBlock.Instance.ReadPlots(HousingSelectBlock.Instance.NumberOfPlots);

                    for (var j = 0; j < plotStatus.Length; j++)
                    {
                        if (plotStatus[j] == 0)
                        {
                            var price = int.Parse(GetNumbers(HousingSelectBlock.Instance.PlotPrice(j)));
                            var size = PlotSize.Small;

                            var bytes = Encoding.ASCII.GetBytes(HousingSelectBlock.Instance.PlotString(j).Split(' ')[1]);
                            if (bytes.Length > 9)
                            {
                                switch (bytes[9])
                                {
                                    case 72:
                                        size = PlotSize.Small;
                                        break;
                                    case 1:
                                        size = PlotSize.Medium;
                                        break;
                                    case 2:
                                        size = PlotSize.Large;
                                        break;
                                }
                            }

                            var plot = new HousingPlot(this.ToHousingDistrict(), i+1, j + 1, size);
                            output.Add(new PlotForSale(plot, price, false));
                            //Log.Information($"{HousingSelectBlock.Instance.HousingWard} Plot {j+1} {size} -  {price}");
                            //output.Add($"{HousingSelectBlock.Instance.HousingWard} Plot {j + 1} {size} -  {price}");
                        }
                    }

                    await Coroutine.Sleep(200);
                }
            }

            return output;
        }
        */

        private static string GetNumbers(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Advances through any open dialogue boxes (Talk bubbles) until they are all dismissed.
        /// </summary>
        public static async Task DealWithTalk()
        {
            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(200, () => !Talk.DialogOpen);
                    await Coroutine.Wait(500, () => Talk.DialogOpen);
                    await Coroutine.Sleep(200);
                    await Coroutine.Yield();
                }
            }
        }
    }
}