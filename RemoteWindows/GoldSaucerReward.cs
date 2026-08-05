using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    public class GoldSaucerReward : RemoteWindow<GoldSaucerReward>
    {
        public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
        {
            { "MGPReward", 1 },
        };

        public GoldSaucerReward() : base("GoldSaucerReward")
        {
        }

        public int MGPReward => Elements[Properties["MGPReward"]].Int;
    }
}