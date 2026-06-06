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
    /// <summary>
    /// Remote agent for the Grand Company Supply and Expert Delivery interface.
    /// Manages the list of items available for turn-in and handles filtering and sorting.
    /// </summary>
    public class AgentGrandCompanySupply : AgentInterface<AgentGrandCompanySupply>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentGrandCompanySupplyOffsets.Vtable;

        /// <summary>
        /// Gets the memory pointer to the beginning of the supply item array.
        /// </summary>
        public IntPtr SupplyItemPtr => Core.Memory.Read<IntPtr>(Pointer + AgentGrandCompanySupplyOffsets.ItemArrayStart);

        /// <summary>
        /// Gets the total number of items in the supply list.
        /// </summary>
        public int SupplyItemCount => Core.Memory.Read<int>(Pointer + AgentGrandCompanySupplyOffsets.ArrayCount);

        /// <summary>
        /// Gets the memory pointer to the sorting array, which contains indices into the supply item array.
        /// </summary>
        public IntPtr SortArrayPtr => Core.Memory.Read<IntPtr>(Pointer + AgentGrandCompanySupplyOffsets.SortArray);

        /// <summary>
        /// Gets the array of indices used to sort and display supply items.
        /// </summary>
        public int[] SortArray => Core.Memory.ReadArray<int>(SortArrayPtr, SupplyItemCount);

        /// <summary>
        /// Gets the full array of <see cref="GCSupplyItem"/> entries from game memory.
        /// </summary>
        public GCSupplyItem[] SupplyItems => Core.Memory.ReadArray<GCSupplyItem>(SupplyItemPtr, SupplyItemCount);

        /// <summary>
        /// Gets the list of items filtered and sorted for the Expert Delivery interface.
        /// </summary>
        /// <remarks>
        /// This property skips the first 11 entries in the <see cref="SortArray"/> (which typically contain metadata)
        /// and applies the current <see cref="ExpertFilter"/> and <see cref="HandinType"/> to the items.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the current hand-in category (Supply, Provisioning, or Expert).
        /// </summary>
        public GCSupplyType HandinType
        {
            get => (GCSupplyType)Core.Memory.Read<byte>(Pointer + AgentGrandCompanySupplyOffsets.HandinType);
            set => Core.Memory.Write(Pointer + AgentGrandCompanySupplyOffsets.HandinType, (byte)value);
        }

        /// <summary>
        /// Gets or sets the active filter for Expert Delivery items.
        /// </summary>
        public GCFilter ExpertFilter
        {
            get => (GCFilter)Core.Memory.Read<byte>(Pointer + AgentGrandCompanySupplyOffsets.ExpertFilter);
            set => Core.Memory.Write(Pointer + AgentGrandCompanySupplyOffsets.ExpertFilter, (byte)value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentGrandCompanySupply"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentGrandCompanySupply(IntPtr pointer) : base(pointer)
        {
        }
    }

    /// <summary>
    /// Defines the categories of Grand Company turn-ins.
    /// </summary>
    public enum GCSupplyType : byte
    {
        /// <summary>Hand in crafted items for Disciple of the Hand jobs (Supply Missions).</summary>
        Supply = 0,
        /// <summary>Hand in gathered items for Disciple of the Land jobs (Provisioning Missions).</summary>
        Provisioning = 1,
        /// <summary>Hand in dungeon drops and gear for seals (Expert Delivery).</summary>
        Expert = 2,
    }

    /// <summary>
    /// Defines filtering options for the Grand Company Expert Delivery list.
    /// </summary>
    public enum GCFilter : byte
    {
        /// <summary>Show all eligible items.</summary>
        All = 0,
        /// <summary>Hide items that are part of a registered gearset.</summary>
        HideGearSet = 1,
        /// <summary>Hide items that are in the Armoury Chest.</summary>
        HideArmory = 2,
    }
}