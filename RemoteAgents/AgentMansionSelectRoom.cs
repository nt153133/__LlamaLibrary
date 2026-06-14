using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMansionSelectRoom : AgentInterface<AgentMansionSelectRoom>, IAgent
    {
        public static LLogger Log = new LLogger("AgentMansionSelectRoom", Colors.Lavender);

        protected AgentMansionSelectRoom(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => AgentMansionSelectRoomOffsets.Vtable;

        /// <summary>
        /// Selects an apartment by its (1-based) number. The <c>MansionSelectRoom</c> window must be open.
        /// </summary>
        /// <param name="apartmentNumber">The 1-based apartment number; 0 is not valid.</param>
        public void SelectApartment(int apartmentNumber)
        {
            if (apartmentNumber <= 0)
            {
                Log.Error($"Invalid apartment number {apartmentNumber}; must be 1 or greater.");
                return;
            }

            // Native signature: void SelectApartment(agentPtr, 0, apartmentNumber)
            Core.Memory.CallInjectedWraper<IntPtr>(AgentMansionSelectRoomOffsets.SelectApartmentFunction, Pointer, 0, apartmentNumber);
        }
    }
}
