using System;
using System.Text;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers
{
    public static class UIInputHelper
    {
        private static readonly LLogger Log = new(nameof(UIInputHelper), Colors.Pink);

        private static class Offsets
        {
            [Offset("Search 48 8B 51 ? 4C 8B 32 Add 3 Read8")]
            internal static int off1; //0x28

            [Offset("Search 49 8B 46 ? 48 8D 95 ? ? ? ? 41 B0 ? Add 3 Read8")]
            internal static int off2; //0x18

            [Offset("Search 48 8B 48 ? 48 8B 01 FF 50 ? 48 8D 8D ? ? ? ? Add 3 Read8")]
            internal static int off3; //0x8

            [Offset("Search 48 89 5C 24 ? 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 41 0F B6 F8")]
            internal static IntPtr SendStringToFocus;

            [Offset("Search E8 ? ? ? ? 44 2B F7 TraceCall")]
            internal static IntPtr Utf8StringCtor;

            [Offset("Search E8 ? ? ? ? B0 ? 66 89 9F ? ? ? ? TraceCall")]
            internal static IntPtr Utf8SetString;

            [Offset("Search E8 ? ? ? ? 48 8B 43 ? 48 8D 54 24 ? 41 B0 ? 48 8B 48 ? 48 8B 01 FF 50 ? 48 8D 4C 24 ? E8 ? ? ? ? 48 8B 8C 24 ? ? ? ? 48 33 CC E8 ? ? ? ? 48 8B 9C 24 ? ? ? ? TraceCall")]
            internal static IntPtr Utf8StringFromSequenceCtor;

            [Offset("Search 48 8B 4B ? 48 8D 55 ? 66 89 75 ? 66 44 89 75 ? 4C 89 65 ? Add 3 Read8")]
            internal static int CurrentTextControl; //0x8
        }

        public static IntPtr GetInputTextPtr
        {
            get
            {
                var one = Core.Memory.Read<IntPtr>(LlamaLibrary.Memory.Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + Offsets.off1);
                var twoHalf = Core.Memory.Read<IntPtr>(two);
                var three = Core.Memory.Read<IntPtr>(twoHalf + Offsets.off2);
                var four = Core.Memory.Read<IntPtr>(three + Offsets.off3);
                return four;
            }
        }

        public static IntPtr SelectedAtkComponentTextInputPtr
        {
            get
            {
                var one = Core.Memory.Read<IntPtr>(LlamaLibrary.Memory.Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + Offsets.off1);
                var twoHalf = Core.Memory.Read<IntPtr>(two);
                var three = Core.Memory.Read<IntPtr>(twoHalf + Offsets.CurrentTextControl);
                return three;
            }
            set
            {
                var one = Core.Memory.Read<IntPtr>(LlamaLibrary.Memory.Offsets.AtkStage);
                var two = Core.Memory.Read<IntPtr>(one + Offsets.off1);
                var twoHalf = Core.Memory.Read<IntPtr>(two);
                Core.Memory.Write(twoHalf + Offsets.CurrentTextControl, value);
            }
        }

        public static void StringCtor(IntPtr ptr)
        {
            Core.Memory.CallInjected64<int>(Offsets.Utf8StringCtor, ptr);
        }

        public static void StringCtorFromSequence(IntPtr ptr, string input, uint length)
        {
            var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(input));

            using var allocatedMemory =
                Core.Memory.CreateAllocatedMemory(array.Length + 30);
            allocatedMemory.AllocateOfChunk("start", array.Length);
            allocatedMemory.WriteBytes("start", array);
            Core.Memory.CallInjected64<int>(Offsets.Utf8StringFromSequenceCtor, ptr, allocatedMemory.Address, length);
        }

        public static void SetString(IntPtr ptr, string input)
        {
            var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(input));

            using var allocatedMemory =
                Core.Memory.CreateAllocatedMemory(array.Length + 30);
            allocatedMemory.AllocateOfChunk("start", array.Length);
            allocatedMemory.WriteBytes("start", array);

            Core.Memory.CallInjected64<int>(Offsets.Utf8SetString, ptr, allocatedMemory.Address);
        }

        public static void SendInput(string input)
        {
            using var seStringAlloc = Core.Memory.CreateAllocatedMemory(0x68);
            Log.Verbose($"Allocated memory at {seStringAlloc.Address}");
            StringCtor(seStringAlloc.Address);
            Log.Verbose($"Constructed string at {seStringAlloc.Address}");
            SetString(seStringAlloc.Address, input);
            Log.Verbose($"Set string at {seStringAlloc.Address}");
            Core.Memory.CallInjected64<int>(Offsets.SendStringToFocus, GetInputTextPtr, seStringAlloc.Address, 0);
            Log.Verbose($"Sent string to focus at {GetInputTextPtr}");
        }

        public static void ClearInput()
        {
            using var seStringAlloc = Core.Memory.CreateAllocatedMemory(0x68);
            StringCtorFromSequence(seStringAlloc.Address, "\r", 0xFFFFFFFF);
            Core.Memory.CallInjected64<int>(Offsets.SendStringToFocus, GetInputTextPtr, seStringAlloc.Address, 1);
        }
    }
}