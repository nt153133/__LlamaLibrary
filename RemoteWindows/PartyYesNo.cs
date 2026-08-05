using System;
using System.Collections.Generic;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    public class PartyYesNo : RemoteWindow<PartyYesNo>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "NameLine", 0 },
        };

        public string NameLine => Core.Memory.ReadStringA((IntPtr)Elements[Properties["NameLine"]].Data);

        public PartyYesNo() : base("SelectYesno")
        {
        }
    }
}