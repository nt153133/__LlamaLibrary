using System;
using Clio.Utilities;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using Newtonsoft.Json;

namespace LlamaLibrary.JsonObjects
{
    public class RecordedPlot : IEquatable<RecordedPlot>
    {
        [JsonIgnore]
        public static uint PlacardNpcId = 2002736;

        public HousingZone HousingZone { get; set; }
        public int Plot { get; set; }
        public PlotSize Size { get; set; }
        public Vector3 PlacardLocation { get; set; }
        public Vector3 EntranceLocation { get; set; }
        public bool IsInSubdivision { get; set; }

        [JsonIgnore]
        public Npc Npc => new Npc(2002736, (ushort)HousingZone, PlacardLocation);

        public RecordedPlot(HousingZone housingZone, int plot, bool isInSubdivision)
        {
            HousingZone = housingZone;
            Plot = plot;
            IsInSubdivision = isInSubdivision;
        }

        public RecordedPlot(HousingZone housingZone, Vector3 placardLocation, bool isInSubdivision)
        {
            HousingZone = housingZone;
            PlacardLocation = placardLocation;
            IsInSubdivision = isInSubdivision;
        }

        public RecordedPlot()
        {
        }

        public bool Equals(RecordedPlot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HousingZone == other.HousingZone && Plot == other.Plot && PlacardLocation.Equals(other.PlacardLocation) && IsInSubdivision == other.IsInSubdivision;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecordedPlot)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)HousingZone;
                hashCode = (hashCode * 397) ^ Plot;
                hashCode = (hashCode * 397) ^ PlacardLocation.GetHashCode();
                hashCode = (hashCode * 397) ^ IsInSubdivision.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"HousingZone: {HousingZone}, Plot: {Plot}, Size: {Size}, PlacardLocation: {PlacardLocation}, EntranceLocation: {EntranceLocation}, IsInSubdivision: {IsInSubdivision}";
        }
    }
}