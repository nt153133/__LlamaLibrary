namespace LlamaLibrary.RemoteWindows
{
    public class MJIGatheringNoteBook : RemoteWindow<MJIGatheringNoteBook>
    {
        public MJIGatheringNoteBook() : base("MJIGatheringNoteBook")
        {
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}