namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "FishGuide2" window (Fishing Log).
    /// Used to navigate the fishing log and select between fishing and spearfishing categories.
    /// </summary>
    public class FishGuide2 : RemoteWindow<FishGuide2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FishGuide2"/> class.
        /// </summary>
        public FishGuide2() : base("FishGuide2")
        {
        }

        /// <summary>
        /// Selects a specific regional or category tab in the fishing log.
        /// </summary>
        /// <param name="index">The zero-based index of the tab to click.</param>
        public void ClickTab(int index)
        {
            SendAction(2, 3, 9, 3, (ulong)index);
        }

        /// <summary>
        /// Switches the fishing log view to standard fishing.
        /// </summary>
        public void SelectFishing()
        {
            SendAction(1, 3, 3);
        }

        /// <summary>
        /// Switches the fishing log view to spearfishing.
        /// </summary>
        public void SelectSpearFishing()
        {
            SendAction(1, 3, 4);
        }
    }
}