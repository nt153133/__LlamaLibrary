namespace LlamaLibrary.RemoteWindows
{
    public class MJIRecipeNoteBook : RemoteWindow<MJIRecipeNoteBook>
    {
        public MJIRecipeNoteBook() : base("MJIRecipeNoteBook")
        {
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}