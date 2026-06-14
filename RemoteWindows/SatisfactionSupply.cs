namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "SatisfactionSupply" window (Custom Deliveries).
    /// Used to select and turn in items to custom delivery NPCs.
    /// </summary>
    public class SatisfactionSupply : RemoteWindow<SatisfactionSupply>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SatisfactionSupply"/> class.
        /// </summary>
        public SatisfactionSupply() : base("SatisfactionSupply")
        {
        }

        /// <summary>
        /// Selects a delivery category (e.g., DoH, DoL, or FSH) by its index in the window.
        /// </summary>
        /// <param name="index">The zero-based index of the deliverable item to select.</param>
        public void ClickItem(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }
    }
}