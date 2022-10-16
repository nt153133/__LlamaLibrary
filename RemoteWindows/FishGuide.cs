namespace LlamaLibrary.RemoteWindows
{
    public class FishGuide : RemoteWindow<FishGuide>
    {
        public FishGuide() : base("FishGuide")
        {
        }

        public void ClickTab(int index)
        {
            SendAction(2, 3, 8, 3, (ulong)index);
        }
    }
}