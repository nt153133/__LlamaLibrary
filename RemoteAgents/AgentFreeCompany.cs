using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Free Company (FC) interface.
    /// Manages the FC roster, online status of members, and active/available FC actions.
    /// </summary>
    //TODO This agent has way too many hardcoded memory offsets
    public class AgentFreeCompany : AgentInterface<AgentFreeCompany>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFreeCompanyOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentFreeCompany"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentFreeCompany(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the base memory pointer for the Free Company member roster.
        /// </summary>
        /// <returns>The pointer to the member roster data.</returns>
        public IntPtr GetRosterPtr()
        {
            var ptr1 = Core.Memory.Read<IntPtr>(Pointer + 0x48);
            var ptr2 = Core.Memory.Read<IntPtr>(ptr1 + 0x98);

            return ptr2;
        }

        /// <summary>
        /// Retrieves a list of all Free Company members including their names and current online status.
        /// </summary>
        /// <returns>A list of tuples containing member names and their online status.</returns>
        public List<(string Name, bool Online)> GetMembers()
        {
            var i = 0;
            var result = new List<(string, bool)>();
            var start = GetRosterPtr();
            byte testByte;
            do
            {
                var addr = start + (i * 0x60);
                testByte = Core.Memory.Read<byte>(addr);
                if (testByte != 0)
                {
                    result.Add((Core.Memory.ReadStringUTF8(addr + 0x22), Core.Memory.Read<byte>(addr + 0xD) != 0));
                }

                i++;
            }
            while (testByte != 0);

            return result;
        }

        /// <summary>
        /// Gets the number of entries currently displayed in the FC history log.
        /// </summary>
        [Obsolete("Not sure what's using this but pattern is returning multiple values")]
        public byte HistoryLineCount => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyOffsets.HistoryCount);

        /// <summary>
        /// Gets the memory address of the FC actions structure by traversing the ATK stage hierarchy.
        /// </summary>
        public IntPtr ActionAddress
        {
            get
            {
                var one = Core.Memory.Read<IntPtr>(Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + AgentFreeCompanyOffsets.off1);
                var three = Core.Memory.Read<IntPtr>(two + AgentFreeCompanyOffsets.off2);
                var four = Core.Memory.Read<IntPtr>(three + AgentFreeCompanyOffsets.off3);
                var final = Core.Memory.Read<IntPtr>(four + AgentFreeCompanyOffsets.off4);
                return final;
            }
        }

        /// <summary>
        /// Retrieves the list of FC actions that are currently active (primed and providing buffs).
        /// </summary>
        /// <returns>An array of <see cref="FcAction"/> representing active buffs.</returns>
        public async Task<FcAction[]> GetCurrentActions()
        {
            var wasopen = FreeCompany.Instance.IsOpen;
            if (!FreeCompany.Instance.IsOpen)
            {
                Instance.Toggle();
                await Coroutine.Wait(5000, () => FreeCompany.Instance.IsOpen);
            }

            if (FreeCompany.Instance.IsOpen)
            {
                FreeCompany.Instance.SelectActions();
                await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                if (FreeCompanyAction.Instance.IsOpen)
                {
                    var numCurrentActions = Core.Memory.NoCacheRead<uint>(ActionAddress + AgentFreeCompanyOffsets.CurrentCount);
                    var currentActions = Core.Memory.ReadArray<FcAction>(ActionAddress + 0x8, (int)numCurrentActions);
                    if (!wasopen)
                    {
                        FreeCompany.Instance.Close();
                    }

                    return currentActions;
                }
            }

            return new FcAction[0];
        }

        /// <summary>
        /// Retrieves the list of FC actions available for activation from the company's current stock.
        /// </summary>
        /// <returns>An array of <see cref="FcAction"/> representing available actions.</returns>
        public async Task<FcAction[]> GetAvailableActions()
        {
            var wasopen = FreeCompany.Instance.IsOpen;
            if (!FreeCompany.Instance.IsOpen)
            {
                Instance.Toggle();
                await Coroutine.Wait(5000, () => FreeCompany.Instance.IsOpen);
            }

            if (FreeCompany.Instance.IsOpen)
            {
                FreeCompany.Instance.SelectActions();
                await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                if (FreeCompanyAction.Instance.IsOpen)
                {
                    var actionCount = Core.Memory.NoCacheRead<uint>(ActionAddress + AgentFreeCompanyOffsets.ActionCount);
                    var actions = Core.Memory.ReadArray<FcAction>(ActionAddress + 0x30, (int)actionCount);
                    if (!wasopen)
                    {
                        FreeCompany.Instance.Close();
                    }

                    return actions;
                }
            }

            return new FcAction[0];
        }
    }

    /// <summary>
    /// Represents a single Free Company action (buff) in memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xC)]
    public struct FcAction
    {
        /// <summary>The internal action identifier.</summary>
        public uint id;
        /// <summary>The icon identifier for the action.</summary>
        public uint iconId;
        /// <summary>Unknown field at offset 0x8.</summary>
        public uint unk;
    }
}