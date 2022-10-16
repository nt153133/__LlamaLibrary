namespace LlamaLibrary.RemoteWindows
{
    public class MJIHud : RemoteWindow<MJIHud>
    {
        public MJIHud() : base("MJIHud")
        {
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}