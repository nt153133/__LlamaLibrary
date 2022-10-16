using System;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.Housing;

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
            var zoneName = HousingZone.ToString();

            if (zoneName.Contains("Shirogane"))
            {
                zoneName = "Shirogane";
            }
            else if (zoneName.Contains("Mist"))
            {
                zoneName = "Mist";
            }
            else if (zoneName.Contains("Goblet"))
            {
                zoneName = "Goblet";
            }
            else if (zoneName.Contains("LavenderBeds"))
            {
                zoneName = "LavenderBeds";
            }
            else if (zoneName.Contains("Empyreum"))
            {
                zoneName = "Empyreum";
            }

            return $"{World} - {zoneName} - W{Ward} P{Plot}";
        }

        public bool Equals(HouseLocation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return World == other.World && HousingZone == other.HousingZone && Ward == other.Ward && Plot == other.Plot;
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

            return Equals((HouseLocation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)World;
                hashCode = (hashCode * 397) ^ (int)HousingZone;
                hashCode = (hashCode * 397) ^ Ward;
                hashCode = (hashCode * 397) ^ Plot;
                return hashCode;
            }
        }
    }
}