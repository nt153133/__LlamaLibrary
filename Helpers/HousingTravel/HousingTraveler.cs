#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
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
            switch (zone)
            {
                case HousingZone.ChambersMist:
                    zone = HousingZone.Mist;
                    break;
                case HousingZone.ChambersLavenderBeds:
                    zone = HousingZone.LavenderBeds;
                    break;
                case HousingZone.ChambersGoblet:
                    zone = HousingZone.Goblet;
                    break;
                case HousingZone.ChambersShirogane:
                    zone = HousingZone.Shirogane;
                    break;
                case HousingZone.ChambersEmpyreum:
                    zone = HousingZone.Empyreum;
                    break;
                case HousingZone.ApartmentMist:
                    zone = HousingZone.Mist;
                    break;
                case HousingZone.ApartmentLavenderBeds:
                    zone = HousingZone.LavenderBeds;
                    break;
                case HousingZone.ApartmentGoblet:
                    zone = HousingZone.Goblet;
                    break;
                case HousingZone.ApartmentShirogane:
                    zone = HousingZone.Shirogane;
                    break;
                case HousingZone.ApartmentEmpyreum:
                    zone = HousingZone.Empyreum;
                    break;
                case HousingZone.CottageMist:
                    zone = HousingZone.Mist;
                    break;
                case HousingZone.CottageLavenderBeds:
                    zone = HousingZone.LavenderBeds;
                    break;
                case HousingZone.CottageGoblet:
                    zone = HousingZone.Goblet;
                    break;
                case HousingZone.CottageShirogane:
                    zone = HousingZone.Shirogane;
                    break;
                case HousingZone.CottageEmpyreum:
                    zone = HousingZone.Empyreum;
                    break;
                case HousingZone.HouseMist:
                    zone = HousingZone.Mist;
                    break;
                case HousingZone.HouseLavenderBeds:
                    zone = HousingZone.LavenderBeds;
                    break;
                case HousingZone.HouseGoblet:
                    zone = HousingZone.Goblet;
                    break;
                case HousingZone.HouseShirogane:
                    zone = HousingZone.Shirogane;
                    break;
                case HousingZone.HouseEmpyreum:
                    zone = HousingZone.Empyreum;
                    break;
                case HousingZone.MansionMist:
                    zone = HousingZone.Mist;
                    break;
                case HousingZone.MansionLavenderBeds:
                    zone = HousingZone.LavenderBeds;
                    break;
                case HousingZone.MansionGoblet:
                    zone = HousingZone.Goblet;
                    break;
                case HousingZone.MansionShirogane:
                    zone = HousingZone.Shirogane;
                    break;
                case HousingZone.MansionEmpyreum:
                    zone = HousingZone.Empyreum;
                    break;
            }

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

            if (location.World == WorldHelper.HomeWorld)
            {
                if (!await WorldTravel.WorldTravel.GoToWorld(location.World))
                {
                    Log.Information("wtf");
                    return false;
                }

                TeleportHelper.UpdateTeleportArray();
                HousingHelper.UpdateResidenceArray();

                var residence = GetResidentialDistrictByZone((ushort)location.HousingZone);

                if (HousingHelper.AccessibleHouseLocations.Contains(location) && residence != null && (residence.ClosestHousingAetheryte(Core.Me.Location).Key != residence.ClosestHousingAetheryte(recorded.PlacardLocation).Key))
                {
                    var place = HouseLocationIndex.FreeCompanyRoom;
                    for (var index = 0; index < HousingHelper.AccessibleHouseLocations.Length; index++)
                    {
                        var houseLocation = HousingHelper.AccessibleHouseLocations[index];
                        if (houseLocation == null)
                        {
                            continue;
                        }

                        if (houseLocation.Equals(location))
                        {
                            place = (HouseLocationIndex)index;
                            break;
                        }
                    }

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

        public static async Task<bool> GetToResidential(ResidentialDistrict district, Vector3 location, int ward)
        {
            if (!await district.SelectWard(ward))
            {
                Log.Error($"Can't get to ward {ward} of {district.Name}");
            }

            //in zone

            return await district.TravelWithinZone(location, 0.5f);
        }

        public static async Task<bool> GetToResidential(ushort zone, Vector3 location, int ward)
        {
            if (!HousingZones.Select(i => i.ZoneId).Contains(zone))
            {
                return false;
            }

            return await GetToResidential(HousingZones.First(i => i.ZoneId == zone), location, ward);
        }

        public static ResidentialDistrict? GetResidentialDistrictByZone(ushort zoneId)
        {
            return (HousingZone)zoneId switch
            {
                HousingZone.Mist => Mist.Instance,
                HousingZone.LavenderBeds => LavenderBeds.Instance,
                HousingZone.Goblet => TheGoblet.Instance,
                HousingZone.Shirogane => Shirogan.Instance,
                HousingZone.Empyreum => Empyreum.Instance,
                _ => null,
            };
        }

        public static async Task<bool> GetToResidential(World world, HousingZone zone, Vector3 entranceLocation, int ward)
        {
            if (!await WorldTravel.WorldTravel.GoToWorld(world))
            {
                return false;
            }

            return await GetToResidential(zone, entranceLocation, ward);
        }
    }
}