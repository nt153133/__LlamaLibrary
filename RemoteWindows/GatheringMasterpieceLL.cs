using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    public class GatheringMasterpieceLL : RemoteWindow<GatheringMasterpieceLL>
    {
        public GatheringMasterpieceLL() : base("GatheringMasterpiece")
        {
        }

        public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
        {
            {
                "Collectability",
                4
            },
            {
                "MaxCollectability",
                5
            },
            {
                "Integrity",
                49
            },
            {
                "MaxIntegrity",
                50
            },
            {
                "ItemID",
                10
            },
            {
                "IntuitionRate",
                45
            },
            {
                "Skill1",
                38
            },
            {
                "Skill2Estimate",
                39
            },
            {
                "Skill2Max",
                40
            },
            {
                "Skill3",
                41
            }
        };

        public int Collectability => Elements[Properties["Collectability"]].Int;
        public int MaxCollectability => Elements[Properties["MaxCollectability"]].Int;
        public int Integrity => Elements[Properties["Integrity"]].Int;
        public int MaxIntegrity => Elements[Properties["MaxIntegrity"]].Int;
        public int ItemID => Elements[Properties["ItemID"]].Int;
        public int Scour => Elements[Properties["Skill1"]].Int;
        public int BrazenEstimate => Elements[Properties["Skill2Estimate"]].Int;
        public int Brazen2Max => Elements[Properties["Skill2Max"]].Int;
        public int Meticulous => Elements[Properties["Skill3"]].Int;
        public int IntuitionRate => Elements[Properties["IntuitionRate"]].Int;

        public void Collect()
        {
            if (IsOpen)
            {
                SendAction(1, 3, 0);
            }
        }

        public void SetScrutiny(bool value = true)
        {
            if (value)
            {
                SendAction(3, 3, 0x65, 0, 0, 2, 1);
            }
            else
            {
                SendAction(3, 3, 0x65, 0, 0, 2, 0);
            }
        }

        public void SetCollectorsIntuition(bool value = true)
        {
            if (value)
            {
                SendAction(3, 3, 0x66, 0, 0, 2, 1);
            }
            else
            {
                SendAction(3, 3, 0x66, 0, 0, 2, 0);
            }
        }
    }
}