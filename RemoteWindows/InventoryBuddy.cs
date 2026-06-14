using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "InventoryBuddy" window (Chocobo Saddlebag).
    /// Used to manage items stored in the player's chocobo saddlebags.
    /// </summary>
    public class InventoryBuddy : RemoteWindow<InventoryBuddy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryBuddy"/> class.
        /// </summary>
        public InventoryBuddy() : base("InventoryBuddy", AgentInventoryBuddy.Instance)
        {
        }
    }
}