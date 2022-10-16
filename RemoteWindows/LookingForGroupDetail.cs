namespace LlamaLibrary.RemoteWindows
{
    public class LookingForGroupDetail : RemoteWindow<LookingForGroupDetail>
    {
        public LookingForGroupDetail() : base("LookingForGroupDetail")
        {
        }

        public void EndRecruitment()
        {
            SendAction(1, 3, 0xB);
        }
    }
}