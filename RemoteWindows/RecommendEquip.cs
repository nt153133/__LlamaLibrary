namespace LlamaLibrary.RemoteWindows
{
    public class RecommendEquip : RemoteWindow<RecommendEquip>
    {
        public RecommendEquip() : base("RecommendEquip")
        {
        }

        public void Confirm()
        {
            SendAction(1, 3, 0);
        }
    }
}