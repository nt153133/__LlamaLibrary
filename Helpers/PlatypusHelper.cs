using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.AClasses;
using ff14bot.Managers;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    // ReSharper disable once UnusedType.Global
    public static class PlatypusHelper
    {
        private static readonly LLogger Log = new("PlatypusHelper", Colors.MediumPurple);

        private static Action _showGui;

        private static Func<Version> _version;
        private static Func<string> _versionString;
        private static Func<List<(BagSlot BagSlot, string RetainerName)>, Task<bool>> _inventoryEntrust;
        private static Func<IEnumerable<BagSlot>, int?, Task<bool>> _inventoryMateriaTransmute;
        private static Func<Task<bool>> _performPlatypusHooks;
        private static Func<Task<bool>> _qolOpenTreasureCoffersInDuty;
        private static Func<Task<bool>> _qolAutoLoot;
        private static Func<Task<bool>> _qolWaitUntilAllLootIsGone;

        static PlatypusHelper()
        {
            FindPlatypus();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static bool HasPlatypus => PlatypusBotBase != null;
        public static Version Version => _version.Invoke();
        public static string VersionString => _versionString.Invoke();

        private static string PlatypusPath => Path.Combine(ff14bot.Helpers.Utils.AssemblyDirectory, "BotBases", "Platypus");

        private static readonly BotBase? PlatypusBotBase = BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Platypus");

        private static string PlatypusAssemblyFile => "Platypus.dll";
        private static string PlatypusLoaderFile => "PlatypusLoader.cs";

        private const string PlatypusLoaderUrl = "https://rbplatypus.com/downloads/loader/PlatypusLoader.txt";

        private static void FindPlatypus()
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
                _version = (Func<Version>)Delegate.CreateDelegate(typeof(Func<Version>), platypusApi, "Version");
                _versionString = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), platypusApi, "VersionString");
                _inventoryEntrust = (Func<List<(BagSlot BagSlot, string RetainerName)>, Task<bool>>)Delegate.CreateDelegate(typeof(Func<List<(BagSlot BagSlot, string RetainerName)>, Task<bool>>), platypusApi, "InventoryEntrust");
                _inventoryMateriaTransmute = (Func<IEnumerable<BagSlot>, int?, Task<bool>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<BagSlot>, int?, Task<bool>>), platypusApi, "InventoryMateriaTransmute");
                _performPlatypusHooks = (Func<Task<bool>>)Delegate.CreateDelegate(typeof(Func<Task<bool>>), platypusApi, "PerformPlatypusHooks");
                _qolOpenTreasureCoffersInDuty = (Func<Task<bool>>)Delegate.CreateDelegate(typeof(Func<Task<bool>>), platypusApi, "QolOpenTreasureCoffersInDuty");
                _qolAutoLoot = (Func<Task<bool>>)Delegate.CreateDelegate(typeof(Func<Task<bool>>), platypusApi, "QolAutoLoot");
                _qolWaitUntilAllLootIsGone = (Func<Task<bool>>)Delegate.CreateDelegate(typeof(Func<Task<bool>>), platypusApi, "QolWaitUntilAllLootIsGone");
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
            if (HasPlatypus)
            {
                _showGui.Invoke();
            }
        }

        public static async Task<bool> InventoryEntrust(List<(BagSlot BagSlot, string RetainerName)> itemsToEntrust)
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return await _inventoryEntrust(itemsToEntrust);
        }

        public static async Task<bool> InventoryMateriaTransmute(IEnumerable<BagSlot> materiasToTransmute, int? maxTransmutes = null)
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return await _inventoryMateriaTransmute(materiasToTransmute, maxTransmutes);
        }

        public static async Task<bool> PerformPlatypusHooks()
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return await _performPlatypusHooks();
        }

        public static async Task<bool> QolOpenTreasureCoffersInDuty()
        {
            if (!HasPlatypus)
            {
                return false;
            }

            await Coroutine.Wait(30000, () => !QuestLogManager.InCutscene);
            await Coroutine.Wait(5000, () => !MovementManager.IsOccupied);

            return await _qolOpenTreasureCoffersInDuty();
        }

        public static async Task<bool> QolAutoLoot()
        {
            if (!HasPlatypus)
            {
                return false;
            }

            await Coroutine.Wait(30000, () => !QuestLogManager.InCutscene);
            await Coroutine.Wait(5000, () => !MovementManager.IsOccupied);

            return await _qolAutoLoot();
        }

        public static async Task<bool> QolWaitUntilAllLootIsGone()
        {
            if (!HasPlatypus)
            {
                return false;
            }

            await Coroutine.Wait(30000, () => !QuestLogManager.InCutscene);
            await Coroutine.Wait(5000, () => !MovementManager.IsOccupied);

            return await _qolWaitUntilAllLootIsGone();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Task<string> DownloadPlatypusLoader()
        {
            using var client = new HttpClient();
            return client.GetStringAsync(PlatypusLoaderUrl).Result;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}