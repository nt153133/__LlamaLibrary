namespace LlamaLibrary.RemoteWindows
{
    public class ShopProxy : RemoteWindow<ShopProxy>
    {
        private const string WindowName = "Shop";

        public ShopProxy() : base(WindowName)
        {
            _name = WindowName;
        }

    }
}