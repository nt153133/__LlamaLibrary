#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Enums;
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

        private static readonly string Name = "HousingTraveler";
        private static readonly LLogger Log = new LLogger(Name, Colors.Gold);

        public static readonly IReadOnlyList<ushort> HousingZoneIds;

        public static readonly IReadOnlyList<HousingZone> HousingZonesEnums = new List<HousingZone>() { HousingZone.Mist, HousingZone.LavenderBeds, HousingZone.Empyreum, HousingZone.Goblet, HousingZone.Shirogane };

        public static ResidentialDistrict? CurrentResidentialDistrict => !HousingHelper.IsInHousingArea ? null : GetResidentialDistrictByZone(WorldManager.ZoneId);

        static HousingTraveler()
        {
            HousingZones = new ResidentialDistrict[] { Mist.Instance, LavenderBeds.Instance, Shirogan.Instance, TheGoblet.Instance, Empyreum.Instance };
            HousingZoneIds = HousingZones.Select(i => i.ZoneId).ToList();
        }

        public static RecordedPlot? GetRecordedPlot(HousingZone zone, int plot)
        {
            return !HousingZonesEnums.Contains(zone) ? null : ResourceManager.HousingPlots[zone].Value[plot];
        }

        public static async Task<bool> GetToResidential(HouseLocation location)
        {
            return await GetToResidential(location.World, location.HousingZone, GetRecordedPlot(location.HousingZone, location.Plot)!.EntranceLocation, location.Ward);
        }

        public static async Task<bool> GetToResidential(Npc npc)
        {
            if (!npc.IsHousingZoneNpc)
            {
                return false;
            }

            int ward = 1;
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
            switch ((HousingZone)zoneId)
            {
                case HousingZone.Mist:
                    return Mist.Instance;
                case HousingZone.LavenderBeds:
                    return LavenderBeds.Instance;
                case HousingZone.Goblet:
                    return TheGoblet.Instance;
                case HousingZone.Shirogane:
                    return Shirogan.Instance;
                case HousingZone.Empyreum:
                    return Empyreum.Instance;
                default:
                    return null;
            }
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