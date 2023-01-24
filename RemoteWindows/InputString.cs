using System;
using ff14bot;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class InputString : RemoteWindow<InputString>
    {
        private static class Offsets
        {
            [Offset("Search 48 8B 81 ? ? ? ? 48 85 C0 75 ? 48 8B 81 ? ? ? ? 48 85 C0 74 ? 48 8B 80 ? ? ? ? Add 3 Read32")]
            internal static int AtkComponentTextInput; //0x230

            [Offset("Search 48 8B 80 ? ? ? ? 80 38 ? 0F 95 C0 84 C0 75 ? 49 8B 00 Add 3 Read32")]
            internal static int StringPtr; //0xE0

            [Offset("Search 48 8D 1D ? ? ? ? BA ? ? ? ? 48 8D 4D ? E8 ? ? ? ? 4C 8D 45 ? Add 3 TraceRelative")]
            internal static IntPtr UnkStatic;
        }

        public IntPtr GetStringPtr => WindowByName == null ? IntPtr.Zero : Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(WindowByName.Pointer + Offsets.AtkComponentTextInput) + Offsets.StringPtr);

        public string GetString
        {
            get
            {
                var ptr = GetStringPtr;
                return ptr == IntPtr.Zero ? string.Empty : Core.Memory.ReadStringUTF8(GetStringPtr);
            }
        }

        public IntPtr GetStaticPtr => Offsets.UnkStatic;

        public InputString() : base("InputString")
        {
        }

        public void SetString(string input)
        {
            if (IsOpen)
            {
                UIInputHelper.SendInput(input);
            }
        }

        public void Confirm(string text)
        {
            UIInputHelper.SendInput(text);
            Confirm();
        }

        public void Confirm()
        {
            SendAction(3, 3, 0, 6, (ulong)GetStringPtr.ToInt64(), 6, (ulong)GetStaticPtr.ToInt64());
        }
    }
}