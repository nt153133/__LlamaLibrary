namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "FateProgress" window.
    /// Used to view progress for Shared FATEs across different regions.
    /// </summary>
    public class FateProgress : RemoteWindow<FateProgress>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FateProgress"/> class.
        /// </summary>
        public FateProgress() : base("FateProgress")
        {
        }
    }
}