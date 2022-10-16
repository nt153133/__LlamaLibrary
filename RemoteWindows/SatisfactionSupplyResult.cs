namespace LlamaLibrary.RemoteWindows
{
    public class SatisfactionSupplyResult : RemoteWindow<SatisfactionSupplyResult>
    {
        public SatisfactionSupplyResult() : base("SatisfactionSupplyResult")
        {
        }

        public void Confirm()
        {
            SendAction(1, 3, 1);
        }
    }
}