namespace LlamaLibrary.RemoteWindows
{
    public class GrandCompanyRankUp : RemoteWindow<GrandCompanyRankUp>
    {
        public GrandCompanyRankUp() : base("GrandCompanyRankUp")
        {
        }

        public void Confirm()
        {
            SendAction(1, 3, 0);
        }
    }
}