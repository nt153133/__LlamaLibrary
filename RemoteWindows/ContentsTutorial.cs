namespace LlamaLibrary.RemoteWindows
{
    public class ContentsTutorial : RemoteWindow<ContentsTutorial>
    {
        private const string WindowName = "ContentsTutorial";

        public ContentsTutorial() : base(WindowName)
        {
            _name = WindowName;
        }

        public override void Close()
        {
            SendAction(1, 3, 0xD);
        }
    }
}