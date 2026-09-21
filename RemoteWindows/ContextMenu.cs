namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Context menu commands not exposed by RB's selection-dialog wrappers.</summary>
    public sealed class ContextMenu : RemoteWindow<ContextMenu>
    {
        /// <summary>Creates a wrapper without opening the menu.</summary>
        public ContextMenu() : base("ContextMenu")
        {
        }

        /// <summary>Selects the first command in a context menu opened by the caller.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting state.</returns>
        /// <remarks>The caller must establish the source row and target. Localized labels do not identify commands.</remarks>
        public bool SelectFirstEntry()
        {
            if (!IsOpen)
            {
                return false;
            }

            // Same first-row callback used by FreeCompanyAction. For items,
            // the current game target determines the recipient.
            SendAction(4, 3, 0, 3, 0, 4, 0, 0, 0);
            return true;
        }
    }
}
