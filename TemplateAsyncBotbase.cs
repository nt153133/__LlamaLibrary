using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using LlamaLibrary.Logging;
using TreeSharp;
// ReSharper disable VirtualMemberCallInConstructor

namespace LlamaLibrary
{
    /// <summary>
    /// A template base class for creating an <see cref="AsyncBotBase"/> in RebornBuddy.
    /// An AsyncBotBase is a botbase that can be used instead of a behavior tree based BotBase,
    /// allowing the core logic to be written using standard async/await patterns instead of TreeSharp composites.
    /// </summary>
    public abstract class TemplateAsyncBotbase : AsyncBotBase
    {
        /// <summary>
        /// Gets the name of the botbase as it will appear in the RebornBuddy drop-down menu.
        /// </summary>
        protected abstract string BotBaseName { get; }

        /// <summary>
        /// Gets the type of the Windows Forms <see cref="Form"/> to be used for the botbase settings UI.
        /// If null, the botbase will not open a settings form when the Settings button is pressed.
        /// </summary>
        protected virtual Type? SettingsForm { get; } = null;

        /// <summary>
        /// Gets the color used for logging messages to the RebornBuddy console.
        /// </summary>
        protected virtual Color LogColor { get; } = Colors.Aqua;

        /// <summary>
        /// The main asynchronous execution loop of the botbase.
        /// </summary>
        /// <returns>A task representing the botbase's execution. Should return true if execution completed normally.</returns>
        protected abstract Task<bool> Run();

        private Composite? _root;

        // ReSharper disable once MemberCanBePrivate.Global
        protected LLogger Log;

        private Form? _settings;

        protected TemplateAsyncBotbase()
        {
            Log = new LLogger(BotBaseName, LogColor);
        }

        /// <summary>
        /// Async root to be ticked by RebornBuddy's engine.
        /// </summary>
        /// <returns>The execution task.</returns>
        public override Task AsyncRoot()
        {
            return Run();
        }

        public override void Start()
        {
            Log.Debug("Start");
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            _root = new ActionRunCoroutine(_ => Run());
        }

        public override void Stop()
        {
            Log.Debug("Stop");
            _root = null;
            (Navigator.NavigationProvider as IDisposable)?.Dispose();
            Navigator.NavigationProvider = null;
        }

        public override void OnButtonPress()
        {
            //Log.Debug("ButtonPress");
            if (SettingsForm == null)
            {
                Log.Error("No setting form type was set in the botbase");
                return;
            }

            if (_settings == null || _settings.IsDisposed)
            {
                _settings = Activator.CreateInstance(SettingsForm) as Form;
            }

            try
            {
                if (_settings == null)
                {
                    Log.Error($"Could not create settings form object {SettingsForm.Name}");
                    return;
                }

                _settings.Show();
                _settings.Activate();
            }
            catch (Exception ee)
            {
                Log.Error($"Exception: {ee.Message}");
            }
        }

        /// <summary>
        /// Return false if human presence is needed.
        /// </summary>
        public override bool IsAutonomous => true;
        
        /// <summary>
        /// Gets the name of the botbase.
        /// </summary>
        public override string Name => BotBaseName;
        
        /// <summary>
        /// Flags indicating what systems should be ticked by the RebornBuddy engine when this botbase is active.
        /// </summary>
        public override PulseFlags PulseFlags => PulseFlags.All;
        
        /// <summary>
        /// Indicates if this botbase requires a profile to be loaded to function.
        /// </summary>
        public override bool RequiresProfile => false;
        
        /// <summary>
        /// The root behavior tree composite.
        /// </summary>
        public override Composite Root => _root!;
    }
}