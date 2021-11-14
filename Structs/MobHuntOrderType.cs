using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    [StructLayout(LayoutKind.Explicit, Size = 0xB)]
    public struct MobHuntOrderType
    {
        [FieldOffset(0)]
        public uint QuestId;

        [FieldOffset(4)]
        public uint EventItem;

        [FieldOffset(8)]
        public short OrderStart;

        [FieldOffset(10)]
        public MobHuntType Type;

        [FieldOffset(11)]
        public byte Amount;

        public Item Item => DataManager.GetItem(EventItem);

        public override string ToString()
        {
            return $"{QuestId} Item: {DataManager.GetItem(EventItem).CurrentLocaleName} {OrderStart} {Type} {Amount}";
        }
    }
}