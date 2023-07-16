namespace LlamaLibrary.RemoteWindows
{
    public class MiniGameAimg : RemoteWindow<MiniGameAimg>
    {
        public MiniGameAimg() : base("MiniGameAimg")
        {
        }

        public void PressButton()
        {
            SendAction(3, 3, 0xB, 3, 2, 3, 0);
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