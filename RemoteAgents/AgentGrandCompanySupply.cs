using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentGrandCompanySupply : AgentInterface<AgentGrandCompanySupply>, IAgent
    {
        

        public IntPtr RegisteredVtable => AgentGrandCompanySupplyOffsets.Vtable;
        public IntPtr SupplyItemPtr => Core.Memory.Read<IntPtr>(Pointer + AgentGrandCompanySupplyOffsets.ItemArrayStart);
        public int SupplyItemCount => Core.Memory.Read<int>(Pointer + AgentGrandCompanySupplyOffsets.ArrayCount);

        public IntPtr SortArrayPtr => Core.Memory.Read<IntPtr>(Pointer + AgentGrandCompanySupplyOffsets.SortArray);

        public int[] SortArray => Core.Memory.ReadArray<int>(SortArrayPtr, SupplyItemCount);

        public GCSupplyItem[] SupplyItems => Core.Memory.ReadArray<GCSupplyItem>(SupplyItemPtr, SupplyItemCount);

        public GCSupplyItem[] ExpertSupplyItems
        {
            get
            {
                var sort = SortArray.Skip(11);
                var itemArray = SupplyItems;
                var resultItems = new List<GCSupplyItem>();
                foreach (var index in sort)
                {
                    var item = itemArray[index];

                    if (HandinType != (GCSupplyType)item.HandInType)
                    {
                        continue;
                    }

                    switch (ExpertFilter)
                    {
                        case GCFilter.All:
                            resultItems.Add(item);
                            break;
                        case GCFilter.HideGearSet:
                            if (!item.InGearSet)
                            {
                                resultItems.Add(item);
                            }

                            break;
                        case GCFilter.HideArmory:
                            if (!item.InArmory)
                            {
                                resultItems.Add(item);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return resultItems.ToArray();
            }
        }

        public GCSupplyType HandinType
        {
            get => (GCSupplyType)Core.Memory.Read<byte>(Pointer + AgentGrandCompanySupplyOffsets.HandinType);
            set => Core.Memory.Write(Pointer + AgentGrandCompanySupplyOffsets.HandinType, (byte)value);
        }

        public GCFilter ExpertFilter
        {
            get => (GCFilter)Core.Memory.Read<byte>(Pointer + AgentGrandCompanySupplyOffsets.ExpertFilter);
            set => Core.Memory.Write(Pointer + AgentGrandCompanySupplyOffsets.ExpertFilter, (byte)value);
        }

        protected AgentGrandCompanySupply(IntPtr pointer) : base(pointer)
        {
        }
    }

    public enum GCSupplyType : byte
    {
        Supply = 0,
        Provisioning = 1,
        Expert = 2,
    }

    public enum GCFilter : byte
    {
        All = 0,
        HideGearSet = 1,
        HideArmory = 2,
    }
}