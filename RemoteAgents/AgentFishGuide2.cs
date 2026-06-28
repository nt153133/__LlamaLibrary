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
    /// Remote agent for the Fish Guide (fishing log) interface.
    /// Manages access to the list of caught fish and their associated metadata from game memory.
    /// </summary>
    public class AgentFishGuide2 : AgentInterface<AgentFishGuide2>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFishGuide2Offsets.Vtable;

        /// <summary>
        /// The total number of tabs available in the fish guide.
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
        /// Gets the memory pointer to the secondary info structure containing the fish list pointers.
        /// </summary>
        public IntPtr InfoPointer => Core.Memory.Read<IntPtr>(Pointer + AgentFishGuide2Offsets.InfoOffset);

        /// <summary>
        /// Gets the memory pointer to the beginning of the fish data pointer array.
        /// </summary>
        public IntPtr StartingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.StartingPointer);

        /// <summary>
        /// Gets the memory pointer to the end of the fish data pointer array.
        /// </summary>
        public IntPtr EndingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.EndingPointer);

        /// <summary>
        /// Retrieves the memory pointer for the start of the fish list, optionally offset by <paramref name="start"/>.
        /// </summary>
        /// <param name="start">The number of items to skip from the beginning of the list.</param>
        /// <returns>A memory pointer to the specified position in the fish data array.</returns>
        public IntPtr FishListPointer(int start = 0) => Core.Memory.Read<IntPtr>(StartingPointer + (8 * start));

        /// <summary>
        /// Gets the total number of fish entries currently available in the loaded fishing log.
        /// </summary>
        public int SlotCount => (int)((EndingPointer.ToInt64() - StartingPointer.ToInt64()) / 8);

        /// <summary>
        /// Reads the entire array of <see cref="FishGuide2Item"/> structures from game memory.
        /// </summary>
        /// <returns>An array of raw fish guide items.</returns>
        public FishGuide2Item[] GetFishListRaw()
        {
            return Core.Memory.ReadArray<FishGuide2Item>(Core.Memory.Read<IntPtr>(StartingPointer), SlotCount);
        }

        /// <summary>
        /// Reads a specific range of <see cref="FishGuide2Item"/> structures from game memory.
        /// </summary>
        /// <param name="start">The zero-based index to start reading from.</param>
        /// <param name="count">The number of items to read.</param>
        /// <returns>An array of raw fish guide items for the specified range.</returns>
        public FishGuide2Item[] GetFishListRaw(int start, int count)
        {
            return Core.Memory.ReadArray<FishGuide2Item>(FishListPointer(start), count);
        }

        /// <summary>
        /// Opens the Fish Guide window if necessary, navigates to the fishing tab, and retrieves the caught status of fish.
        /// </summary>
        /// <param name="start">The zero-based index to start reading from.</param>
        /// <param name="count">The number of items to read. If 0, reads all items from <paramref name="start"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning an array of fish guide items.</returns>
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
    /// Represents an individual item entry in the fish guide (fishing log).
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct FishGuide2Item
    {
        /// <summary>
        /// The numeric ID of the fish item from the Item EXD.
        /// </summary>
        [FieldOffset(0x0)]
        public uint FishItem;

        /// <summary>
        /// The display index or order of the fish in the guide.
        /// </summary>
        [FieldOffset(0X4)]
        public ushort Index;

        /// <summary>
        /// Unknown memory field.
        /// </summary>
        [FieldOffset(0x6)]
        public ushort Unknown;

        /// <summary>
        /// Unknown memory field.
        /// </summary>
        [FieldOffset(0x8)]
        public ushort Unknown2;

        /// <summary>
        /// Unknown memory field.
        /// </summary>
        [FieldOffset(0xA)]
        public ushort Unknown5;

        /// <summary>
        /// The raw byte indicating the caught status (1 if caught, 0 otherwise).
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