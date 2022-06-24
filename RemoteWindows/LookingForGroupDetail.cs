namespace LlamaLibrary.RemoteWindows
{
    public class LookingForGroupDetail : RemoteWindow<LookingForGroupDetail>
    {
        private const string WindowName = "LookingForGroupDetail";

        public LookingForGroupDetail() : base(WindowName)
        {
        }

        public void EndRecruitment()
        {
            SendAction(1, 3, 0xB);
        }
    }
}