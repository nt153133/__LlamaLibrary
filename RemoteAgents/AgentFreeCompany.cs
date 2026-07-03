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
    /// Remote agent for the Free Company interface.
    /// Manages information about the Free Company, including member roster and active/available company actions.
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
        /// Gets the memory pointer to the Free Company member roster.
        /// </summary>
        /// <returns>A pointer to the roster data.</returns>
        public IntPtr GetRosterPtr()
        {
            var ptr1 = Core.Memory.Read<IntPtr>(Pointer + 0x48);
            var ptr2 = Core.Memory.Read<IntPtr>(ptr1 + 0x98);

            return ptr2;
        }

        /// <summary>
        /// Retrieves the list of Free Company members and their current online status from game memory.
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
        /// Gets the number of lines in the Free Company history log.
        /// </summary>
        [Obsolete("Not sure what's using this but pattern is returning multiple values")]
        public byte HistoryLineCount => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyOffsets.HistoryCount);

        /// <summary>
        /// Gets the memory address where the Free Company action data is stored.
        /// Resolved through a series of offsets from the AtkStage.
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
        /// Retrieves the Free Company actions that are currently active (buffs).
        /// Opens the Free Company interface if it is not already open.
        /// </summary>
        /// <returns>An array of <see cref="FcAction"/> structures representing active actions.</returns>
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
        /// Retrieves the list of available Free Company actions that have been purchased or unlocked.
        /// Opens the Free Company interface if it is not already open.
        /// </summary>
        /// <returns>An array of <see cref="FcAction"/> structures representing available actions.</returns>
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
    /// Represents a Free Company action (buff).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xC)]
    public struct FcAction
    {
        /// <summary>The unique identifier of the action.</summary>
        public uint id;

        /// <summary>The icon identifier associated with the action.</summary>
        public uint iconId;

        /// <summary>An unknown field, possibly a status or timestamp.</summary>
        public uint unk;
    }
}