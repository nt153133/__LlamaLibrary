namespace LlamaLibrary.RemoteWindows
{
    public class MJIPouch : RemoteWindow<MJIPouch>
    {
        private const string WindowName = "MJIPouch";

        public MJIPouch() : base(WindowName)
        {
            _name = WindowName;
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}