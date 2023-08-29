#nullable enable
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
    public static class HousingTraveler
    {
        public static readonly ResidentialDistrict[] HousingZones;

        private static readonly LLogger Log = new(nameof(HousingTraveler), Colors.Gold);

        public static readonly IReadOnlyList<ushort> HousingZoneIds;

        public static readonly IReadOnlyList<HousingZone> HousingZonesEnums = new List<HousingZone>() { HousingZone.Mist, HousingZone.LavenderBeds, HousingZone.Empyreum, HousingZone.Goblet, HousingZone.Shirogane };

        public static ResidentialDistrict? CurrentResidentialDistrict => !HousingHelper.IsInHousingArea ? null : GetResidentialDistrictByZone(WorldManager.ZoneId);

        static HousingTraveler()
        {
            HousingZones = new ResidentialDistrict[] { Mist.Instance, LavenderBeds.Instance, Shirogan.Instance, TheGoblet.Instance, Empyreum.Instance };
            HousingZoneIds = HousingZones.Select(i => i.ZoneId).ToList();
        }

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

        public static RecordedPlot? GetRecordedPlot(HousingZone zone, int plot)
        {
            return !HousingZonesEnums.Contains(TranslateZone(zone)) ? null : ResourceManager.HousingPlots[TranslateZone(zone)].Value[plot];
        }

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

        public static async Task<bool> GetToResidential(World world, Npc npc)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetToResidential(npc);
        }

        public static async Task<bool> GetToResidential(Location location, int ward)
        {
            return await GetToResidential(location.ZoneId, location.Coordinates, ward);
        }

        public static async Task<bool> GetToResidential(ushort zoneId, double x, double y, double z, int ward)
        {
            var district = GetResidentialDistrictByZone(zoneId);

            if (district == null)
            {
                return false;
            }

            return await GetToResidential(district, new Vector3((float)x, (float)y, (float)z), ward);
        }

        public static async Task<bool> GetToResidential(World world, Location location, int ward)
        {
            return await GetToResidential(world, location.ZoneId, location.Coordinates, ward);
        }

        private static async Task<bool> GetToResidential(World world, ushort zoneId, Vector3 locationCoordinates, int ward)
        {
            return await GetToResidential(world, zoneId, locationCoordinates.X, locationCoordinates.Y, locationCoordinates.Z, ward);
        }

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

        public static async Task<bool> GetToResidential(HousingZone zone, Vector3 location, int ward)
        {
            var district = GetResidentialDistrictByZone((ushort)zone);

            if (district == null)
            {
                return false;
            }

            return await GetToResidential(district, location, ward);
        }

        public static async Task<bool> GetToResidential(ResidentialDistrict district, Vector3 location, int ward, float distance = 2.5f)
        {
            if (!await district.SelectWard(ward))
            {
                Log.Error($"Can't get to ward {ward} of {district.Name}");
            }

            //in zone

            return await district.TravelWithinZone(location, distance);
        }

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

        public static async Task<bool> GetToResidential(World world, HousingZone zone, Vector3 entranceLocation, int ward)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetToResidential(zone, entranceLocation, ward);
        }

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

        public static async Task<int> GetWardWithTeleport(Location targetLocation)
        {
            var availableHouses = HousingHelper.Residences.Where(i => i.Zone != 255).ToDictionary(i => i.HouseLocationIndex, i => i);
            int ward = 1;
            if (availableHouses.Values.Any(i => i != null && (ushort)(ushort)HousingTraveler.TranslateZone((HousingZone)i.Zone) == targetLocation.ZoneId))
            {
                var place1 = availableHouses.FirstOrDefault(i => i.Value != null && (ushort)HousingTraveler.TranslateZone((HousingZone)i.Value.Zone) == targetLocation.ZoneId);
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
                        if (!await TeleportHelper.TeleportToSharedEstate((ushort)HousingTraveler.TranslateZone(((HousingZone)house.Zone)), house.Ward, house.Plot))
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
            else
            {
                /*Log.Information($"No house found in {targetLocation.ZoneId}");
                foreach (var keyValuePair in availableHouses)
                {
                    Log.Information($"{keyValuePair.Key} {keyValuePair.Value}");
                }*/
            }

            return ward;
        }
    }
}