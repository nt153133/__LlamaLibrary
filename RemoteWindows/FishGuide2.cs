namespace LlamaLibrary.RemoteWindows
{
    public class FishGuide2 : RemoteWindow<FishGuide2>
    {
        public FishGuide2() : base("FishGuide2")
        {
        }

        public void ClickTab(int index)
        {
            SendAction(2, 3, 9, 3, (ulong)index);
        }

        public void SelectFishing()
        {
            SendAction(1, 3, 3);
        }

        public void SelectSpearFishing()
        {
            SendAction(1, 3, 4);
        }
    }
}