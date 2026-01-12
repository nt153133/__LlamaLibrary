using System;
using System.Linq;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent might be completely useless given the current way I get the retainers
    public class AgentRetainerList : AgentInterface<AgentRetainerList>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentRetainerList;
        

        protected AgentRetainerList(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr[] RetainerList => Core.Memory.ReadArray<IntPtr>(Pointer + AgentRetainerListOffsets.AgentRetainerOffset, AgentRetainerListOffsets.MaxRetainers);

        public RetainerInfo[] OrderedRetainerList(RetainerInfo[] retainers)
        {
            var count = RetainerList.Count(i => i != IntPtr.Zero);

            if (count == 0)
            {
                return retainers;
            }

            var result = new RetainerInfo[count]; // new List<KeyValuePair<int, RetainerInfo>>();

            //IntPtr[] RetainerList = Core.Memory.ReadArray<IntPtr>(new IntPtr(0x18FD0C64510) + 0x4a8, 0xA);
            var index = 0;
            foreach (var ptr in RetainerList.Where(i => i != IntPtr.Zero))
            {
                var next = Core.Memory.Read<IntPtr>(ptr);

                result[index] = retainers.First(j => j.Name.Equals(Core.Memory.ReadStringUTF8(next)));
                index++;
            }

            return result;
        }
    }
}