namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    ///     Class for using the Blunderville registration window.
    /// </summary>
    public class FGSEnterDialog : RemoteWindow<FGSEnterDialog>
    {
        public FGSEnterDialog() : base("FGSEnterDialog")
        {
        }

        public void Register()
        {
            SendAction(1, 3, 0);
        }

    }
}