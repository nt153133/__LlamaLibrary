namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "SatisfactionSupplyResult" window.
    /// Appears after completing a custom delivery, displaying rewards and satisfaction increases.
    /// </summary>
    public class SatisfactionSupplyResult : RemoteWindow<SatisfactionSupplyResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SatisfactionSupplyResult"/> class.
        /// </summary>
        public SatisfactionSupplyResult() : base("SatisfactionSupplyResult")
        {
        }

        /// <summary>
        /// Clicks the "Accept" or "Confirm" button to dismiss the result window and claim rewards.
        /// </summary>
        public void Confirm()
        {
            SendAction(1, 3, 1);
        }
    }
}