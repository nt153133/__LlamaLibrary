using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class LookingForGroupCondition : RemoteWindow<LookingForGroupCondition>
    {
        

        private static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            {
                "Comment",
                185
            }
        };

        public LookingForGroupCondition() : base("LookingForGroupCondition")
        {
        }

        public IntPtr AtkComponentTextInputNode => WindowByName == null ? IntPtr.Zero : Core.Memory.Read<IntPtr>(WindowByName.Pointer + LookingForGroupConditionOffsets.AtkComponentTextInputNodePtr);

        public IntPtr TextField => AtkComponentTextInputNode + LookingForGroupConditionOffsets.TextFieldPtr;

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

        public void SetIlvl(int ilvl)
        {
            SendAction(3, 3, 0xB, 4, (ulong)ilvl, 2, 1);
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