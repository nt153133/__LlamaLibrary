namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class HugeCraftworksSupplyResul : RemoteWindow<HugeCraftworksSupplyResul>
    {
        private const string WindowName = "HugeCraftworksSupplyResul";

        public HugeCraftworksSupplyResul() : base(WindowName)
        {
            _name = WindowName;
        }

        public void Accept()
        {
            SendAction(1, 3, 0);
        }
    }
}