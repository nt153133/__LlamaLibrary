namespace LlamaLibrary.RemoteWindows
{
    public class MJIGatheringNoteBook : RemoteWindow<MJIGatheringNoteBook>
    {
        private const string WindowName = "MJIGatheringNoteBook";

        public MJIGatheringNoteBook() : base(WindowName)
        {
            _name = WindowName;
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}