using System;
using System.Text;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides static utility methods for low-level UI text input manipulation.
    /// Interacts directly with the game's <c>AtkStage</c> and focuses on injecting or clearing text in UI components.
    /// </summary>
    public static class UIInputHelper
    {
        private static readonly LLogger Log = new(nameof(UIInputHelper), Colors.Pink);

        

        /// <summary>
        /// Gets the pointer to the underlying text input handler within the <c>AtkStage</c> structure.
        /// </summary>
        public static IntPtr GetInputTextPtr
        {
            get
            {
                var one = Core.Memory.Read<IntPtr>(Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + UIInputHelperOffsets.off1);
                var twoHalf = Core.Memory.Read<IntPtr>(two);
                var three = Core.Memory.Read<IntPtr>(twoHalf + UIInputHelperOffsets.off2);
                var four = Core.Memory.Read<IntPtr>(three + UIInputHelperOffsets.off3);
                return four;
            }
        }

        /// <summary>
        /// Gets or sets the pointer to the currently selected <c>AtkComponentTextInput</c> in the UI.
        /// </summary>
        public static IntPtr SelectedAtkComponentTextInputPtr
        {
            get
            {
                var one = Core.Memory.Read<IntPtr>(Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + UIInputHelperOffsets.off1);
                var twoHalf = Core.Memory.Read<IntPtr>(two);
                var three = Core.Memory.Read<IntPtr>(twoHalf + UIInputHelperOffsets.CurrentTextControl);
                return three;
            }
            set
            {
                var one = Core.Memory.Read<IntPtr>(Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + UIInputHelperOffsets.off1);
                var twoHalf = Core.Memory.Read<IntPtr>(two);
                Core.Memory.Write(twoHalf + UIInputHelperOffsets.CurrentTextControl, value);
            }
        }

        /// <summary>
        /// Calls the game's UTF-8 string constructor on the specified memory address.
        /// </summary>
        /// <param name="ptr">The address where the string object should be constructed.</param>
        public static void StringCtor(IntPtr ptr)
        {
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.Utf8StringCtor, ptr);
        }

        /// <summary>
        /// Calls the game's UTF-8 string constructor using a character sequence and length.
        /// </summary>
        /// <param name="ptr">The address where the string object should be constructed.</param>
        /// <param name="input">The source string.</param>
        /// <param name="length">The length of the sequence to copy.</param>
        public static void StringCtorFromSequence(IntPtr ptr, string input, uint length)
        {
            var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(input));

            using var allocatedMemory =
                Core.Memory.CreateAllocatedMemory(array.Length + 30);
            allocatedMemory.AllocateOfChunk("start", array.Length);
            allocatedMemory.WriteBytes("start", array);
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.Utf8StringFromSequenceCtor, ptr, allocatedMemory.Address, length);
        }

        /// <summary>
        /// Updates the content of an existing game UTF-8 string object.
        /// </summary>
        /// <param name="ptr">The pointer to the game's string object.</param>
        /// <param name="input">The new text content.</param>
        public static void SetString(IntPtr ptr, string input)
        {
            var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(input));

            using var allocatedMemory =
                Core.Memory.CreateAllocatedMemory(array.Length + 30);
            allocatedMemory.AllocateOfChunk("start", array.Length);
            allocatedMemory.WriteBytes("start", array);

            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.Utf8SetString, ptr, allocatedMemory.Address);
        }

        /// <summary>
        /// Programmatically injects text into the currently focused UI text field.
        /// </summary>
        /// <param name="input">The text to send.</param>
        public static void SendInput(string input)
        {
            using var seStringAlloc = Core.Memory.CreateAllocatedMemory(0x68);
            Log.Verbose($"Allocated memory at {seStringAlloc.Address}");
            StringCtor(seStringAlloc.Address);
            Log.Verbose($"Constructed string at {seStringAlloc.Address}");
            SetString(seStringAlloc.Address, input);
            Log.Verbose($"Set string at {seStringAlloc.Address}");
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.SendStringToFocus, GetInputTextPtr, seStringAlloc.Address, 0);
            Log.Verbose($"Sent string to focus at {GetInputTextPtr}");
        }

        /// <summary>
        /// Clears all text from the currently focused UI text field.
        /// </summary>
        public static void ClearInput()
        {
            using var seStringAlloc = Core.Memory.CreateAllocatedMemory(0x68);
            StringCtorFromSequence(seStringAlloc.Address, "\0", 0xFFFFFFFF);
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.SendStringToFocus, GetInputTextPtr, seStringAlloc.Address, 1);
        }
    }
}