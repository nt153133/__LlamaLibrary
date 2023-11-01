namespace LlamaLibrary.RemoteWindows
{
    public class FGSSpectatorMenu : RemoteWindow<FGSSpectatorMenu>
    {
        public FGSSpectatorMenu() : base("FGSSpectatorMenu")
        {
        }

        public void Leave()
        {
            SendAction(1, 3, 3);
        }
    }
}