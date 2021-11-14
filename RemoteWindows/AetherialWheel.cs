using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    ///     Window that is displayed when dealing with FC Aetherial Wheels.
    /// </summary>
    public class AetherialWheel : RemoteWindow<AetherialWheel>
    {
        private const string WindowName = "AetherialWheel";

        public static readonly Dictionary<string, int> Properties = new Dictionary<string, int>
        {
            {
                "MaxSlots",
                0
            }
        };

        public AetherialWheel() : base(WindowName)
        {
            _name = WindowName;
        }

        public int MaxSlots => Elements[Properties["MaxSlots"]].TrimmedData;

        public void RemoveWheel(uint slotIndex)
        {
            SendAction(2, 3, 2, 4, slotIndex);
        }
    }
}