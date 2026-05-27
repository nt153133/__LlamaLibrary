using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.LlamaManagers
{
    /// <summary>
    /// Static manager for the FFXIV "Snipe" mini-game.
    /// Handles reading mini-game state, identifying targetable objects, and executing shots via memory manipulation.
    /// </summary>
    public class SnipeManager
    {
        private static LLogger Log = new LLogger("SnipeManager", Colors.Silver);

        /// <summary>
        /// Gets the memory address of the SnipeManager instance in game memory.
        /// </summary>
        public static IntPtr addr => Core.Memory.Read<IntPtr>(SnipeManagerOffsets.Instance);

        /// <summary>
        /// Gets the row ID of the current snipe instance from the Snipe sheet.
        /// </summary>
        public static uint SnipeRowId => Core.Memory.Read<uint>(addr + SnipeManagerOffsets.Id);

        /// <summary>
        /// Gets or sets the current state of the snipe mini-game.
        /// State 4 typically indicates the system is ready to fire.
        /// </summary>
        public static byte State
        {
            get => Core.Memory.NoCacheRead<byte>(addr + SnipeManagerOffsets.State);
            set => Core.Memory.Write(addr + SnipeManagerOffsets.State, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the snipe mini-game is currently active.
        /// </summary>
        public static byte Active
        {
            get => Core.Memory.Read<byte>(addr + SnipeManagerOffsets.Active);
            set => Core.Memory.Write(addr + SnipeManagerOffsets.Active, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a shot is currently being processed.
        /// </summary>
        private static bool Shoot
        {
            get => Core.Memory.Read<byte>(addr + SnipeManagerOffsets.Shoot) == 1;
            set => Core.Memory.Write(addr + SnipeManagerOffsets.Shoot, value ? 1 : 0);
        }

        /// <summary>
        /// Gets a pointer to the array of <see cref="SnipeObject"/>s currently present in the mini-game.
        /// </summary>
        public static IntPtr SnipeObjectsPtr => Core.Memory.Read<IntPtr>(addr + SnipeManagerOffsets.SnipeObjects);

        /// <summary>
        /// Gets an array of <see cref="SnipeObject"/>s that can be targeted in the mini-game.
        /// </summary>
        public static SnipeObject[] SnipeObjects
        {
            get
            {
                if (addr == IntPtr.Zero)
                {
                    return Array.Empty<SnipeObject>();
                }

                var first = Core.Memory.Read<IntPtr>(addr + SnipeManagerOffsets.SnipeObjects);
                var end = Core.Memory.Read<IntPtr>(addr + SnipeManagerOffsets.SnipeObjects + 8);

                var count = (end.ToInt64() - first.ToInt64()) / 0x48;
                Log.Information($"{count} snipe objects found");
                return Core.Memory.ReadArray<SnipeObject>(first, (int)count);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mini-game is in state 4, which is ready for user interaction/firing.
        /// </summary>
        public static bool Ready => State == 4;

        /// <summary>
        /// Executes a snipe shot at the target object specified by <paramref name="index"/>.
        /// Writes the necessary shoot data to memory and advances the state to trigger the shot.
        /// </summary>
        /// <param name="index">The zero-based index of the target in the <see cref="SnipeObjects"/> array.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Snipe(byte index)
        {
            if (addr == IntPtr.Zero)
            {
                return;
            }

            var obj = SnipeObjects[index];

            byte shoot_1;
            if (obj.x18 != 0 && obj.x22 != 0)
            {
                shoot_1 = 0;
            }
            else if (obj.x34 != 0 && obj.x3e != 0)
            {
                shoot_1 = 1;
            }
            else
            {
                Log.Information($"Tried to snipe object but object is in an unexpected state. {obj}");
                return;
            }

            Log.Information($"Snipe {obj.GameObject?.Name ?? "Unknown Object"}");
            Core.Memory.Write(addr + SnipeManagerOffsets.ShootData, index); //0x5000 - this should be object Index (0x4c) (0x4e)
            //ox4c - should be the object index
            //ox4e - should be the array index (0 or 1)
            // these get set during state 4
            Core.Memory.Write(addr + SnipeManagerOffsets.ShootData + 1, shoot_1); //0x5001
            Core.Memory.Write(addr + SnipeManagerOffsets.ShootData + 2, shoot_1 == 0 ? obj.x22 : obj.x3e); //0x5002
            Core.Memory.Write<byte>(addr + SnipeManagerOffsets.ShootData + 4, 1); //0x5004
            State = (byte)(State + 1);

            await Coroutine.Sleep(500);
        }
    }

    /// <summary>
    /// Represents a targetable object within the FFXIV snipe mini-game.
    /// Maps to a 0x48 byte structure in game memory.
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Explicit, Size = 0x48)]
    public struct SnipeObject
    {
        [FieldOffset(0)]
        private IntPtr objPtr;

        // -0x10
        [FieldOffset(0x8)]
        internal byte x8;

        //Start of array 1
        [FieldOffset(0x18)]
        internal byte x18;

        [FieldOffset(0x22)]
        internal byte x22;

        //-0x10
        [FieldOffset(0x24)]
        internal byte x24;

        //start of array 2
        [FieldOffset(0x34)]
        internal byte x34;

        [FieldOffset(0x3e)]
        internal byte x3e;

        /// <summary>
        /// Gets a value indicating whether this object has been hit.
        /// </summary>
        [FieldOffset(0x44)]
        public byte Hit;

        /// <summary>
        /// Returns a string representation of the snipe object, including its name and hit status.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"{GameObject?.Name ?? "Null Object"} - Hit:{Hit:X} - {x8:X} - {x18:X}:{x22:X} or {x34:X}:{x3e:X}";
        }

        /// <summary>
        /// Gets the live <see cref="ff14bot.Objects.GameObject"/> associated with this snipe target.
        /// </summary>
        /// <value>The <see cref="GameObject"/> instance, or <see langword="null"/> if the pointer is invalid.</value>
        public GameObject? GameObject
        {
            get
            {
                var id = objPtr;
                return objPtr == IntPtr.Zero ? null : GameObjectManager.GameObjects.FirstOrDefault(i => i.Pointer == id);
            }
        }
    }
}
