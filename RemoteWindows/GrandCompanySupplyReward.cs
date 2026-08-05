using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    public class GrandCompanySupplyReward : RemoteWindow<GrandCompanySupplyReward>
    {
        public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
        {
            { "SealReward", 9 },
        };

        public int SealReward => Elements[Properties["SealReward"]].Int;

        public GrandCompanySupplyReward() : base("GrandCompanySupplyReward")
        {
        }

        public void Confirm()
        {
            SendAction(1, 3, 0);
        }
    }
}