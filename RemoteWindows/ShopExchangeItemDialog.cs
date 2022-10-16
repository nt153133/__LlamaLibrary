namespace LlamaLibrary.RemoteWindows
{
    public class ShopExchangeItemDialog : RemoteWindow<ShopExchangeItemDialog>
    {
        public ShopExchangeItemDialog() : base("ShopExchangeItemDialog")
        {
        }

        public void Exchange()
        {
            SendAction(1, 3, 0);
        }
    }
}