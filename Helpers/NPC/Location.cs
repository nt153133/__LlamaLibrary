using System;
using System.Collections.Generic;
using System.Linq;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.Helpers.LocationTracking;
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

        public AetheryteResult? ClosestAetherytePrimaryResult => DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == Navigation.GetPrimaryAetheryte(ZoneId, Coordinates));

        public AetheryteResult? ClosestAetheryteResult => DataManager.AetheryteCache.Values.Where(i => i.ZoneId == ZoneId).OrderBy(i => i.Position.Distance2DSqr(Coordinates)).FirstOrDefault();

        public bool CanTeleportTo
        {
            get
            {
                var closestAetheryte = ClosestAetherytePrimaryResult;
                return closestAetheryte != null && WorldManager.HasAetheryteId(closestAetheryte.Id);
            }
        }

        public bool IsHousingLocation => HousingTraveler.HousingZoneIds.Contains(ZoneId);

        public bool IsInCurrentZone => WorldManager.ZoneId == ZoneId;

        public bool IsInCurrentArea
        {
            get
            {
                var ae = ClosestAetherytePrimaryResult;
                if (ae == null)
                {
                    return false;
                }

                return WorldManager.AetheryteIdsForZone(WorldManager.ZoneId).Select(i => i.Item1).Contains(ae.Id);
            }
        }

        public bool CanGetTo => CanTeleportTo || IsHousingLocation || WorldManager.ZoneId == ZoneId || HouseTravelHelper.WorkshopZones.Contains(ZoneId);

        public int TeleportCost
        {
            get
            {
                if (!CanTeleportTo && !IsHousingLocation)
                {
                    return int.MaxValue;
                }

                if (WorldManager.ZoneId == ZoneId)
                {
                    return 0;
                }

                if (IsHousingLocation)
                {
                    if (WorldHelper.IsOnHomeWorld && WorldManager.AvailableLocations.Any(i => i.ZoneId == ZoneId))
                    {
                        return (int)WorldManager.AvailableLocations.First(i => i.ZoneId == ZoneId).GilCost;
                    }

                    var zone = HousingTraveler.HousingZones.First(i => i.ZoneId == ZoneId);

                    return (int)WorldManager.AvailableLocations.First(i => i.AetheryteId == zone.TownAetheryteId).GilCost;
                }

                var ae = ClosestAetherytePrimaryResult;
                if (ae == null)
                {
                    return int.MaxValue;
                }

                return (int)WorldManager.AvailableLocations.First(i => i.AetheryteId == ae.Id).GilCost;
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

        public static Location? GetClosestLocation(IEnumerable<Location> locations)
        {
            if (locations.Any(i => i.IsInCurrentZone))
            {
                return locations.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Coordinates.Distance2DSqr(Core.Me.Location)).FirstOrDefault();
            }

            var temp = locations.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost);
            if (temp.All(i => i.ClosestAetheryteResult != null))
            {
                return temp.ThenBy(i => i.Coordinates.Distance2DSqr(i.ClosestAetheryteResult.Position)).FirstOrDefault();
            }

            return temp.FirstOrDefault();
        }
    }
}