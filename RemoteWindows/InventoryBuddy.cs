using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    public class InventoryBuddy : RemoteWindow<InventoryBuddy>
    {
        public InventoryBuddy() : base("InventoryBuddy", AgentInventoryBuddy.Instance)
        {
        }
    }
}