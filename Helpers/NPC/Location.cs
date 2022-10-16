using System;
using System.Linq;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers.HousingTravel;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers.NPC
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Location : IEquatable<Location>
    {
        [JsonProperty]
        public ushort ZoneId { get; set; }
        [JsonProperty]
        public Vector3 Coordinates { get; set; }

        public AetheryteResult ClosestAetherytePrimaryResult => DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == Navigation.GetPrimaryAetheryte(ZoneId, Coordinates));

        public AetheryteResult ClosestAetheryteResult => DataManager.AetheryteCache.Values.Where(i => i.ZoneId == ZoneId).OrderBy(i => i.Position.Distance2DSqr(Coordinates)).FirstOrDefault();

        public bool CanTeleportTo => WorldManager.HasAetheryteId(ClosestAetherytePrimaryResult.Id);

        public bool IsHousingLocation => HousingTraveler.HousingZoneIds.Contains(ZoneId);

        public int TeleportCost
        {
            get
            {
                if (!CanTeleportTo)
                {
                    return -1;
                }

                return (int)WorldManager.AvailableLocations.First(i => i.AetheryteId == ClosestAetherytePrimaryResult.Id).GilCost;
            }
        }

        public string ZoneName
        {
            get
            {
                var aeName = "";
                var ae = DataManager.AetheryteCache.Values.Where(i => i.ZoneId == ZoneId).OrderBy(i => i.Position.Distance2DSqr(Coordinates)).FirstOrDefault();

                if (ae != default(AetheryteResult))
                {
                    aeName = ae.CurrentLocaleName ?? ae.CurrentLocaleAethernetName;
                }

                var zoneName = "";

                if (DataManager.ZoneNameResults.Any(i => i.Key == ZoneId))
                {
                    zoneName = DataManager.ZoneNameResults.FirstOrDefault(i => i.Key == ZoneId).Value.CurrentLocaleName;
                }

                if (zoneName == "")
                {
                    return aeName;
                }

                if (zoneName == aeName || aeName == null || aeName == "")
                {
                    return zoneName;
                }

                return $"{zoneName} ({aeName})";
            }
        }

        public Location(ushort zoneId, Vector3 coordinates)
        {
            ZoneId = zoneId;
            Coordinates = coordinates;
        }

        public bool Equals(Location other)
        {
            return ZoneId == other.ZoneId && Coordinates.Equals(other.Coordinates);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Location)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ZoneId.GetHashCode() * 397) ^ Coordinates.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"Zone: {ZoneName}({ZoneId}), Coordinates: {Coordinates}";
        }

        public static Location CurrentLocation()
        {
            return new Location(WorldManager.ZoneId, Core.Me.Location);
        }
    }
}