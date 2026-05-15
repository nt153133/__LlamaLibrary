using System;
using System.Windows.Media;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Legacy logging helpers. Use <see cref="LlamaLibrary.Logging.LLogger"/> for all new code.
    /// </summary>
    [Obsolete("Use LlamaLibrary.Logging.LLogger instead.")]
    public static class LoggerOld
    {
        /// <summary>Writes a colored message to the RebornBuddy log prefixed with the caller name.</summary>
        /// <param name="caller">The name of the calling class or module.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The color to use in the log output.</param>
        [Obsolete("Use LlamaLibrary.Logging.LLogger.WriteLog() instead.")]
        public static void External(string caller, string message, Color color)
        {
            ff14bot.Helpers.Logging.Write(color, $"[{caller}]" + message);
        }

        /// <summary>Writes a critical-error message in orange-red to the RebornBuddy log.</summary>
        /// <param name="text">The error message to log.</param>
        [Obsolete("Use LlamaLibrary.Logging.LLogger.Error() instead.")]
        public static void LogCritical(string text)
        {
            ff14bot.Helpers.Logging.Write(Colors.OrangeRed, text);
        }

        /// <summary>Writes an informational message in aqua to the RebornBuddy log.</summary>
        /// <param name="text">The informational message to log.</param>
        [Obsolete("Use LlamaLibrary.Logging.LLogger.Information() instead.")]
        public static void Info(string text)
        {
            ff14bot.Helpers.Logging.Write(Colors.Aqua, text);
        }
    }
}