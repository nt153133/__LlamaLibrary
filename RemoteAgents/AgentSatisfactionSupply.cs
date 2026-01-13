using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentSatisfactionSupply : AgentInterface<AgentSatisfactionSupply>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentSatisfactionSupply;

        

        protected AgentSatisfactionSupply(IntPtr pointer) : base(pointer)
        {
        }

        public byte DeliveriesRemaining => Core.Memory.Read<byte>(Pointer + AgentSatisfactionSupplyOffsets.DeliveriesRemaining);
        public byte HeartLevel => Core.Memory.Read<byte>(Pointer + AgentSatisfactionSupplyOffsets.HeartLevel);
        public uint DoHItemId => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.DoHItemId);
        public uint DoLItemId => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.DoLItemId);
        public uint FshItemId => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.FshItemId);

        //public byte Npc => Core.Memory.Read<byte>(Pointer + Offsets.Npc);

        //6.3
//#if RB_CN
//        public uint NpcId => Core.Memory.Read<uint>(Pointer + AgentSatisfactionSupplyOffsets.NpcId);
//#else
        public uint NpcId => Core.Memory.Read<uint>(Pointer);
//#endif

        public uint CurrentRep => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.CurrentRep);
        public uint MaxRep => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.MaxRep);

        public bool HasDoHTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.DoHItemId);
        public bool HasDoLTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.DoLItemId);
        public bool HasFshTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.FshItemId);

        public bool HasAnyTurnin => HasDoHTurnin || HasDoLTurnin || HasFshTurnin;

        public async Task LoadWindow(uint npc)
        {
            if (SatisfactionSupply.Instance.IsOpen)
            {
                SatisfactionSupply.Instance.Close();
                await Coroutine.Wait(5000, () => !SatisfactionSupply.Instance.IsOpen);
            }

            Core.Memory.CallInjectedWraper<IntPtr>(AgentSatisfactionSupplyOffsets.OpenWindow,
                                               Pointer,
                                               0U,
                                               npc,
                                               1U);

            await Coroutine.Wait(5000, () => SatisfactionSupply.Instance.IsOpen);

            if (SatisfactionSupply.Instance.IsOpen)
            {
                SatisfactionSupply.Instance.Close();
                await Coroutine.Wait(5000, () => !SatisfactionSupply.Instance.IsOpen);
            }
        }
    }
}