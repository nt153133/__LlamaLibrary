using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Window that is displayed when dealing with FC Aetherial Wheels.
    /// </summary>
    public class AetherialWheel : RemoteWindow<AetherialWheel>
    {
        public static readonly Dictionary<string, int> Properties = new()
        {
            {
                "MaxSlots",
                0
            }
        };

        public AetherialWheel() : base("AetherialWheel")
        {
        }

        public int MaxSlots => Elements[Properties["MaxSlots"]].Int;

        public void RemoveWheel(uint slotIndex)
        {
            SendAction(2, 3, 2, 4, slotIndex);
        }
    }
}