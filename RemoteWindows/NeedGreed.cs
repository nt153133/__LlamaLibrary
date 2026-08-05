using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    public class NeedGreed : RemoteWindow<NeedGreed>
    {
        public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
        {
            { "NumberOfItems", 3 },
            { "ItemIdsBase", 7 },
        };

        public NeedGreed() : base("NeedGreed")
        {
        }

        public int NumberOfItems => ElementCount < 4 ? 0 : Elements[Properties["NumberOfItems"]].Int;

        public uint[] ItemIds
        {
            get
            {
                var result = new uint[NumberOfItems];
                var j = 0;
                for (var i = Properties["ItemIdsBase"]; i < NumberOfItems; i += 7)
                {
                    result[j] = Elements[i].UInt;
                    j++;
                }

                return result;
            }
        }

        public void ClickItem(int index)
        {
            if (IsOpen && index < NumberOfItems)
            {
                SendAction(2, 3, 0, 4, (ulong)index);
            }
        }

        public void PassItem(int index)
        {
            if (IsOpen && index < NumberOfItems)
            {
                ClickItem(index);
                SendAction(4, 3, 2, 4, 0, 4, ItemIds[index], 3, 1);
            }
        }
    }
}