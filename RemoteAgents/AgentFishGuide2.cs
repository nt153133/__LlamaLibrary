using System;
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
    /// Remote agent for the Fishing Guide (Fishing Log) interface.
    /// Provides access to the player's catch history and facilitates interaction with the fishing log UI.
    /// </summary>
    public class AgentFishGuide2 : AgentInterface<AgentFishGuide2>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFishGuide2Offsets.Vtable;

        /// <summary>
        /// The total number of tabs (categories) available in the fishing guide.
        /// </summary>
        public const int TabCount = 37;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentFishGuide2"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentFishGuide2(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the pointer to the internal information structure for the fishing guide.
        /// </summary>
        public IntPtr InfoPointer => Core.Memory.Read<IntPtr>(Pointer + AgentFishGuide2Offsets.InfoOffset);

        /// <summary>
        /// Gets the pointer to the start of the fish list array in memory.
        /// </summary>
        public IntPtr StartingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.StartingPointer);

        /// <summary>
        /// Gets the pointer to the end of the fish list array in memory.
        /// </summary>
        public IntPtr EndingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.EndingPointer);

        /// <summary>
        /// Retrieves the memory pointer for a specific fish entry in the list.
        /// </summary>
        /// <param name="start">The zero-based index of the fish entry.</param>
        /// <returns>The memory pointer to the fish entry.</returns>
        public IntPtr FishListPointer(int start = 0) => Core.Memory.Read<IntPtr>(StartingPointer + (8 * start));

        /// <summary>
        /// Gets the total number of fish slots available in the current log context.
        /// </summary>
        public int SlotCount => (int)((EndingPointer.ToInt64() - StartingPointer.ToInt64()) / 8);

        /// <summary>
        /// Reads all fish entries from the current fishing log context.
        /// </summary>
        /// <returns>An array of <see cref="FishGuide2Item"/> structures.</returns>
        public FishGuide2Item[] GetFishListRaw()
        {
            return Core.Memory.ReadArray<FishGuide2Item>(Core.Memory.Read<IntPtr>(StartingPointer), SlotCount); //.Select(x => x.FishItem) as List<uint>;
        }

        /// <summary>
        /// Reads a specific range of fish entries from the fishing log.
        /// </summary>
        /// <param name="start">The zero-based starting index.</param>
        /// <param name="count">The number of entries to read.</param>
        /// <returns>An array of <see cref="FishGuide2Item"/> structures.</returns>
        public FishGuide2Item[] GetFishListRaw(int start, int count)
        {
            return Core.Memory.ReadArray<FishGuide2Item>(FishListPointer(start), count); //.Select(x => x.FishItem) as List<uint>;
        }

        /// <summary>
        /// Asynchronously opens the fishing log UI, refreshes its state, and retrieves the specified fish entries.
        /// Closes the window when finished.
        /// </summary>
        /// <param name="start">The zero-based starting index to retrieve.</param>
        /// <param name="count">The number of entries to retrieve. If 0, retrieves all available entries.</param>
        /// <returns>A task representing the asynchronous operation, containing the array of fish entries.</returns>
        public async Task<FishGuide2Item[]> GetFishList(int start = 0, int count = 0)
        {
            if (!FishGuide2.Instance.IsOpen)
            {
                Toggle();
                if (!await Coroutine.Wait(10000, () => FishGuide2.Instance.IsOpen))
                {
                    return Array.Empty<FishGuide2Item>();
                }
            }

            if (count == 0)
            {
                count = SlotCount;
            }

            FishGuide2.Instance.SelectFishing();

            await Coroutine.Sleep(2000);

            var results = GetFishListRaw(start, count);

            FishGuide2.Instance.Close();
            await Coroutine.Wait(10000, () => !FishGuide2.Instance.IsOpen);

            return results;
        }
    }

    /// <summary>
    /// Represents a single fish entry within the Fishing Guide memory structure.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct FishGuide2Item
    {
        /// <summary>
        /// Gets the item ID of the fish.
        /// </summary>
        [FieldOffset(0x0)]
        public uint FishItem;

        /// <summary>
        /// Gets the zero-based index of the fish within the log.
        /// </summary>
        [FieldOffset(0X4)]
        public ushort Index;

        /// <summary>
        /// An unknown value at offset 0x6.
        /// </summary>
        [FieldOffset(0x6)]
        public ushort Unknown;

        /// <summary>
        /// An unknown value at offset 0x8.
        /// </summary>
        [FieldOffset(0x8)]
        public ushort Unknown2;

        /// <summary>
        /// An unknown value at offset 0xA.
        /// </summary>
        [FieldOffset(0xA)]
        public ushort Unknown5;

        /// <summary>
        /// The raw status byte indicating if the fish has been caught.
        /// </summary>
        [FieldOffset(0xE)]
        public byte bHasCaught;

        /// <summary>
        /// Gets a value indicating whether the fish has been caught by the player.
        /// </summary>
        public bool HasCaught => bHasCaught == 1;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DataManager.GetItem(FishItem)} : {HasCaught}";
        }
    }
}
