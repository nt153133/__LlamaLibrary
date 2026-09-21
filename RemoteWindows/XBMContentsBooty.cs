namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Battle-spoils collection window.</summary>
    public sealed class XBMContentsBooty : RemoteWindow<XBMContentsBooty>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMContentsBooty() : base("XBMContentsBooty")
        {
        }

        /// <summary>Requests all spoils; the caller must validate the resulting confirmation.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool CollectAll()
        {
            if (!IsOpen)
            {
                return false;
            }

            // Only the observed Global spoils layout has this collect-all callback.
            WindowSnapshot.RequireLength(Elements, 147, "XBMContentsBooty");
            SendAction(1, 3, 1);
            return true;
        }
    }
}