namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class HugeCraftworksSupplyResul : RemoteWindow<HugeCraftworksSupplyResul>
    {
        public HugeCraftworksSupplyResul() : base("HugeCraftworksSupplyResul")
        {
        }

        public void Accept()
        {
            SendAction(1, 3, 0);
        }
    }
}