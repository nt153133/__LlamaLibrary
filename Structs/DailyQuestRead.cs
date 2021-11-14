using System.Runtime.InteropServices;

namespace LlamaLibrary.Helpers
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
                else
                {
                    return 0;
                }
            }
        }

        public bool Accepted => IDRaw != 0;
    }
}