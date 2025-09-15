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
    public static class UIInputHelper
    {
        private static readonly LLogger Log = new(nameof(UIInputHelper), Colors.Pink);

        

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

        public static void StringCtor(IntPtr ptr)
        {
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.Utf8StringCtor, ptr);
        }

        public static void StringCtorFromSequence(IntPtr ptr, string input, uint length)
        {
            var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(input));

            using var allocatedMemory =
                Core.Memory.CreateAllocatedMemory(array.Length + 30);
            allocatedMemory.AllocateOfChunk("start", array.Length);
            allocatedMemory.WriteBytes("start", array);
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.Utf8StringFromSequenceCtor, ptr, allocatedMemory.Address, length);
        }

        public static void SetString(IntPtr ptr, string input)
        {
            var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(input));

            using var allocatedMemory =
                Core.Memory.CreateAllocatedMemory(array.Length + 30);
            allocatedMemory.AllocateOfChunk("start", array.Length);
            allocatedMemory.WriteBytes("start", array);

            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.Utf8SetString, ptr, allocatedMemory.Address);
        }

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

        public static void ClearInput()
        {
            using var seStringAlloc = Core.Memory.CreateAllocatedMemory(0x68);
            StringCtorFromSequence(seStringAlloc.Address, "\0", 0xFFFFFFFF);
            Core.Memory.CallInjectedWraper<int>(UIInputHelperOffsets.SendStringToFocus, GetInputTextPtr, seStringAlloc.Address, 1);
        }
    }
}