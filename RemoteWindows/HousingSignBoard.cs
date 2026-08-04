using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    public class HousingSignBoard : RemoteWindow<HousingSignBoard>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "IsForSaleStatus", 1 },
        };

        public HousingSignBoard() : base("HousingSignBoard")
        {
        }

        public bool IsForSale => Core.Memory.ReadString((IntPtr)Elements[Properties["IsForSaleStatus"]].Data, Encoding.UTF8).Contains("Sale");

        public void ClickBuy()
        {
            SendAction(1, 3, 1);
        }
    }
}