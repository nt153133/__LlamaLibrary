using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has way too many hardcoded memory offsets
    public class AgentFreeCompany : AgentInterface<AgentFreeCompany>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentFreeCompany;
        

        protected AgentFreeCompany(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr GetRosterPtr()
        {
            var ptr1 = Core.Memory.Read<IntPtr>(Pointer + 0x48);
            var ptr2 = Core.Memory.Read<IntPtr>(ptr1 + 0x98);

            return ptr2;
        }

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

        public byte HistoryLineCount => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyOffsets.HistoryCount);

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

    [StructLayout(LayoutKind.Sequential, Size = 0xC)]
    public struct FcAction
    {
        public uint id;
        public uint iconId;
        public uint unk;
    }
}