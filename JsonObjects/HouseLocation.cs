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

        public HouseLocation()
        {
        }

        public override string ToString()
        {
            var zoneName = HousingTraveler.TranslateZone(HousingZone).AddSpacesToEnum();
            return $"{World} - {zoneName} - W{Ward} P{Plot}";
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

            return World == other.World && HousingTraveler.TranslateZone(HousingZone) == HousingTraveler.TranslateZone(other.HousingZone) && Ward == other.Ward && Plot == other.Plot;
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
            return HashCode.Combine(World, HousingZone, Ward, Plot);
        }
    }
}