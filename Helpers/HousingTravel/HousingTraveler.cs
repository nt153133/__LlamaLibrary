#nullable enable
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using ff14bot.Managers;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.HousingTravel.Districts;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.HousingTravel
{
    public static class HousingTraveler
    {
        public static readonly ResidentialDistrict[] HousingZones;

        private static readonly string Name = "HousingTraveler";
        private static readonly LLogger Log = new LLogger(Name, Colors.Gold);

        public static ResidentialDistrict? CurrentResidentialDistrict => !HousingHelper.IsInHousingArea ? null : GetResidentialDistrictByZone(WorldManager.ZoneId);

        static HousingTraveler()
        {
            HousingZones = new ResidentialDistrict[] { Mist.Instance, LavenderBeds.Instance, Shirogan.Instance, TheGoblet.Instance, Empyreum.Instance };
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

            return await district.TravelWithinZone(location);
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
    }
}