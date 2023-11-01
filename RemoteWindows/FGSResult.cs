namespace LlamaLibrary.RemoteWindows
{
    public class FGSResult : RemoteWindow<FGSResult>
    {
        public FGSResult() : base("FGSResult")
        {
        }

        public void Leave()
        {
            SendAction(1, 3, 0xB);
        }
    }
}