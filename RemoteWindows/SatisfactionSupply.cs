namespace LlamaLibrary.RemoteWindows
{
    public class SatisfactionSupply : RemoteWindow<SatisfactionSupply>
    {
        public SatisfactionSupply() : base("SatisfactionSupply")
        {
        }

        public void ClickItem(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }
    }
}