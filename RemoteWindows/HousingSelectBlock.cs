using System;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class HousingSelectBlock : RemoteWindow<HousingSelectBlock>
    {
        private const string WindowName = "HousingSelectBlock";

        public HousingSelectBlock() : base(WindowName)
        {
            _name = WindowName;
        }

        public int NumberOfWards => Elements[3].TrimmedData;

        public int NumberOfPlots => Elements[34].TrimmedData;

        public string HousingWard => Core.Memory.ReadString((IntPtr)Elements[2].Data, Encoding.UTF8);

        public string PlotPrice(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[37 + (plot * 7)].Data, Encoding.UTF8);
        }

        public string PlotString(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[36 + (plot * 7)].Data, Encoding.UTF8);
        }

        public string PlotString1(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[36 + (plot * 7)].Data, Encoding.Unicode);
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