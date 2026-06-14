using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.HousingTravel.Districts;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.HousingTravel
{
    /// <summary>
    /// Central helper for navigating to FFXIV residential districts, wards, and individual housing plots.
    /// </summary>
    /// <remarks>
    /// This static class coordinates world travel, aetheryte teleportation, and in-zone navigation to
    /// bring the player to any plot in Mist, Lavender Beds, The Goblet, Shirogane, or Empyreum.
    /// It also exposes helpers for entering house interiors and determining the optimal ward via
    /// known house teleports.
    /// </remarks>
    public static class HousingTraveler
    {
        /// <summary>The singleton instances for each of the five residential districts.</summary>
        public static readonly ResidentialDistrict[] HousingZones;

        private static readonly LLogger Log = new(nameof(HousingTraveler), Colors.Gold);

        /// <summary>The zone IDs for all five residential districts.</summary>
        public static readonly IReadOnlyList<ushort> HousingZoneIds;

        /// <summary>The five primary <see cref="HousingZone"/> enum values (excludes sub-types such as Apartment and Chamber).</summary>
        public static readonly IReadOnlyList<HousingZone> HousingZonesEnums = new List<HousingZone> { HousingZone.Mist, HousingZone.LavenderBeds, HousingZone.Empyreum, HousingZone.Goblet, HousingZone.Shirogane };

        /// <summary>
        /// Gets the <see cref="ResidentialDistrict"/> the player is currently inside, or
        /// <see langword="null"/> when the player is not in any housing area.
        /// </summary>
        public static ResidentialDistrict? CurrentResidentialDistrict => !HousingHelper.IsInHousingArea ? null : GetResidentialDistrictByZone(WorldManager.ZoneId);

        static HousingTraveler()
        {
            HousingZones = new ResidentialDistrict[] { Mist.Instance, LavenderBeds.Instance, Shirogan.Instance, TheGoblet.Instance, Empyreum.Instance };
            HousingZoneIds = HousingZones.Select(i => i.ZoneId).ToList();
        }

        /// <summary>
        /// Normalises a sub-type <see cref="HousingZone"/> (e.g. Apartment, Chamber, Cottage, House,
        /// Mansion variant) to its parent residential-district zone.
        /// </summary>
        /// <param name="zone">The <see cref="HousingZone"/> to translate.</param>
        /// <returns>
        /// The primary <see cref="HousingZone"/> for the district, or the original value when no
        /// mapping is needed.
        /// </returns>
        /// <example>
        /// <code>
        /// var zoneName = HousingTraveler.TranslateZone(houseLocation.HousingZone).AddSpacesToEnum();
        /// </code>
        /// </example>
        public static HousingZone TranslateZone(HousingZone zone)
        {
            zone = zone switch
            {
                HousingZone.ChambersMist          => HousingZone.Mist,
                HousingZone.ChambersLavenderBeds  => HousingZone.LavenderBeds,
                HousingZone.ChambersGoblet        => HousingZone.Goblet,
                HousingZone.ChambersShirogane     => HousingZone.Shirogane,
                HousingZone.ChambersEmpyreum      => HousingZone.Empyreum,
                HousingZone.ApartmentMist         => HousingZone.Mist,
                HousingZone.ApartmentLavenderBeds => HousingZone.LavenderBeds,
                HousingZone.ApartmentGoblet       => HousingZone.Goblet,
                HousingZone.ApartmentShirogane    => HousingZone.Shirogane,
                HousingZone.ApartmentEmpyreum     => HousingZone.Empyreum,
                HousingZone.CottageMist           => HousingZone.Mist,
                HousingZone.CottageLavenderBeds   => HousingZone.LavenderBeds,
                HousingZone.CottageGoblet         => HousingZone.Goblet,
                HousingZone.CottageShirogane      => HousingZone.Shirogane,
                HousingZone.CottageEmpyreum       => HousingZone.Empyreum,
                HousingZone.HouseMist             => HousingZone.Mist,
                HousingZone.HouseLavenderBeds     => HousingZone.LavenderBeds,
                HousingZone.HouseGoblet           => HousingZone.Goblet,
                HousingZone.HouseShirogane        => HousingZone.Shirogane,
                HousingZone.HouseEmpyreum         => HousingZone.Empyreum,
                HousingZone.MansionMist           => HousingZone.Mist,
                HousingZone.MansionLavenderBeds   => HousingZone.LavenderBeds,
                HousingZone.MansionGoblet         => HousingZone.Goblet,
                HousingZone.MansionShirogane      => HousingZone.Shirogane,
                HousingZone.MansionEmpyreum       => HousingZone.Empyreum,
                _                                 => zone
            };

            return zone;
        }

        /// <summary>
        /// Navigates to a housing plot and enters the house interior.
        /// </summary>
        /// <param name="location">The <see cref="HouseLocation"/> identifying the district, ward, and plot.</param>
        /// <returns>
        /// <see langword="true"/> if the player is inside the house after the operation;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static async Task<bool> EnterHouse(HouseLocation location)
        {
            if (!await GetToResidential(location))
            {
                return false;
            }

            var recordedPlot = GetRecordedPlot(location.HousingZone, location.Plot);

            if (recordedPlot == null)
            {
                Log.Error($"Recorded plot null for {location.HousingZone} {location.Plot}");
                return false;
            }

            return await EnterHouse(recordedPlot);
        }

        /// <summary>
        /// Enters a house interior via a <see cref="RecordedPlot"/> reference.
        /// </summary>
        /// <param name="recordedPlot">The recorded plot whose entrance to use.</param>
        /// <returns>
        /// <see langword="true"/> if the player is already inside the correct house or successfully
        /// enters it; otherwise <see langword="false"/>.
        /// </returns>
        public static async Task<bool> EnterHouse(RecordedPlot recordedPlot)
        {
            if (HousingHelper.IsInsideHouse && HousingHelper.CurrentHouseLocation?.Plot == recordedPlot.Plot)
            {
                return true;
            }

            return await recordedPlot.Enter();
        }

        /// <summary>
        /// Looks up the pre-recorded plot data for a specific housing zone and plot number.
        /// </summary>
        /// <param name="zone">The housing zone (will be normalised via <see cref="TranslateZone"/>).</param>
        /// <param name="plot">The 1-based plot number within the ward.</param>
        /// <returns>
        /// The <see cref="RecordedPlot"/> if available, or <see langword="null"/> when the zone
        /// is not in <see cref="HousingZonesEnums"/> or the plot is not recorded.
        /// </returns>
        /// <example>
        /// <code>
        /// var recorded = HousingTraveler.GetRecordedPlot(previousHouseLocation.HousingZone, previousHouseLocation.Plot);
        /// </code>
        /// </example>
        public static RecordedPlot? GetRecordedPlot(HousingZone zone, int plot)
        {
            var translated = TranslateZone(zone);
            if (!HousingZonesEnums.Contains(translated))
            {
                return null;
            }

            return ResourceManager.HousingPlots[translated].Value.TryGetValue(plot, out var recordedPlot) ? recordedPlot : null;
        }

        /// <summary>
        /// Navigates the player to a specific housing plot, using the fastest available route
        /// (private estate, free-company estate, shared estate, or direct travel).
        /// </summary>
        /// <param name="location">The target <see cref="HouseLocation"/>.</param>
        /// <returns>
        /// <see langword="true"/> when the player is in the correct ward near the plot entrance;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// if (!await HousingTraveler.GetToResidential(estate))
        /// {
        ///     Log.Error("Failed to travel to the estate.");
        /// }
        /// </code>
        /// </example>
        public static async Task<bool> GetToResidential(HouseLocation location)
        {
            location.HousingZone = TranslateZone(location.HousingZone);
            var recorded = GetRecordedPlot(location.HousingZone, location.Plot);

            if (recorded == null)
            {
                Log.Error($"Recorded plot null for {location.HousingZone} {location.Plot}");
                return false;
            }

            Log.Information($"Getting to {location}");

            if (location.World == WorldHelper.HomeWorld)
            {
                if (!await WorldTravel.WorldTravel.GoToWorld(location.World))
                {
                    Log.Information("wtf");
                    return false;
                }

                TeleportHelper.UpdateTeleportArray();
                await Coroutine.Sleep(200);
                HousingHelper.UpdateResidenceArray();

                var residence = GetResidentialDistrictByZone((ushort)location.HousingZone);

                if (residence == null)
                {
                    Log.Error($"Residential district null for {location.HousingZone}");
                    return false;
                }

                var closestAetherytePlacard = residence.ClosestHousingAetheryte(recorded.PlacardLocation);
                var closestAetheryteMe = residence.ClosestHousingAetheryte(Core.Me.Location);
                var availableHouses = HousingHelper.Residences.ToDictionary(i => i.HouseLocationIndex, i => (HouseLocation?)i);
                Log.Information("Checking if we can use a house teleport directly");

                if (((ushort)TranslateZone(recorded.HousingZone) == WorldManager.ZoneId) && (closestAetheryteMe?.Key == closestAetherytePlacard?.Key))
                {
                    Log.Information("We are already in the zone and the closest aetheryte is the same as the placard");
                    return await GetToResidential(location.World, location.HousingZone, recorded.EntranceLocation, location.Ward);
                }

                if (availableHouses.Values.Contains(location))
                {
                    var place1 = availableHouses.FirstOrDefault(i => i.Value != null && i.Value.Equals(location));
                    if (place1.Value == null)
                    {
                        return await GetToResidential(location.World, location.HousingZone, recorded.EntranceLocation, location.Ward);
                    }

                    var place = place1.Key;

                    Log.Information($"We can use teleport to {place.AddSpacesToEnum()}");
                    switch (place)
                    {
                        case HouseLocationIndex.PrivateEstate:
                            if (!await TeleportHelper.TeleportToPrivateEstate())
                            {
                                return false;
                            }

                            break;
                        case HouseLocationIndex.Apartment:
                            break;
                        case HouseLocationIndex.FreeCompanyRoom:
                        case HouseLocationIndex.FreeCompanyEstate:
                            if (!await TeleportHelper.TeleportToFreeCompanyEstate())
                            {
                                Log.Error("Failed to teleport to free company estate, maybe we don't have one? Check if you have a free company estate and try again");
                                return false;
                            }

                            break;
                        case HouseLocationIndex.SharedEstate1:
                        case HouseLocationIndex.SharedEstate2:
                            if (!await TeleportHelper.TeleportToSharedEstate((ushort)location.HousingZone, location.Ward, location.Plot))
                            {
                                return false;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    Log.Information("We have no residential options to make it quicker");
                    //list available houses
                    var houses = availableHouses.Where(i => i.Value != null).Select(i => i.Value).ToList();
                    if (houses.Count > 0)
                    {
                        Log.Information("Available houses:");
                        foreach (var house in houses)
                        {
                            Log.Information($"- {house}");
                        }
                    }
                    else
                    {
                        Log.Information("No houses available");
                    }
                }
            }

            return await GetToResidential(location.World, location.HousingZone, recorded.EntranceLocation, location.Ward);
        }

        /// <summary>
        /// Navigates to the housing zone associated with an <see cref="Npc"/>, using the NPC's
        /// current ward when already inside the correct zone.
        /// </summary>
        /// <param name="npc">An NPC whose location is inside a housing zone.</param>
        /// <returns>
        /// <see langword="true"/> on success; <see langword="false"/> if the NPC is not a housing-zone NPC.
        /// </returns>
        /// <example>
        /// <code>
        /// if (await HousingTraveler.GetToResidential(npc))
        /// {
        ///     npc.Interact();
        /// }
        /// </code>
        /// </example>
        public static async Task<bool> GetToResidential(Npc npc)
        {
            if (!npc.IsHousingZoneNpc)
            {
                return false;
            }

            var ward = 1;
            if (HousingHelper.IsInHousingArea && WorldManager.ZoneId == npc.Location.ZoneId)
            {
                ward = HousingHelper.HousingPositionInfo.Ward;
            }

            return await GetToResidential(npc.Location, ward);
        }

        /// <summary>
        /// Travels to the specified world and then navigates to a housing NPC's location.
        /// </summary>
        /// <param name="world">The target world server.</param>
        /// <param name="npc">An NPC located inside a housing zone on that world.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(World world, Npc npc)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetToResidential(npc);
        }

        /// <summary>
        /// Navigates to a <see cref="Location"/> inside a housing zone, selecting the given ward.
        /// </summary>
        /// <param name="location">The target location (zone ID + coordinates).</param>
        /// <param name="ward">The 1-based ward number to navigate to.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(Location location, int ward)
        {
            return await GetToResidential(location.ZoneId, location.Coordinates, ward);
        }

        /// <summary>
        /// Navigates to a housing zone by zone ID and explicit coordinates, selecting the specified ward.
        /// </summary>
        /// <param name="zoneId">The housing zone ID.</param>
        /// <param name="x">Target X coordinate.</param>
        /// <param name="y">Target Y coordinate.</param>
        /// <param name="z">Target Z coordinate.</param>
        /// <param name="ward">The 1-based ward number.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(ushort zoneId, double x, double y, double z, int ward)
        {
            var district = GetResidentialDistrictByZone(zoneId);

            if (district == null)
            {
                return false;
            }

            return await GetToResidential(district, new Vector3((float)x, (float)y, (float)z), ward);
        }

        /// <summary>
        /// Travels to the given world and then navigates to a location inside a housing zone.
        /// </summary>
        /// <param name="world">The target world server.</param>
        /// <param name="location">The in-game location inside the housing zone.</param>
        /// <param name="ward">The 1-based ward number to navigate to.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(World world, Location location, int ward)
        {
            return await GetToResidential(world, location.ZoneId, location.Coordinates, ward);
        }

        private static async Task<bool> GetToResidential(World world, ushort zoneId, Vector3 locationCoordinates, int ward)
        {
            return await GetToResidential(world, zoneId, locationCoordinates.X, locationCoordinates.Y, locationCoordinates.Z, ward);
        }

        /// <summary>
        /// Travels to the given world and navigates to an explicit location within a housing zone.
        /// </summary>
        /// <param name="world">The target world server.</param>
        /// <param name="zoneId">The housing zone ID.</param>
        /// <param name="x">Target X coordinate.</param>
        /// <param name="y">Target Y coordinate.</param>
        /// <param name="z">Target Z coordinate.</param>
        /// <param name="ward">The 1-based ward number.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(World world, ushort zoneId, double x, double y, double z, int ward)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            var district = GetResidentialDistrictByZone(zoneId);

            if (district == null)
            {
                return false;
            }

            return await GetToResidential(district, new Vector3((float)x, (float)y, (float)z), ward);
        }

        /// <summary>
        /// Navigates to a specific <see cref="Vector3"/> location within a housing zone enum,
        /// selecting the given ward.
        /// </summary>
        /// <param name="zone">The primary <see cref="HousingZone"/> enum value.</param>
        /// <param name="location">The 3-D destination coordinates within the zone.</param>
        /// <param name="ward">The 1-based ward number.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(HousingZone zone, Vector3 location, int ward)
        {
            var district = GetResidentialDistrictByZone((ushort)zone);

            if (district == null)
            {
                return false;
            }

            return await GetToResidential(district, location, ward, 1f);
        }

        /// <summary>
        /// Selects the specified ward within a district and then navigates to the destination.
        /// </summary>
        /// <param name="district">The <see cref="ResidentialDistrict"/> singleton to use.</param>
        /// <param name="location">The 3-D destination coordinates.</param>
        /// <param name="ward">The 1-based ward number (1–30).</param>
        /// <param name="distance">Stop distance from the destination in yalms (default 2.5).</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(ResidentialDistrict district, Vector3 location, int ward, float distance = 2.5f)
        {
            if (!await district.SelectWard(ward))
            {
                Log.Error($"Can't get to ward {ward} of {district.Name}");
            }

            //in zone

            return await district.TravelWithinZone(location, distance);
        }

        /// <summary>
        /// Navigates to a destination inside a housing zone by raw zone ID.
        /// </summary>
        /// <param name="zone">The zone ID of the residential district.</param>
        /// <param name="location">The 3-D destination coordinates.</param>
        /// <param name="ward">
        /// The 1-based ward number; when 1, the ward is determined automatically via
        /// <see cref="GetWardWithTeleport"/>.
        /// </param>
        /// <param name="distance">Stop distance in yalms (default 2.5).</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(ushort zone, Vector3 location, int ward = 1, float distance = 2.5f)
        {
            if (!HousingZones.Select(i => i.ZoneId).Contains(zone))
            {
                return false;
            }

            if (ward == 1)
            {
                ward = await GetWardWithTeleport(new Location(zone, location));
            }

            return await GetToResidential(HousingZones.First(i => i.ZoneId == zone), location, ward, distance);
        }

        /// <summary>
        /// Travels to the given world and then navigates to a housing zone and ward.
        /// </summary>
        /// <param name="world">The target world server.</param>
        /// <param name="zone">The target <see cref="HousingZone"/>.</param>
        /// <param name="entranceLocation">The 3-D location to navigate to within the zone.</param>
        /// <param name="ward">The 1-based ward number.</param>
        /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GetToResidential(World world, HousingZone zone, Vector3 entranceLocation, int ward)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetToResidential(zone, entranceLocation, ward);
        }

        /// <summary>
        /// Returns the <see cref="ResidentialDistrict"/> singleton that corresponds to a given zone ID.
        /// </summary>
        /// <param name="zoneId">The zone ID to look up (should be one of the five primary district zone IDs).</param>
        /// <returns>
        /// The matching <see cref="ResidentialDistrict"/> singleton, or <see langword="null"/> when
        /// the zone ID does not correspond to any known residential district.
        /// </returns>
        public static ResidentialDistrict? GetResidentialDistrictByZone(ushort zoneId)
        {
            return (HousingZone)zoneId switch
            {
                HousingZone.Mist         => Mist.Instance,
                HousingZone.LavenderBeds => LavenderBeds.Instance,
                HousingZone.Goblet       => TheGoblet.Instance,
                HousingZone.Shirogane    => Shirogan.Instance,
                HousingZone.Empyreum     => Empyreum.Instance,
                _                        => null,
            };
        }

        /// <summary>
        /// Determines the best ward to target for a given housing location by attempting to teleport
        /// via a known registered house (private estate, apartment, FC estate, or shared estate).
        /// </summary>
        /// <param name="targetLocation">The <see cref="Location"/> of the target housing zone.</param>
        /// <returns>
        /// The 1-based ward number the player landed in after teleporting, or <c>1</c> when no
        /// house teleport is available or the player is not on the home world.
        /// </returns>
        /// <remarks>
        /// This method only attempts house teleports when the player is on their home world.
        /// If already inside the correct housing area, the current ward is returned directly.
        /// </remarks>
        public static async Task<int> GetWardWithTeleport(Location targetLocation)
        {
            if (!WorldHelper.IsOnHomeWorld)
            {
                return 1;
            }

            if (WorldManager.ZoneId == targetLocation.ZoneId && HousingHelper.IsInHousingArea)
            {
                return HousingHelper.HousingPositionInfo.Ward;
            }

            var availableHouses = HousingHelper.Residences.Where(i => i.Zone != 255).ToDictionary(i => i.HouseLocationIndex, i => i);
            int ward = 1;
            if (availableHouses.Values.Any(i => i != null && (ushort)TranslateZone((HousingZone)i.Zone) == targetLocation.ZoneId))
            {
                var place1 = availableHouses.FirstOrDefault(i => i.Value != null && (ushort)TranslateZone((HousingZone)i.Value.Zone) == targetLocation.ZoneId);
                var place = place1.Key;
                var house = place1.Value;
                Log.Information($"Found a house in {place} {house}");
                switch (place)
                {
                    case HouseLocationIndex.PrivateEstate:
                        if (!await TeleportHelper.TeleportToPrivateEstate())
                        {
                            ward = 1;
                        }

                        break;
                    case HouseLocationIndex.ApartmentRoom:
                    case HouseLocationIndex.Apartment:
                        if (!await TeleportHelper.TeleportToApartment())
                        {
                            ward = 1;
                        }

                        break;
                    case HouseLocationIndex.FreeCompanyRoom:
                    case HouseLocationIndex.FreeCompanyEstate:
                        if (!await TeleportHelper.TeleportToFreeCompanyEstate())
                        {
                            ward = 1;
                        }

                        break;
                    case HouseLocationIndex.SharedEstate1:
                    case HouseLocationIndex.SharedEstate2:
                        if (!await TeleportHelper.TeleportToSharedEstate((ushort)TranslateZone(((HousingZone)house.Zone)), house.Ward, house.Plot))
                        {
                            ward = 1;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (HousingHelper.IsInHousingArea)
                {
                    ward = HousingHelper.HousingPositionInfo.Ward;
                }
            }

            /*Log.Information($"No house found in {targetLocation.ZoneId}");
                foreach (var keyValuePair in availableHouses)
                {
                    Log.Information($"{keyValuePair.Key} {keyValuePair.Value}");
                }*/
            return ward;
        }
    }
}