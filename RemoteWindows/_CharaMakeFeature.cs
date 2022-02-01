namespace LlamaLibrary.RemoteWindows
{
    public class _CharaMakeFeature: RemoteWindow<_CharaMakeFeature>
    {
        private const string WindowName = "_CharaMakeFeature";

        public _CharaMakeFeature() : base(WindowName)
        {
            _name = WindowName;
        }

        public void ConfirmAppearance()
        {
            SendAction(1, 3, 0x64);
        }
    }
}