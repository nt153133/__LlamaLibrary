namespace LlamaLibrary.RemoteWindows
{
    public class MateriaAttachDialog : RemoteWindow<MateriaAttachDialog>
    {
        public MateriaAttachDialog() : base("MateriaAttachDialog")
        {
        }

        public void ClickAttach()
        {
            SendAction(1, 3, 0);
        }
    }
}