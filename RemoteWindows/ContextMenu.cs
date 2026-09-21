namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Context menu commands not exposed by RB's selection-dialog wrappers.</summary>
    public sealed class ContextMenu : RemoteWindow<ContextMenu>
    {
        /// <summary>Creates a wrapper without opening the menu.</summary>
        public ContextMenu() : base("ContextMenu")
        {
        }

        /// <summary>First command label, or empty when absent; read on the bot thread or within a frame lock.</summary>
        public string FirstLabel => WindowSnapshot.Text(Elements, 8);

        /// <summary>Selects the first command only if its label still matches.</summary>
        /// <param name="expectedLabel">Exact localized label observed by the caller.</param>
        /// <returns>Whether a callback was submitted.</returns>
        public bool SelectFirstEntry(string expectedLabel)
        {
            return !string.IsNullOrEmpty(expectedLabel) && FirstLabel == expectedLabel && SelectFirstEntry();
        }

        /// <summary>Selects the first command when the caller has already established its context.</summary>
        /// <returns>Whether a callback was submitted.</returns>
        public bool SelectFirstEntry()
        {
            if (!IsOpen)
            {
                return false;
            }

            // Shared with FreeCompanyAction's existing context selection.
            // Item commands use the current game target as their recipient.
            SendAction(4, 3, 0, 3, 0, 4, 0, 0, 0);
            return true;
        }
    }
}