namespace LlamaLibrary.RemoteWindows
{
    public class Bank : RemoteWindow<Bank>
    {
        private const string WindowName = "Bank";

        public Bank() : base(WindowName)
        {
            _name = WindowName;
        }

        public void ClickWithdrawalDeposit()
        {
            SendAction(1, 3, 2);
        }

        public void SetAmount(int amount)
        {
            SendAction(2, 3, 3, 4, (ulong)amount);
        }

        public void Proceed()
        {
            SendAction(1, 3, 0);
        }

        public void Cancel()
        {
            SendAction(1, 3, 0);
        }
    }
}