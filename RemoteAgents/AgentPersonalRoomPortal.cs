using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the personal room portal interface (HousingSelectRoom).
    /// Manages entering and selecting FC private chambers (individual player rooms).
    /// </summary>
    public class AgentPersonalRoomPortal : AgentInterface<AgentPersonalRoomPortal>, IAgent
    {
        /// <summary>
        /// The logger instance for this agent.
        /// </summary>
        public static LLogger Log = new LLogger("AgentPersonalRoomPortal", Colors.Lavender);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentPersonalRoomPortal"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentPersonalRoomPortal(IntPtr pointer) : base(pointer)
        {
        }

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentPersonalRoomPortalOffsets.Vtable;

        /// <summary>
        /// Selects an FC private chamber by its (1-based) room number. The <c>HousingSelectRoom</c> window must be open.
        /// </summary>
        /// <param name="roomNumber">The 1-based room number; 0 is not valid.</param>
        /// <remarks>
        /// This agent uses 1-based numbering for selecting FC private chambers; inputs must be greater than zero.
        /// </remarks>
        public void SelectRoom(int roomNumber)
        {
            if (roomNumber <= 0)
            {
                Log.Error($"Invalid room number {roomNumber}; must be 1 or greater.");
                return;
            }

            // Reuses the apartment select function with this agent's pointer.
            // Native signature: void Select(agentPtr, 0, roomNumber)
            Core.Memory.CallInjectedWraper<IntPtr>(AgentMansionSelectRoomOffsets.SelectApartmentFunction, Pointer, 0, roomNumber);
        }
    }
}
