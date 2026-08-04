using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class HousingSelectBlock : RemoteWindow<HousingSelectBlock>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "NumberOfWards", 4 },
            { "NumberOfPlots", 35 },
            { "HousingWardName", 2 },
            { "PlotPriceBase", 38 },
            { "PlotNameBase", 37 },
        };

        public HousingSelectBlock() : base("HousingSelectBlock")
        {
        }

        public int NumberOfWards => Elements[Properties["NumberOfWards"]].Int;

        public int NumberOfPlots => Elements[Properties["NumberOfPlots"]].Int;

        public string HousingWard => Core.Memory.ReadString((IntPtr)Elements[Properties["HousingWardName"]].Data, Encoding.UTF8);

        public byte[]? EligibilityArray => WindowByName != null ? Core.Memory.ReadBytes(WindowByName.Pointer + HousingSelectBlockOffsets.EligibilityArray, 4) : null;

        public string PlotPrice(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[Properties["PlotPriceBase"] + (plot * 7)].Data, Encoding.UTF8);
        }

        public string PlotString(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[Properties["PlotNameBase"] + (plot * 7)].Data, Encoding.UTF8);
        }

        public string PlotString1(int plot)
        {
            return Core.Memory.ReadString((IntPtr)Elements[Properties["PlotNameBase"] + (plot * 7)].Data, Encoding.Unicode);
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