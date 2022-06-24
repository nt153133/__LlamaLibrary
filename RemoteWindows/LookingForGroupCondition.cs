using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class LookingForGroupCondition : RemoteWindow<LookingForGroupCondition>
    {
        private const string WindowName = "LookingForGroupCondition";

        private static class Offsets
        {
            [Offset("48 8B 8B ? ? ? ? E8 ? ? ? ? 49 8D 8E ? ? ? ? 8B 01 24 ? Add 3 Read32")]
            internal static int AtkComponentTextInputNodePtr;

            [Offset("48 8D 97 ? ? ? ? 48 8B 05 ? ? ? ? 33 F6 Add 3 Read32")]
            internal static int TextFieldPtr;
        }

        private static readonly Dictionary<string, int> Properties = new Dictionary<string, int>
        {
            {
                "Comment",
                185
            }
        };

        public LookingForGroupCondition() : base(WindowName)
        {
        }

        public IntPtr AtkComponentTextInputNode => Core.Memory.Read<IntPtr>(WindowByName.Pointer + Offsets.AtkComponentTextInputNodePtr);

        public IntPtr TextField => AtkComponentTextInputNode + Offsets.TextFieldPtr;

        public bool TextBool
        {
            get => Core.Memory.Read<byte>(AtkComponentTextInputNode + 0xC0) == 1;
            set => Core.Memory.Write(AtkComponentTextInputNode + 0xC0, value);
        }

        public string Comment => Core.Memory.ReadString((IntPtr)Elements[Properties["Comment"]].Data, Encoding.UTF8);

        public void UnselectClasses()
        {
            SendAction(2, 3, 0x21, 4, 1);
        }

        public void EnableSprout()
        {
            SendAction(3, 3, 0x12, 4, 0, 4, 1);
        }

        public void DisableSprout()
        {
            SendAction(3, 3, 0x13, 4, 0, 4, 1);
        }

        public void SetDutyNone()
        {
            SendAction(2, 3, 0xC, 4, 0);
        }

        public void Reset()
        {
            SendAction(1, 3, 0x1E);
        }

        public void Register()
        {
            SendAction(1, 3, 0x0);
        }
    }
}