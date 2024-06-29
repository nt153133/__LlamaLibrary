using System;
using System.Text;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class HousingSelectBlock : RemoteWindow<HousingSelectBlock>
    {
        private static class Offsets
        {
            [Offset("Search 89 86 ? ? ? ? 8B D0 Add 2 Read32")]
            [OffsetDawntrail("Search 89 87 ? ? ? ? 8B D0 48 39 B7 ? ? ? ? Add 2 Read32")]
            internal static int EligibilityArray;
        }

        public HousingSelectBlock() : base("HousingSelectBlock")
        {
        }

        public int NumberOfWards => Elements[4].TrimmedData;

        public int NumberOfPlots => Elements[35].TrimmedData;

        public string HousingWard => Core.Memory.ReadString((IntPtr)Elements[2].Data, Encoding.UTF8);

        public byte[]? EligibilityArray => WindowByName != null ? Core.Memory.ReadBytes(WindowByName.Pointer + Offsets.EligibilityArray, 4) : null;

        public string PlotPrice(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[38 + (plot * 7)].Data, Encoding.UTF8);
        }

        public string PlotString(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[37 + (plot * 7)].Data, Encoding.UTF8);
        }

        public string PlotString1(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[37 + (plot * 7)].Data, Encoding.Unicode);
        }

        public void SelectWard(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }

        public void GoToWard(int index)
        {
            SendAction(2, 3, 0, 3, (ulong)index);
        }
    }
}