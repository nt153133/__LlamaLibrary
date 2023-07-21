using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot.AClasses;
using ff14bot.Managers;
using GreyMagic;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    public static class PlatypusHelper
    {
        public enum TaskType : uint
        {
            None = 0,
            CurrencyGCSealsExchange = 1,
            InventoryOpenUse = 2,
            InventoryExpertDelivery = 3,
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

        public static bool HasPlatypus => PlatypusBotBase != null;
        public static Version Version => _version.Invoke();
        public static string VersionString => _versionString.Invoke();

        private static string PlatypusPath => Path.Combine(ff14bot.Helpers.Utils.AssemblyDirectory, "BotBases", "Platypus");

#nullable enable
        private static readonly BotBase? PlatypusBotBase = BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Platypus");
#nullable restore

        private static string PlatypusAssemblyFile => "Platypus.dll";

        private static string PlatypusLoaderFile => "PlatypusLoader.cs";

        private static string PlatypusLoaderUrl = "https://rbplatypus.com/downloads/loader/PlatypusLoader.txt";

        internal static void FindPlatypus()
        {
            if (PlatypusBotBase == null)
            {
                return;
            }

            var platypusObjectProperty = PlatypusBotBase.GetType().GetProperty("Api");
            var platypusApi = platypusObjectProperty?.GetValue(PlatypusBotBase);

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

            Log.Verbose("PlatypusHelper - Platypus found.");
        }

        public static bool InstallPlatypus()
        {
            if (File.Exists(Path.Combine(PlatypusPath, PlatypusAssemblyFile)))
            {
                // Platypus is already installed
                return true;
            }

            if (!Directory.Exists(PlatypusPath))
            {
                try
                {
                    Directory.CreateDirectory(PlatypusPath);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to install Platypus, could not create target directory {PlatypusPath}: {ex}");
                    return false;
                }
            }

            string platypusLoader;

            try
            {
                platypusLoader = DownloadPlatypusLoader().Result;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to download the Platypus loader to create a new installation: {ex}");
                return false;
            }

            try
            {
                File.WriteAllText(Path.Combine(PlatypusPath, PlatypusLoaderFile), platypusLoader);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to install Platypus, could not write loader inside {PlatypusPath}: {ex}");
                return false;
            }

            return true;
        }

        public static void ShowGui()
        {
            _showGui.Invoke();
        }

        public static bool CanProfileExecuteTask(TaskType taskType)
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return _canProfileExecuteTask.Invoke(taskType);
        }

        private static async Task<string> DownloadPlatypusLoader()
        {
            using (var client = new HttpClient())
            {
                return client.GetStringAsync(PlatypusLoaderUrl).Result;
            }
        }
    }
}