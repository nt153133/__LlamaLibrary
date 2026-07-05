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
    /// Remote agent for the fishing guide (log) interface.
    /// Provides access to the player's fishing history, including which fish have been caught.
    /// </summary>
    public class AgentFishGuide2 : AgentInterface<AgentFishGuide2>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFishGuide2Offsets.Vtable;

        /// <summary>
        /// The total number of tabs in the fishing guide.
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
        /// Gets the memory pointer to the fishing guide's information block.
        /// </summary>
        public IntPtr InfoPointer => Core.Memory.Read<IntPtr>(Pointer + AgentFishGuide2Offsets.InfoOffset);

        /// <summary>
        /// Gets the memory pointer to the beginning of the fish list array.
        /// </summary>
        public IntPtr StartingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.StartingPointer);

        /// <summary>
        /// Gets the memory pointer to the end of the fish list array.
        /// </summary>
        public IntPtr EndingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.EndingPointer);

        /// <summary>
        /// Gets the memory pointer to a specific position in the fish list array.
        /// </summary>
        /// <param name="start">The zero-based index to start from.</param>
        /// <returns>An <see cref="IntPtr"/> to the specified fish list entry.</returns>
        public IntPtr FishListPointer(int start = 0) => Core.Memory.Read<IntPtr>(StartingPointer + (8 * start));

        /// <summary>
        /// Gets the total number of fish entries (slots) available in the current fishing guide view.
        /// </summary>
        public int SlotCount => (int)((EndingPointer.ToInt64() - StartingPointer.ToInt64()) / 8);

        /// <summary>
        /// Retrieves the entire fish list from game memory.
        /// </summary>
        /// <returns>An array of <see cref="FishGuide2Item"/> structures.</returns>
        public FishGuide2Item[] GetFishListRaw()
        {
            return Core.Memory.ReadArray<FishGuide2Item>(Core.Memory.Read<IntPtr>(StartingPointer), SlotCount); //.Select(x => x.FishItem) as List<uint>;
        }

        /// <summary>
        /// Retrieves a portion of the fish list from game memory.
        /// </summary>
        /// <param name="start">The index of the first item to retrieve.</param>
        /// <param name="count">The number of items to retrieve.</param>
        /// <returns>An array of <see cref="FishGuide2Item"/> structures.</returns>
        public FishGuide2Item[] GetFishListRaw(int start, int count)
        {
            return Core.Memory.ReadArray<FishGuide2Item>(FishListPointer(start), count); //.Select(x => x.FishItem) as List<uint>;
        }

        /// <summary>
        /// Opens the fishing guide UI and retrieves the fish list.
        /// Handles UI navigation to ensure the correct tab and mode are selected.
        /// </summary>
        /// <param name="start">The index of the first item to retrieve.</param>
        /// <param name="count">The number of items to retrieve. If 0, retrieves all items up to <see cref="SlotCount"/>.</param>
        /// <returns>An array of <see cref="FishGuide2Item"/> structures, or an empty array if the UI could not be opened.</returns>
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
    /// Represents an entry in the fishing guide.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct FishGuide2Item
    {
        /// <summary>
        /// The item ID of the fish.
        /// </summary>
        [FieldOffset(0x0)]
        public uint FishItem;

        /// <summary>
        /// The internal index of the fish in the guide.
        /// </summary>
        [FieldOffset(0X4)]
        public ushort Index;

        /// <summary>
        /// Unknown field.
        /// </summary>
        [FieldOffset(0x6)]
        public ushort Unknown;

        /// <summary>
        /// Unknown field.
        /// </summary>
        [FieldOffset(0x8)]
        public ushort Unknown2;

        /// <summary>
        /// Unknown field.
        /// </summary>
        [FieldOffset(0xA)]
        public ushort Unknown5;

        /// <summary>
        /// Raw byte indicating whether the player has caught this fish (1 if caught, 0 otherwise).
        /// </summary>
        [FieldOffset(0xE)]
        public byte bHasCaught;

        /// <summary>
        /// Gets a value indicating whether the player has caught this fish.
        /// </summary>
        public bool HasCaught => bHasCaught == 1;

        /// <summary>
        /// Returns a string representation of the fish entry.
        /// </summary>
        /// <returns>A string containing the fish name and its caught status.</returns>
        public override string ToString()
        {
            return $"{DataManager.GetItem(FishItem)} : {HasCaught}";
        }
    }
}