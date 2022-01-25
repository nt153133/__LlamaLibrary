namespace LlamaLibrary.RemoteWindows
{
    public class FreeCompanyChest : RemoteWindow<FreeCompanyChest>
    {
        private const string WindowName = "FreeCompanyChest";
        public FreeCompanyChest() : base(WindowName)
        {
            _name = WindowName;
        }
    }
}