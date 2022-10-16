namespace LlamaLibrary.RemoteWindows
{
    public class GcArmyExpeditionResult : RemoteWindow<GcArmyExpeditionResult>
    {
        public GcArmyExpeditionResult() : base("GcArmyExpeditionResult")
        {
        }

        public override void Close()
        {
            SendAction(1, 3, 0);
        }
    }
}