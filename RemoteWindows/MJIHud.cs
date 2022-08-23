namespace LlamaLibrary.RemoteWindows
{
    public class MJIHud : RemoteWindow<MJIHud>
    {
        private const string WindowName = "MJIHud";

        public MJIHud() : base(WindowName)
        {
            _name = WindowName;
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}