namespace LlamaLibrary.RemoteWindows
{
    public class GrandCompanySupplyReward : RemoteWindow<GrandCompanySupplyReward>
    {
        public int SealReward => Elements[9].TrimmedData;

        public GrandCompanySupplyReward() : base("GrandCompanySupplyReward")
        {
        }

        public void Confirm()
        {
            SendAction(1, 3, 0);
        }
    }
}