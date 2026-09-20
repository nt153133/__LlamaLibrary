using System;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Shared visibility and callback handling for Crucible windows.</summary>
    /// <typeparam name="T">Concrete window type.</typeparam>
    /// <remarks>Layouts and callbacks were verified on Global 2026.09.01. Call from the bot thread or within a frame lock.</remarks>
    public abstract class CrucibleWindow<T> : RemoteWindow<T> where T : CrucibleWindow<T>, new()
    {
        /// <summary>Associates the wrapper with its native addon.</summary>
        /// <param name="name">Native addon name.</param>
        protected CrucibleWindow(string name) : base(name)
        {
        }

        /// <summary>Whether the addon is currently visible; hidden cached addons are not open.</summary>
        public override bool IsOpen => WindowByName?.IsVisible == true;

        /// <summary>Reads the visible addon on the bot thread or within a frame lock.</summary>
        /// <returns>Copied ATK values, or an empty array when closed.</returns>
        protected TwoInt[] ReadVisibleValues()
        {
            var window = WindowByName;
            return window != null && window.IsVisible ? ReadElements(window) : Array.Empty<TwoInt>();
        }

        /// <summary>Sends a verified callback to the currently visible addon.</summary>
        /// <param name="arguments">Native ATK type/value pairs.</param>
        /// <returns>Whether a callback was submitted; the caller verifies completion.</returns>
        protected bool Callback(params ulong[] arguments)
        {
            var window = WindowByName;
            if (window == null || !window.IsVisible)
            {
                return false;
            }

            window.SendAction(arguments.Length / 2, arguments);
            return true;
        }

        /// <summary>Requests a standard close.</summary>
        /// <returns>Whether the callback was submitted.</returns>
        public bool TryClose() => Callback(3, uint.MaxValue);
    }
}