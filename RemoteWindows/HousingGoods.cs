namespace LlamaLibrary.RemoteWindows
{
    public class HousingGoods : RemoteWindow<HousingGoods>
    {
        private const string WindowName = "HousingGoods";

        public HousingGoods() : base(WindowName)
        {
            _name = WindowName;
        }
    }
}