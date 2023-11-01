namespace LlamaLibrary.RemoteWindows
{
    public class FGSExitDialog : RemoteWindow<FGSExitDialog>
    {
        public FGSExitDialog() : base("FGSExitDialog")
        {
        }

        public void Leave()
        {
            SendAction(1, 3, 0);
        }
    }
}