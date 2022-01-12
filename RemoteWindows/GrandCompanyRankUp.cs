namespace LlamaLibrary.RemoteWindows
{
    public class GrandCompanyRankUp : RemoteWindow<GrandCompanyRankUp>
    {
        private const string WindowName = "GrandCompanyRankUp";

        public GrandCompanyRankUp() : base(WindowName)
        {
            _name = WindowName;
        }

        public void Confirm()
        {
            SendAction(1, 3, 0);
        }
    }
}