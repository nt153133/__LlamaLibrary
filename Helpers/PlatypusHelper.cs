using System;
using System.Linq;
using System.Windows.Media;
using ff14bot.Managers;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    public static class PlatypusHelper
    {
        public enum TaskType
        {
            None,
            CurrencyGcSealsTurnIn,
            InventoryOpenUnlock
        }

        private static readonly LLogger Log = new("PlatypusHelper", Colors.MediumPurple);

#nullable enable
        private static object? _platypusApi;
#nullable restore
        private static Action _showGui;
        private static Func<TaskType, bool> _canProfileExecuteTask;
        private static Func<Version> _version;
        private static Func<string> _versionString;

        static PlatypusHelper()
        {
            FindPlatypus();
        }

        public static bool HasPlatypus => BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Platypus") != null;
        public static Version Version => _version.Invoke();
        public static string VersionString => _versionString.Invoke();

        internal static void FindPlatypus()
        {
#pragma warning disable IDE0007 // Use implicit type
            var loader = BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Platypus");
#pragma warning restore IDE0007 // Use implicit type

            if (loader == null)
            {
                return;
            }

            var platypusObjectProperty = loader.GetType().GetProperty("Api");
            var platypusApi = platypusObjectProperty?.GetValue(loader);

            if (platypusApi == null)
            {
                return;
            }

            try
            {
                _showGui = (Action)Delegate.CreateDelegate(typeof(Action), platypusApi, "ShowGui");
                _canProfileExecuteTask = (Func<TaskType, bool>)Delegate.CreateDelegate(typeof(Func<TaskType, bool>), platypusApi, "CanProfileExecuteTask");
                _version = (Func<Version>)Delegate.CreateDelegate(typeof(Func<Version>), platypusApi, "Version");
                _versionString = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), platypusApi, "VersionString");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            Log.Information("Platypus found.");
        }

        public static void ShowGui()
        {
            _showGui.Invoke();
        }

        public static bool CanProfileExecuteTask(TaskType taskType)
        {
            return _canProfileExecuteTask.Invoke(taskType);
        }
    }
}