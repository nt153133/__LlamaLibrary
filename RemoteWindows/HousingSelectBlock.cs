using System;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class HousingSelectBlock : RemoteWindow<HousingSelectBlock>
    {
        public HousingSelectBlock() : base("HousingSelectBlock")
        {
        }

        public int NumberOfWards => Elements[4].TrimmedData;

        public int NumberOfPlots => Elements[35].TrimmedData;

        public string HousingWard => Core.Memory.ReadString((IntPtr)Elements[2].Data, Encoding.UTF8);

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