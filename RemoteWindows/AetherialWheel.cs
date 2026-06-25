using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Window that is displayed when dealing with FC Aetherial Wheels.
    /// </summary>
    public class AetherialWheel : RemoteWindow<AetherialWheel>
    {
        public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
        {
            {
                "MaxSlots",
                0
            }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AetherialWheel"/> class.
        /// </summary>
        public AetherialWheel() : base("AetherialWheel")
        {
        }

        /// <summary>
        /// Gets the maximum number of slots available in the current aetherial wheel stand.
        /// </summary>
        public int MaxSlots => Elements[Properties["MaxSlots"]].Int;

        /// <summary>
        /// Removes the wheel from the specified slot.
        /// </summary>
        /// <param name="slotIndex">The zero-based index of the slot to interact with.</param>
        public void RemoveWheel(uint slotIndex)
        {
            SendAction(2, 3, 2, 4, slotIndex);
        }
    }
}