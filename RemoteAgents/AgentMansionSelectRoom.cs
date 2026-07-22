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
    /// Remote agent for the apartment room selection interface (MansionSelectRoom).
    /// Manages selecting rooms in apartment buildings or FC private chambers.
    /// </summary>
    public class AgentMansionSelectRoom : AgentInterface<AgentMansionSelectRoom>, IAgent
    {
        /// <summary>
        /// The logger instance for this agent.
        /// </summary>
        public static LLogger Log = new LLogger("AgentMansionSelectRoom", Colors.Lavender);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMansionSelectRoom"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMansionSelectRoom(IntPtr pointer) : base(pointer)
        {
        }

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMansionSelectRoomOffsets.Vtable;

        /// <summary>
        /// Selects an apartment by its (1-based) number. The <c>MansionSelectRoom</c> window must be open.
        /// </summary>
        /// <param name="apartmentNumber">The 1-based apartment number; 0 is not valid.</param>
        /// <remarks>
        /// This agent uses 1-based numbering for selecting apartments; inputs must be greater than zero.
        /// </remarks>
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
