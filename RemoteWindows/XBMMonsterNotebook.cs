using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Provides access to the Master's Bestiary used to select Beastmaster familiars.</summary>
    /// <remarks>
    /// The addon identity comes from FFXIVClientStructs bb007e6f (Global 7.56).
    /// Reuse RemoteWindow's ATK access rather than copying native module layouts or agent IDs.
    /// Battlehorn assignment callbacks are not yet mapped; inherited SendAction is a raw API,
    /// not a verified assignment operation. See Docs/Beastmaster.md for the evidence still needed.
    /// </remarks>
    public class XBMMonsterNotebook : RemoteWindow<XBMMonsterNotebook>
    {
        /// <summary>Creates a wrapper that resolves the visible bestiary each time it is accessed.</summary>
        public XBMMonsterNotebook() : base("XBMMonsterNotebook")
        {
        }

        /// <summary>Opens the bestiary through the game's documented /bestiary command.</summary>
        /// <returns>True if the bestiary is already visible or becomes visible within the standard window timeout.</returns>
        /// <exception cref="InvalidOperationException">The window is closed and the caller is outside an RB coroutine.</exception>
        /// <remarks>
        /// Run on the bot coroutine, with UI operations serialized. The command toggles the window,
        /// so it is sent only when closed. No agent number is registered: those IDs vary by client.
        /// The client retains responsibility for job/unlock restrictions; rejection produces a timeout.
        /// </remarks>
        public override async Task<bool> Open()
        {
            if (IsOpen)
            {
                return true;
            }

            if (Coroutine.Current == null)
            {
                throw new InvalidOperationException("Opening the Master's Bestiary requires an RB coroutine.");
            }

            ChatManager.SendChat("/bestiary");
            return await WaitTillWindowOpen();
        }
    }
}