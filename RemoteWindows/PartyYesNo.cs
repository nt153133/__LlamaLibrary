using System;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    public class PartyYesNo : RemoteWindow<PartyYesNo>
    {
        public string NameLine => Core.Memory.ReadStringA((IntPtr)Elements[0].Data);

        public PartyYesNo() : base("SelectYesno")
        {
        }
    }
}