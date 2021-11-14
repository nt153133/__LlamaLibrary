namespace LlamaLibrary.RemoteWindows
{
    public class NeedGreed : RemoteWindow<NeedGreed>
    {
        private const string WindowName = "NeedGreed";

        public NeedGreed() : base(WindowName)
        {
            _name = WindowName;
        }

        public int NumberOfItems => Elements()[3].TrimmedData;

        public uint[] ItemIds
        {
            get
            {
                var result = new uint[NumberOfItems];
                var j = 0;
                for (var i = 7; i < NumberOfItems; i += 7)
                {
                    result[j] = (uint)Elements()[i].TrimmedData;
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