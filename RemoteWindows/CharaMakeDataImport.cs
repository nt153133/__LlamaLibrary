namespace LlamaLibrary.RemoteWindows
{
    public class CharaMakeDataImport : RemoteWindow<CharaMakeDataImport>
    {
        public CharaMakeDataImport() : base("CharaMakeDataImport")
        {
        }

        public void SelectAppearanceSave(int index)
        {
            SendAction(2, 3, 0x66, 3, 0);
        }
    }
}