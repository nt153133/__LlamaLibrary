namespace LlamaLibrary.RemoteWindows
{
    public class RetainerTaskList : RemoteWindow<RetainerTaskList>
    {
        public RetainerTaskList() : base("RetainerTaskList")
        {
        }

        public void SelectVenture(int taskId)
        {
            SendAction(2, 3, 0x0B, 03, (ulong)taskId);
        }
    }
}