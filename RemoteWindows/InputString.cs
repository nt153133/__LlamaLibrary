using System;
using ff14bot;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class InputString : RemoteWindow<InputString>
    {
        

        public IntPtr GetStringPtr => WindowByName == null ? IntPtr.Zero : Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(WindowByName.Pointer + InputStringOffsets.AtkComponentTextInput) + InputStringOffsets.StringPtr);

        public string GetString
        {
            get
            {
                var ptr = GetStringPtr;
                return ptr == IntPtr.Zero ? string.Empty : Core.Memory.ReadStringUTF8(GetStringPtr);
            }
        }

        public IntPtr GetStaticPtr => InputStringOffsets.UnkStatic;

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