using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
#if RB_CN
    [StructLayout(LayoutKind.Explicit, Size = 0x4C)]
#else
    [StructLayout(LayoutKind.Explicit, Size = 0x60)]
#endif
    public struct DutyInfo
    {
        [FieldOffset(0x0)]
        public uint DutyId;

        [FieldOffset(0xB)]
        public byte TableKey;

        public string Name => DataManager.InstanceContentResults[DutyId].CurrentLocaleName;

        public override string ToString()
        {
            return $"{nameof(DutyId)}: {DutyId}, {nameof(TableKey)}: {TableKey} {Name}";
        }
    }
}