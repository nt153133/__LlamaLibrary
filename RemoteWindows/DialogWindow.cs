using System;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Reads standard confirmation and context-menu text without retaining native string pointers.</summary>
    public sealed class DialogWindow : RemoteWindow<DialogWindow>
    {
        /// <summary>Creates a wrapper for SelectYesno.</summary>
        public DialogWindow() : this("SelectYesno")
        {
        }

        /// <summary>Creates a wrapper for one of the standard selection dialogs.</summary>
        /// <param name="name">SelectYesno, SelectOk, ContextMenu, SelectString or SelectIconString.</param>
        public DialogWindow(string name) : base(name)
        {
            if (name != "SelectYesno" && name != "SelectOk" && name != "ContextMenu" &&
                name != "SelectString" && name != "SelectIconString")
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        /// <summary>Whether the dialog is visible, excluding cached hidden addons.</summary>
        public override bool IsOpen => WindowByName?.IsVisible == true;

        /// <summary>Copies the visible dialog's text on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot, empty when closed.</returns>
        public Snapshot ReadSnapshot()
        {
            var window = WindowByName;
            return new Snapshot(window != null && window.IsVisible ? ReadElements(window) : Array.Empty<TwoInt>());
        }

        /// <summary>Selects the first context command only if its label still matches.</summary>
        /// <param name="expectedLabel">Exact localized label observed by the caller.</param>
        /// <returns>Whether a callback was submitted.</returns>
        public bool SelectFirstContextEntry(string expectedLabel)
        {
            var window = WindowByName;
            if (string.IsNullOrEmpty(expectedLabel) || WindowName != "ContextMenu" || window == null || !window.IsVisible ||
                ReadSnapshot().FirstContextLabel != expectedLabel)
            {
                return false;
            }

            // Captured ContextMenu selection: Int command, Int row, Int64 zero,
            // Undefined zero. The current target determines item recipients.
            window.SendAction(4, 3, 0, 3, 0, 4, 0, 0, 0);
            return true;
        }

        /// <summary>Closes the currently visible dialog.</summary>
        /// <returns>Whether a close callback was submitted.</returns>
        public bool TryClose()
        {
            var window = WindowByName;
            if (window == null || !window.IsVisible)
            {
                return false;
            }

            Close();
            return true;
        }

        /// <summary>Copied confirmation or context-menu text.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                Prompt = Text(data, 0);
                FirstContextLabel = Text(data, 8);
            }

            /// <summary>Localized confirmation text.</summary>
            public string Prompt { get; }

            /// <summary>First context-menu label, or empty when absent.</summary>
            public string FirstContextLabel { get; }
        }
    }
}