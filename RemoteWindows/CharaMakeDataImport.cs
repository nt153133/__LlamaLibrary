namespace LlamaLibrary.RemoteWindows
{
    public class CharaMakeDataImport : RemoteWindow<CharaMakeDataImport>
    {
        private const string WindowName = "CharaMakeDataImport";

        public CharaMakeDataImport() : base(WindowName)
        {
            _name = WindowName;
        }

        public void SelectAppearanceSave(int index)
        {
            SendAction(2, 3, 0x66, 3, 0);
        }
    }
}