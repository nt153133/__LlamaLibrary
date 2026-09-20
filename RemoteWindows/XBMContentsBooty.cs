using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Battle-spoils collection window.</summary>
    public sealed class XBMContentsBooty : CrucibleWindow<XBMContentsBooty>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMContentsBooty() : base("XBMContentsBooty")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(ReadVisibleValues());

        /// <summary>Requests all spoils; the caller must validate the resulting confirmation.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool CollectAll()
        {
            return Callback(3, 1);
        }

        /// <summary>Detached battle-spoils collection window data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 147, "XBMContentsBooty");
            }
        }
    }
}