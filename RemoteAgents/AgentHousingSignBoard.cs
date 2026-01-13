using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentHousingSignboard : AgentInterface<AgentHousingSignboard>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentHousingSignBoard;

        

        protected AgentHousingSignboard(IntPtr pointer) : base(pointer)
        {
        }

        public ushort Zone => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.Zone);

        public byte Ward => (byte)(Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Ward) + 1);

        public byte Plot => (byte)(Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Plot) + 1);

        public bool ForSale => Core.Memory.Read<bool>(Pointer + AgentHousingSignBoardOffsets.ForSale);

        public PlotSize Size => (PlotSize)Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Size);

        public ushort WinningLotteryNumber => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.LotteryEntryCount + 0xC);

        public ushort LotteryEntryCount => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.WinningLotteryNumber);

        public bool FcOwned => Core.Memory.Read<int>(Pointer + AgentHousingSignBoardOffsets.FcOwned) != 0;

        public override string ToString()
        {
            return $"Zone: {Zone}, Ward: {Ward}, Plot: {Plot}, ForSale: {ForSale}, Size: {Size}, LotteryEntryCount: {LotteryEntryCount}, WinningLotteryNumber: {WinningLotteryNumber}, FcOwned: {FcOwned}";
        }
    }
}