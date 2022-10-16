namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class GoldSaucerReward : RemoteWindow<GoldSaucerReward>
    {
        public GoldSaucerReward() : base("GoldSaucerReward")
        {
        }

        public int MGPReward => Elements[1].TrimmedData;
    }
}