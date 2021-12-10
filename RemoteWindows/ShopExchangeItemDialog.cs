namespace LlamaLibrary.RemoteWindows
{
    public class ShopExchangeItemDialog : RemoteWindow<ShopExchangeItemDialog>
    {
        private const string WindowName = "ShopExchangeItemDialog";

        public ShopExchangeItemDialog() : base(WindowName)
        {
        }

        public void Exchange()
        {
            SendAction(1, 3, 0);
        }
    }
}