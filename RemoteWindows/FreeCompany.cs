namespace LlamaLibrary.RemoteWindows
{
    public class FreeCompany : RemoteWindow<FreeCompany>
    {
        public FreeCompany() : base("FreeCompany")
        {
        }

        public void SelectActions()
        {
            SendAction(2, 3, 0, 4, 4);
        }
    }
}