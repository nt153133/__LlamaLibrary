namespace LlamaLibrary.RemoteWindows
{
    public class ContentsTutorial : RemoteWindow<ContentsTutorial>
    {
        public ContentsTutorial() : base("ContentsTutorial")
        {
        }

        public override void Close()
        {
            SendAction(1, 3, 0xD);
        }
    }
}