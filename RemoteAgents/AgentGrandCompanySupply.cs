using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentGrandCompanySupply : AgentInterface<AgentGrandCompanySupply>, IAgent
    {
        internal static class Offsets
        {
            //0x
            [Offset("Search 48 8D 05 ? ? ? ? 48 8B D9 48 89 01 E8 ? ? ? ? 48 8D 05 ? ? ? ? 48 8B CB 48 89 43 ? 48 83 C4 ? 5B E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 79 ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //0x68 ptr to GCSupplyItem[]
            [Offset("Search 48 03 5D ? 0F B6 83 ? ? ? ? Add 3 Read8")]
            internal static int ItemArrayStart;

            //0x78 int
            [Offset("Search 44 3B 65 ? 0F 82 ? ? ? ? 85 FF Add 3 Read8")]
            [OffsetDawntrail("Search 44 3B 65 ? 0F 82 ? ? ? ? 44 8B BC 24 ? ? ? ? Add 3 Read8")]
            internal static int ArrayCount;

            //0x90 byte
            [Offset("Search 66 3B 85 ? ? ? ? 0F 85 ? ? ? ? Add 3 Read32")]
            internal static int HandinType;

            //0x93 byte
            [Offset("Search 0F B6 85 ? ? ? ? 3A C2 Add 3 Read32")]
            internal static int ExpertFilter;

            //0x70 ptr to int[]
            [Offset("Search 49 8B 47 ? 48 8D 4D ? 48 8D 14 B8 Add 3 Read8")]
            [OffsetDawntrail("Search 49 8B 46 ? 8B D3 Add 3 Read8")]
            internal static int SortArray;
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;
        public IntPtr SupplyItemPtr => Core.Memory.Read<IntPtr>(Pointer + Offsets.ItemArrayStart);
        public int SupplyItemCount => Core.Memory.Read<int>(Pointer + Offsets.ArrayCount);

        public IntPtr SortArrayPtr => Core.Memory.Read<IntPtr>(Pointer + Offsets.SortArray);

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
            get => (GCSupplyType)Core.Memory.Read<byte>(Pointer + Offsets.HandinType);
            set => Core.Memory.Write(Pointer + Offsets.HandinType, (byte)value);
        }

        public GCFilter ExpertFilter
        {
            get => (GCFilter)Core.Memory.Read<byte>(Pointer + Offsets.ExpertFilter);
            set => Core.Memory.Write(Pointer + Offsets.ExpertFilter, (byte)value);
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