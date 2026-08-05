using System;
using System.Text;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

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
        /// Gets the <c>TextServiceEvent</c> interface used to dispatch text input.
        /// </summary>
        public static IntPtr GetInputTextPtr
        {
            get
            {
                var raptureAtkModule = UiManagerProxy.RaptureAtkModule;
                return raptureAtkModule == IntPtr.Zero
                    ? IntPtr.Zero
                    : raptureAtkModule + UIInputHelperOffsets.AtkModuleTextServiceEvent;
            }
        }

        private static IntPtr AtkTextInput
        {
            get
            {
                var atkStage = Core.Memory.Read<IntPtr>(Offsets.AtkStage);
                if (atkStage == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                var atkInputManager = Core.Memory.Read<IntPtr>(atkStage + UIInputHelperOffsets.AtkStageAtkInputManager);
                return atkInputManager == IntPtr.Zero
                    ? IntPtr.Zero
                    : Core.Memory.Read<IntPtr>(atkInputManager);
            }
        }

        /// <summary>
        /// Gets or sets the <c>AtkTextInputEventInterface</c> targeted by the active text input.
        /// </summary>
        public static IntPtr SelectedAtkComponentTextInputPtr
        {
            get
            {
                var atkTextInput = AtkTextInput;
                return atkTextInput == IntPtr.Zero
                    ? IntPtr.Zero
                    : Core.Memory.Read<IntPtr>(atkTextInput + UIInputHelperOffsets.AtkTextInputTargetTextInputEventInterface);
            }
            set
            {
                var atkTextInput = AtkTextInput;
                if (atkTextInput != IntPtr.Zero)
                {
                    Core.Memory.Write(atkTextInput + UIInputHelperOffsets.AtkTextInputTargetTextInputEventInterface, value);
                }
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
