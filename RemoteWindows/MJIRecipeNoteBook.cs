namespace LlamaLibrary.RemoteWindows
{
    public class MJIRecipeNoteBook : RemoteWindow<MJIRecipeNoteBook>
    {
        private const string WindowName = "MJIRecipeNoteBook";

        public MJIRecipeNoteBook() : base(WindowName)
        {
            _name = WindowName;
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}