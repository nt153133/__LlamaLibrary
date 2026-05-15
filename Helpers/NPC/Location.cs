using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers.HousingTravel;
using LlamaLibrary.Helpers.LocationTracking;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers.NPC
{
    /// <summary>
    /// Represents an in-game location defined by a territory (zone) ID and world-space 3D coordinates.
    /// Used throughout LlamaLibrary to identify where an NPC, aetheryte, or point of interest resides in Eorzea,
    /// and to determine travel feasibility via teleport, housing travel, or proximity.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Location : IEquatable<Location>
    {
        /// <summary>
        /// Gets or sets the FFXIV territory identifier for this location, equivalent to <c>WorldManager.ZoneId</c>.
        /// </summary>
        /// <value>The numeric zone/territory ID, e.g. <c>132</c> for New Gridania.</value>
        [JsonProperty]
        public ushort ZoneId { get; set; }

        /// <summary>
        /// Gets or sets the world-space 3D coordinates of this location within its zone.
        /// </summary>
        /// <value>A <see cref="Vector3"/> representing the X, Y, and Z position in the game world.</value>
        [JsonProperty]
        public Vector3 Coordinates { get; set; }

        /// <summary>
        /// Gets the primary aetheryte associated with this location's zone and coordinates, as determined
        /// by <c>Navigation.GetPrimaryAetheryte</c>. Used to check teleport eligibility and cost.
        /// </summary>
        /// <value>The primary <see cref="AetheryteResult"/> for this zone, or <see langword="null"/> if none is mapped.</value>
        public AetheryteResult? ClosestAetherytePrimaryResult => DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == Navigation.GetPrimaryAetheryte(ZoneId, Coordinates));

        /// <summary>
        /// Gets the nearest aetheryte crystal located in the same zone as this location, ordered by 2D distance.
        /// Used as a fallback for distance calculations when the primary aetheryte is unavailable.
        /// </summary>
        /// <value>The nearest <see cref="AetheryteResult"/> in the same zone, or <see langword="null"/> if none exists.</value>
        public AetheryteResult? ClosestAetheryteResult => DataManager.AetheryteCache.Values.Where(i => i.ZoneId == ZoneId).OrderBy(i => i.Position.Distance2DSqr(Coordinates)).FirstOrDefault();

        /// <summary>
        /// Gets a value indicating whether the local player has the nearest primary aetheryte for this
        /// location unlocked and can directly teleport here.
        /// </summary>
        /// <value><see langword="true"/> if the player owns the required aetheryte via <c>WorldManager.HasAetheryteId</c>; otherwise <see langword="false"/>.</value>
        public bool CanTeleportTo
        {
            get
            {
                var closestAetheryte = ClosestAetherytePrimaryResult;
                return closestAetheryte != null && WorldManager.HasAetheryteId(closestAetheryte.Id);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this location's zone is an FFXIV residential district
        /// (e.g. Mist, Lavender Beds, The Goblet, Shirogane, or Empyreum).
        /// </summary>
        /// <value><see langword="true"/> if the zone is a housing district; otherwise <see langword="false"/>.</value>
        public bool IsHousingLocation => HousingTraveler.HousingZoneIds.Contains(ZoneId);

        /// <summary>
        /// Gets a value indicating whether the player's current zone matches this location's zone.
        /// </summary>
        /// <value><see langword="true"/> if <c>WorldManager.ZoneId</c> equals <see cref="ZoneId"/>; otherwise <see langword="false"/>.</value>
        public bool IsInCurrentZone => WorldManager.ZoneId == ZoneId;

        /// <summary>
        /// Gets a value indicating whether the primary aetheryte for this location is within the same
        /// aetheryte network region as the player's current zone. A location is "in the current area"
        /// when the player does not need to teleport to a different city-state to reach it.
        /// </summary>
        /// <value><see langword="true"/> if the aetheryte is reachable from the current zone's network; otherwise <see langword="false"/>.</value>
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

        /// <summary>
        /// Gets a value indicating whether the player can reach this location by any available means:
        /// direct aetheryte teleportation, housing travel, already being in the zone, or via a workshop zone shortcut.
        /// </summary>
        /// <value><see langword="true"/> if the location is reachable; otherwise <see langword="false"/>.</value>
        public bool CanGetTo => CanTeleportTo || IsHousingLocation || WorldManager.ZoneId == ZoneId || HouseTravelHelper.WorkshopZones.Contains(ZoneId);

        /// <summary>
        /// Gets the gil cost required to teleport to this location.
        /// Returns <see cref="int.MaxValue"/> if the location is unreachable, <c>0</c> if the player is already
        /// in the zone, and the actual aetheryte or housing town teleport cost otherwise.
        /// Housing locations on the home world use the direct housing aetheryte cost when available,
        /// or fall back to the associated town aetheryte cost.
        /// </summary>
        /// <value>Gil cost as an <see cref="int"/>, or <see cref="int.MaxValue"/> if unreachable.</value>
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

        /// <summary>
        /// Gets a human-readable display name for this location combining the zone name and the nearest
        /// aetheryte name, e.g. <c>"Limsa Lominsa Upper Decks (Limsa Lominsa)"</c>.
        /// Falls back to just the zone or aetheryte name when only one is available.
        /// </summary>
        /// <value>A formatted display string for the location's zone and aetheryte.</value>
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

        /// <summary>
        /// Initializes a new <see cref="Location"/> with the specified zone ID and world-space coordinates.
        /// </summary>
        /// <param name="zoneId">The FFXIV territory identifier for the target zone.</param>
        /// <param name="coordinates">The world-space 3D position within the zone.</param>
        public Location(ushort zoneId, Vector3 coordinates)
        {
            ZoneId = zoneId;
            Coordinates = coordinates;
        }

        /// <summary>
        /// Determines whether this location equals another <see cref="Location"/> by comparing
        /// <see cref="ZoneId"/> and <see cref="Coordinates"/>.
        /// </summary>
        /// <param name="other">The other location to compare against.</param>
        /// <returns><see langword="true"/> if both zone and coordinates match; otherwise <see langword="false"/>.</returns>
        public bool Equals(Location? other)
        {
            return other != null && ZoneId == other.ZoneId && Coordinates.Equals(other.Coordinates);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
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

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (ZoneId.GetHashCode() * 397) ^ Coordinates.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a string representation in the format <c>"Zone: {ZoneName}({ZoneId}), Coordinates: {Coordinates}"</c>.
        /// </summary>
        /// <returns>A human-readable description of this location.</returns>
        public override string ToString()
        {
            return $"Zone: {ZoneName}({ZoneId}), Coordinates: {Coordinates}";
        }

        /// <summary>
        /// Creates a <see cref="Location"/> representing the player's current zone and world position
        /// using <c>WorldManager.ZoneId</c> and <c>Core.Me.Location</c>.
        /// </summary>
        /// <returns>A new <see cref="Location"/> at the player's current position.</returns>
        public static Location CurrentLocation()
        {
            return new Location(WorldManager.ZoneId, Core.Me.Location);
        }

        /// <summary>
        /// From a collection of locations, finds the single closest reachable location using this priority order:
        /// <list type="number">
        ///   <item><description>Current zone first (with player position as the distance reference).</description></item>
        ///   <item><description>Current aetheryte area next.</description></item>
        ///   <item><description>Lowest teleport gil cost.</description></item>
        ///   <item><description>Shortest 2D distance to player (in zone) or to the nearest aetheryte (out of zone).</description></item>
        /// </list>
        /// </summary>
        /// <param name="locations">The candidate locations to evaluate.</param>
        /// <returns>The optimal reachable <see cref="Location"/>, or <see langword="null"/> if none are reachable.</returns>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static Location? GetClosestLocation(IEnumerable<Location> locations)
        {
            var meLocation = Core.Me.Location;
            var enumerable = locations.ToList();
            if (enumerable.Any(i => i.IsInCurrentZone))
            {
                return enumerable.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Coordinates.Distance2DSqr(meLocation)).FirstOrDefault();
            }

            var temp = enumerable.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost);
            if (temp.All(i => i.ClosestAetheryteResult != null))
            {
                return temp.ThenBy(i => i.Coordinates.Distance2DSqr(i.ClosestAetheryteResult!.Position)).FirstOrDefault();
            }

            return temp.FirstOrDefault();
        }
    }
}