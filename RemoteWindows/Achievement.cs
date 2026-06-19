namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "Achievement" window.
    /// Provides access to the player's achievement progress and rewards.
    /// </summary>
    public class Achievement : RemoteWindow<Achievement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Achievement"/> class.
        /// </summary>
        public Achievement() : base("Achievement")
        {
        }
    }
}