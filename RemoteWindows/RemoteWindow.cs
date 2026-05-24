using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.RemoteWindows.Atk;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// A generic base class for remote windows that implement a singleton pattern via <see cref="Instance"/>.
    /// </summary>
    /// <typeparam name="T">The type of the derived remote window.</typeparam>
    public class RemoteWindow<T> : RemoteWindow
        where T : RemoteWindow<T>, new()
    {
        private static T _instance;

        /// <summary>
        /// Gets the singleton instance of the remote window.
        /// </summary>
        public static T Instance => _instance ??= new T();

        protected RemoteWindow(string windowName) : base(windowName)
        {
        }

        protected RemoteWindow(string windowName, AgentInterface agent) : base(windowName, agent)
        {
        }
    }

    /// <summary>
    /// An abstract base class for interacting with FFXIV remote UI windows (addons).
    /// Provides methods for opening, closing, and sending actions to the window.
    /// </summary>
    public abstract class RemoteWindow
    {
        //7.1
        /*
#if RB_CN
        private const int Offset0 = 0x1DA; //0F BF 93 ? ? ? ? 41 B1 ? 4C 8B 83 ? ? ? ? 48 8B CB C6 44 24 ? ? E8 ? ? ? ? 48 8B CB Add 3 Read32
        private const int Offset2 = 0x170; //4C 8B 83 ? ? ? ? 48 8B CB C6 44 24 ? ? E8 ? ? ? ? 48 8B CB Add 3 Read32
#else
*/
        private const int Offset0 = 0x1E2; //0F BF 93 ? ? ? ? 41 B1 ? 4C 8B 83 ? ? ? ? 48 8B CB C6 44 24 ? ? E8 ? ? ? ? 48 8B CB Add 3 Read32
        private const int Offset2 = 0x178; //4C 8B 83 ? ? ? ? 48 8B CB C6 44 24 ? ? E8 ? ? ? ? 48 8B CB Add 3 Read32


        /// <summary>
        /// Gets a value indicating whether the window is currently open and visible in the UI.
        /// </summary>
        public virtual bool IsOpen => WindowByName != null;

        /// <summary>
        /// Gets the internal name of the window (e.g., "RetainerList").
        /// </summary>
        public virtual string WindowName { get; private set; }

        /// <summary>
        /// Gets the <see cref="AgentInterface"/> associated with this window.
        /// </summary>
        public virtual AgentInterface Agent { get; private set; }

        /// <summary>
        /// Gets the <see cref="AtkAddonControl"/> for the window if it is open.
        /// </summary>
        public virtual AtkAddonControl? WindowByName => RaptureAtkUnitManager.GetWindowByName(WindowName);

        protected bool HasAgentInterfaceId => GetAgentInterfaceId() != 0;

        protected RemoteWindow(string windowName)
        {
            WindowName = windowName;
        }

        protected RemoteWindow(string windowName, AgentInterface agent)
        {
            Agent = agent;
            WindowName = windowName;
        }

        /// <summary>
        /// Closes the window by sending a standard close action.
        /// </summary>
        public virtual void Close()
        {
            if (IsOpen)
            {
                SendAction(1, 3uL, 0xFFFFFFFFuL);
            }
        }

        public int GetAgentInterfaceId()
        {
            if (WindowByName == null)
            {
                return 0;
            }

            var test = WindowByName.TryFindAgentInterface();

            return test == null ? 0 : test.Id;
        }

        public async Task<bool> WaitTillWindowOpen(int maxTimeOut = 5000)
        {
            await Coroutine.Wait(maxTimeOut, () => IsOpen);
            return IsOpen;
        }

        protected TwoInt[] Elements
        {
            get
            {
                if (WindowByName == null)
                {
                    return Array.Empty<TwoInt>();
                }

                var elementCount = ElementCount;
                var addr = Core.Memory.Read<IntPtr>(WindowByName.Pointer + Offset2);

                return Core.Memory.ReadArray<TwoInt>(addr, elementCount);
            }
        }

        protected ushort ElementCount => WindowByName != null ? Core.Memory.Read<ushort>(WindowByName.Pointer + Offset0) : (ushort)0;

        /// <summary>
        /// Sends a raw action to the window using the specified number of ulong pairs.
        /// </summary>
        /// <param name="pairCount">The number of parameter pairs being sent.</param>
        /// <param name="param">The parameters for the action.</param>
        public void SendAction(int pairCount, params ulong[] param)
        {
            if (WindowByName == null)
            {
                return;
            }

            if (IsOpen)
            {
                WindowByName.SendAction(pairCount, param);
            }
        }

        /// <summary>
        /// Attempts to open the window by toggling its associated <see cref="Agent"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the window was opened successfully; otherwise <see langword="false"/>.</returns>
        public virtual async Task<bool> Open()
        {
            if (IsOpen)
            {
                return true;
            }

            Agent.Toggle();
            return await WaitTillWindowOpen();

            //return SyncRoutines.WaitUntil(() => IsOpen, 50, 5000, true);
        }

        public void SendAction(bool updateState = true, params AtkValue[] parms)
        {
            if (WindowByName == null)
            {
                return;
            }

            if (IsOpen)
            {
                AtkClientFunctions.SendActionPtr(WindowByName.Pointer, updateState, parms);
            }
        }
    }
}