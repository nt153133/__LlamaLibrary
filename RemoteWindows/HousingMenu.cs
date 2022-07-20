namespace LlamaLibrary.RemoteWindows
{
    public class HousingMenu : RemoteWindow<HousingMenu>
    {
        private const string WindowName = "HousingMenu";

        public HousingMenu() : base(WindowName)
        {
            _name = WindowName;
        }

        public void SelectHousingGoods()
        {
            SendAction(1, 3, 0);
        }
    }
}