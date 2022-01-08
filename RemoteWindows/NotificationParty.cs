namespace LlamaLibrary.RemoteWindows
{
    public class NotificationParty : RemoteWindow<NotificationParty>
    {
        private const string WindowName = "_NotificationParty";

        public NotificationParty() : base(WindowName)
        {
            _name = WindowName;
        }
    }
}