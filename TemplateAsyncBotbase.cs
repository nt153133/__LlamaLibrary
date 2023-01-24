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

namespace LlamaLibrary
{
    public abstract class TemplateAsyncBotbase : AsyncBotBase
    {
        protected abstract string BotBaseName { get; }

        protected virtual Type? SettingsForm { get; } = null;

        protected virtual Color LogColor { get; } = Colors.Aqua;

        protected abstract Task<bool> Run();

        private Composite? _root;

        // ReSharper disable once MemberCanBePrivate.Global
        protected LLogger Log;

        private Form? _settings;

        protected TemplateAsyncBotbase()
        {
            Log = new LLogger(BotBaseName, LogColor);
        }

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

        public override bool IsAutonomous => true;
        public override string Name => BotBaseName;
        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool RequiresProfile => false;
        public override Composite Root => _root!;
    }
}