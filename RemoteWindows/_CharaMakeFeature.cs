namespace LlamaLibrary.RemoteWindows
{
    public class _CharaMakeFeature : RemoteWindow<_CharaMakeFeature>
    {
        public _CharaMakeFeature() : base("_CharaMakeFeature")
        {
        }

        public void ConfirmAppearance()
        {
            SendAction(1, 3, 0x64);
        }
    }
}