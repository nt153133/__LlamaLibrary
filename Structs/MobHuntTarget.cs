using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MobHuntTarget
    {
        [FieldOffset(4)]
        public short BNpcName;

        [FieldOffset(6)]
        public short Fate;

        [FieldOffset(8)]
        public short Territory;

        public bool FateRequired => Fate > 0;

        public string Name => DataManager.GetBattleNPCData((uint)BNpcName).CurrentLocaleName;

        public override string ToString()
        {
            return $"{BNpcName} {Territory} {Fate}";
        }
    }
}