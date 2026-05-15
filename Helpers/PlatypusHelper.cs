using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.AClasses;
using ff14bot.Helpers;
using ff14bot.Managers;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    // ReSharper disable once UnusedType.Global
    /// <summary>
    /// Reflection-based bridge to the Platypus botbase plugin.
    /// Provides access to Platypus inventory automation, materia transmutation, QoL hooks, and loot helpers.
    /// </summary>
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
        /// <summary>Gets whether the Platypus botbase is currently loaded and available.</summary>
        public static bool HasPlatypus => PlatypusBotBase != null;

        /// <summary>Gets the current Platypus plugin version.</summary>
        public static Version Version => _version.Invoke();

        /// <summary>Gets the current Platypus plugin version as a formatted string.</summary>
        public static string VersionString => _versionString.Invoke();

        private static string PlatypusPath => Path.Combine(Utils.AssemblyDirectory, "BotBases", "Platypus");

        private static readonly BotBase? PlatypusBotBase = BotManager.Bots.FirstOrDefault(c => string.Equals(c.EnglishName, "Platypus", StringComparison.Ordinal));

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

        /// <summary>
        /// Downloads and installs the Platypus loader script if Platypus is not already installed.
        /// </summary>
        /// <returns><see langword="true"/> if Platypus is already installed or was installed successfully.</returns>
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

        /// <summary>Opens the Platypus settings/GUI window. Does nothing if Platypus is not loaded.</summary>
        public static void ShowGui()
        {
            if (HasPlatypus)
            {
                _showGui.Invoke();
            }
        }

        /// <summary>
        /// Entrusts the specified bag slots to the named retainers using Platypus's inventory entrust API.
        /// </summary>
        /// <param name="itemsToEntrust">List of bag slot/retainer name pairs to entrust.</param>
        /// <returns><see langword="true"/> if entrustment completed successfully.</returns>
        public static async Task<bool> InventoryEntrust(List<(BagSlot BagSlot, string RetainerName)> itemsToEntrust)
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return await _inventoryEntrust(itemsToEntrust);
        }

        /// <summary>
        /// Transmutes materia from the specified inventory slots via Platypus's materia transmutation API.
        /// </summary>
        /// <param name="materiasToTransmute">The bag slots containing materia to transmute.</param>
        /// <param name="maxTransmutes">Optional cap on the number of transmutations to perform.</param>
        /// <returns><see langword="true"/> if transmutation completed successfully.</returns>
        public static async Task<bool> InventoryMateriaTransmute(IEnumerable<BagSlot> materiasToTransmute, int? maxTransmutes = null)
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return await _inventoryMateriaTransmute(materiasToTransmute, maxTransmutes);
        }

        /// <summary>Runs all registered Platypus hook functions in sequence.</summary>
        /// <returns><see langword="true"/> if hooks ran successfully.</returns>
        public static async Task<bool> PerformPlatypusHooks()
        {
            if (!HasPlatypus)
            {
                return false;
            }

            return await _performPlatypusHooks();
        }

        /// <summary>Opens all treasure coffers in the current duty using Platypus's QoL routine.</summary>
        /// <returns><see langword="true"/> if coffers were opened successfully.</returns>
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

        /// <summary>Automatically loots all loot windows currently present using Platypus's auto-loot routine.</summary>
        /// <returns><see langword="true"/> if looting completed successfully.</returns>
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

        /// <summary>Waits until all pending loot windows have been cleared using Platypus's loot tracker.</summary>
        /// <returns><see langword="true"/> if all loot was cleared.</returns>
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