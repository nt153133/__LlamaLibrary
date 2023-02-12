using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Interfaces;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using TreeSharp;

namespace LlamaLibrary
{
    public abstract class TemplatePlugin : BotPlugin, IBotPlugin
    {
        public abstract string PluginName { get; }

        protected virtual Color LogColor { get; } = Colors.CornflowerBlue;

        protected virtual Type? SettingsForm { get; } = null;
        public override string Name => PluginName;
        protected LLogger Log = null!;

        private Form? _settings;

        private List<Func<Task<bool>>> _orderBotHooks = new List<Func<Task<bool>>>();
        private List<Func<Task>> _lisbethHooks = new List<Func<Task>>();
        private List<ActionRunCoroutine> _orderBotHooksCoroutines = new List<ActionRunCoroutine>();

        public override void OnInitialize()
        {
            Log = new LLogger(PluginName, LogColor);
            Log.Information("Initializing");
            _orderBotHooks = GetOrderBotHooks();
            _lisbethHooks = GetLisbethHooks();

            if (_orderBotHooks.Any())
            {
                foreach (var orderBotHook in _orderBotHooks)
                {
                    _orderBotHooksCoroutines.Add(new ActionRunCoroutine(r => orderBotHook()));
                }
            }
        }

        public override void OnEnabled()
        {
            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
            Log.Information($"{PluginName} Enabled");
        }

        public override void OnDisabled()
        {
            TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;
            Log.Information($"{PluginName} Disabled");
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

        private void AddHooks()
        {
            if (_orderBotHooksCoroutines.Any())
            {
                foreach (var orderBotHook in _orderBotHooksCoroutines)
                {
                    Log.Information($"Adding Orderbot {orderBotHook.Runner.Method.Name} Hook");

                    TreeHooks.Instance.AddHook("TreeStart", orderBotHook);
                }
            }

            if (!HasLisbeth())
            {
                return;
            }

            if (!_lisbethHooks.Any())
            {
                return;
            }

            foreach (var lisbethHook in _lisbethHooks)
            {
                var hooks = Lisbeth.GetHookList();
                var baseHook = lisbethHook.Method.Name;
                var craftCycleHook = baseHook + "_Craft";
                if (!hooks.Contains(baseHook))
                {
                    Log.Information($"Adding Lisbeth {lisbethHook.Method.Name} Hook");
                    Lisbeth.AddHook(baseHook, lisbethHook);
                }

                if (!hooks.Contains(craftCycleHook))
                {
                    Log.Information($"Adding {craftCycleHook} Hook");
                    Lisbeth.AddCraftCycleHook(craftCycleHook, lisbethHook);
                }
            }
        }

        private void RemoveHooks()
        {
            if (_orderBotHooksCoroutines.Any())
            {
                foreach (var orderBotHook in _orderBotHooksCoroutines)
                {
                    Log.Information($"Removing Orderbot {orderBotHook.Runner.Method.Name} Hook");

                    TreeHooks.Instance.RemoveHook("TreeStart", orderBotHook);
                }
            }

            if (!HasLisbeth())
            {
                return;
            }

            if (!_lisbethHooks.Any())
            {
                return;
            }

            foreach (var lisbethHook in _lisbethHooks)
            {
                var hooks = Lisbeth.GetHookList();
                var baseHook = lisbethHook.Method.Name;
                var craftCycleHook = baseHook + "_Craft";
                if (!hooks.Contains(baseHook))
                {
                    Log.Information($"Removing Lisbeth {lisbethHook.Method.Name} Hook");
                    Lisbeth.RemoveHook(baseHook);
                }

                if (!hooks.Contains(craftCycleHook))
                {
                    Log.Information($"Removing {craftCycleHook} Hook");
                    Lisbeth.RemoveCraftCycleHook(craftCycleHook);
                }
            }
        }

        protected virtual void OnBotStop(BotBase bot)
        {
            RemoveHooks();
        }

        protected virtual void OnBotStart(BotBase bot)
        {
            AddHooks();
        }

        private static bool HasLisbeth()
        {
            return BotManager.Bots.Any(c => c.EnglishName == "Lisbeth");
        }

        public virtual List<Func<Task<bool>>> GetOrderBotHooks()
        {
            return new();
        }

        public virtual List<Func<Task>> GetLisbethHooks()
        {
            return new();
        }
    }
}