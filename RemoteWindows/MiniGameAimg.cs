namespace LlamaLibrary.RemoteWindows
{
    public class MiniGameAimg : RemoteWindow<MiniGameAimg>
    {
        public MiniGameAimg() : base("MiniGameAimg")
        {
        }

        public void PressButton()
        {
            SendAction(1, 3, 0xB);
        }

        public void PauseCursor()
        {
            SendAction(1, 3, 0xF);
        }

        public void ResumeCursor()
        {
            SendAction(1, 3, 0xF);
        }
    }
}