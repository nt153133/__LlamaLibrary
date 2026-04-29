namespace LlamaLibrary.RemoteWindows
{
    public class WorldTravelSelect : RemoteWindow<WorldTravelSelect>
    {
        public WorldTravelSelect() : base("WorldTravelSelect")
        {
        }

        public void SelectWorld(int index)
        {
            //7.5
#if RB_TC
                SendAction(1, 3, (ulong)(index + 2));
#else
            SendAction(2, 3, 0, 3, (ulong)(index + 2));
#endif
        }
    }
}