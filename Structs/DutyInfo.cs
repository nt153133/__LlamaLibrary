using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{

    [StructLayout(LayoutKind.Explicit, Size = 0x60)]

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