namespace LlamaLibrary.RemoteWindows
{
    public class HelpWindow : RemoteWindow<HelpWindow>
    {
        private const string WindowName = "HelpWindow";

        public HelpWindow() : base(WindowName)
        {
            _name = WindowName;
        }
    }
}