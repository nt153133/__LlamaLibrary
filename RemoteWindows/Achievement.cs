namespace LlamaLibrary.RemoteWindows
{
    public class Achievement : RemoteWindow<Achievement>
    {
        private const string WindowName = "Achievement";

        public Achievement() : base(WindowName)
        {
            _name = WindowName;
        }
    }
}