using System.IO;
using ff14bot;
using ff14bot.Helpers;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helper properties for resolving per-character and per-world settings directories
    /// compatible with the ff14bot <see cref="JsonSettings"/> system.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Gets the settings directory path unique to the current character and home world.
        /// Path format: <c>{SettingsPath}/{CharacterName}_World{HomeWorldId}</c>.
        /// Use this directory for settings that should be isolated per character.
        /// </summary>
        public static string UniqueCharacterSettingsDirectory => Path.Combine(JsonSettings.SettingsPath, $"{Core.Me.Name}_World{WorldHelper.HomeWorldId}");

        /// <summary>
        /// Gets the settings directory path shared across all characters on the current home world.
        /// Path format: <c>{SettingsPath}/World{HomeWorldId}</c>.
        /// Use this directory for settings that apply to every character on the same world.
        /// </summary>
        public static string HomeWorldSettingsDirectory => Path.Combine(JsonSettings.SettingsPath, $"World{WorldHelper.HomeWorldId}");

        /// <summary>
        /// Gets the settings directory path shared across all characters in the current data center.
        /// Path format: <c>{SettingsPath}/DataCenter{DataCenterId}</c>.
        /// Use this directory for settings that apply to all characters in the same data center region.
        /// </summary>
        public static string DataCenterSettingsDirectory => Path.Combine(JsonSettings.SettingsPath, $"DataCenter{WorldHelper.DataCenterId}");
    }
}