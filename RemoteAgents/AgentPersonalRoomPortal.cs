using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentPersonalRoomPortal : AgentInterface<AgentPersonalRoomPortal>, IAgent
    {
        public static LLogger Log = new LLogger("AgentPersonalRoomPortal", Colors.Lavender);

        protected AgentPersonalRoomPortal(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => AgentPersonalRoomPortalOffsets.Vtable;

        /// <summary>
        /// Selects an FC private chamber by its (1-based) room number. The <c>HousingSelectRoom</c> window must be open.
        /// </summary>
        /// <param name="roomNumber">The 1-based room number; 0 is not valid.</param>
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
