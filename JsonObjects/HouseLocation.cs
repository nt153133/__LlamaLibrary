using System;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.HousingTravel;

namespace LlamaLibrary.JsonObjects
{
    public class HouseLocation : IEquatable<HouseLocation>
    {
        public World World { get; set; }
        public HousingZone HousingZone { get; set; }
        public int Ward { get; set; }
        public int Plot { get; set; }

        // Apartment / FC private-chamber identity. Room is null for plot-level locations (and for
        // legacy serialized data that predates these fields), keeping old saves behaving as before.
        public int? Room { get; set; }
        public bool Subdivision { get; set; }
        public HousingRoomKind RoomKind { get; set; }

        public HouseLocation(World world, HousingZone housingZone, int ward, int plot)
        {
            World = world;
            HousingZone = housingZone;
            Ward = ward;
            Plot = plot;
        }

        public HouseLocation(HousingZone housingZone, int ward, int plot)
        {
            World = WorldHelper.CurrentWorld;
            HousingZone = housingZone;
            Ward = ward;
            Plot = plot;
        }

        public HouseLocation(HousingZone housingZone, int ward, int plot, int? room, HousingRoomKind roomKind = HousingRoomKind.None, bool subdivision = false)
            : this(housingZone, ward, plot)
        {
            Room = room;
            RoomKind = roomKind;
            Subdivision = subdivision;
        }

        public HouseLocation()
        {
        }

        public override string ToString()
        {
            var zoneName = HousingTraveler.TranslateZone(HousingZone).AddSpacesToEnum();
            var roomText = Room.HasValue ? $" R{Room.Value}" : string.Empty;
            return $"{World} - {zoneName} - W{Ward} P{Plot}{roomText}";
        }

        public bool Equals(HouseLocation? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return World == other.World && HousingTraveler.TranslateZone(HousingZone) == HousingTraveler.TranslateZone(other.HousingZone) && Ward == other.Ward && Plot == other.Plot && Room == other.Room && RoomKind == other.RoomKind && Subdivision == other.Subdivision;
        }

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

            return Equals((HouseLocation)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(World, HousingZone, Ward, Plot, Room, RoomKind, Subdivision);
        }
    }

    /// <summary>Classifies a <see cref="HouseLocation"/> that points at an instanced room.</summary>
    public enum HousingRoomKind
    {
        /// <summary>Not a room — a plot, house, or workshop location.</summary>
        None = 0,

        /// <summary>A residential apartment room.</summary>
        Apartment,

        /// <summary>A Free Company private chamber.</summary>
        FreeCompanyRoom,
    }
}