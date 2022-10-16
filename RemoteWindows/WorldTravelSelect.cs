namespace LlamaLibrary.RemoteWindows
{
    public class WorldTravelSelect : RemoteWindow<WorldTravelSelect>
    {
        public WorldTravelSelect() : base("WorldTravelSelect")
        {
        }

        public void SelectWorld(int index)
        {
            SendAction(1, 3, (ulong)(index + 2));
        }
    }
}