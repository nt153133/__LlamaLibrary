namespace LlamaLibrary.RemoteWindows
{
    public class HousingMenu : RemoteWindow<HousingMenu>
    {
        public HousingMenu() : base("HousingMenu")
        {
        }

        public void SelectHousingGoods()
        {
            SendAction(1, 3, 0);
        }
    }
}