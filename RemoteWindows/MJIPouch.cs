namespace LlamaLibrary.RemoteWindows
{
    public class MJIPouch : RemoteWindow<MJIPouch>
    {
        public MJIPouch() : base("MJIPouch")
        {
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}