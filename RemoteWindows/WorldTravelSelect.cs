namespace LlamaLibrary.RemoteWindows
{
    public class WorldTravelSelect : RemoteWindow<WorldTravelSelect>
    {
        private const string WindowName = "WorldTravelSelect";

        public WorldTravelSelect() : base(WindowName)
        {
        }

        public void SelectWorld(int index)
        {
            SendAction(1, 3, (ulong)(index + 2));
        }
    }
}