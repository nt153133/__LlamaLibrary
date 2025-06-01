using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public struct DailyQuestRead
    {
        [FieldOffset(8)]
        public ushort IDRaw;

        [FieldOffset(0xA)]
        public ushort CompleteRaw;

        public bool IsComplete => CompleteRaw == 1;

        public int ID
        {
            get
            {
                if (IDRaw != 0)
                {
                    return IDRaw + 0x10000;
                }

                return 0;
            }
        }

        public bool Accepted => IDRaw != 0;
    }
}