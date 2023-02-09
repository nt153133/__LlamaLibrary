namespace LlamaLibrary.RemoteWindows
{
    public class JournalDetail : RemoteWindow<JournalDetail>
    {
        public JournalDetail() : base("JournalDetail")
        {
        }

        public void SetQuest(ulong globalId)
        {
            SendAction(3, 3, 0xD, 3, globalId, 3, 2);
        }

        public void InitiateLeve(ulong globalId)
        {
            SendAction(2, 3, 4, 4, globalId);
        }

        public override void Close()
        {
            SendAction(1, 3uL, 0xFFFFFFFFuL);
        }
    }
}