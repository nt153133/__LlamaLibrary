using System;
using System.Windows.Media;

namespace LlamaLibrary.Helpers
{
    [Obsolete("Use LlamaLibrary.Logging.LLogger instead.")]
    public static class LoggerOld
    {
        [Obsolete("Use LlamaLibrary.Logging.LLogger.WriteLog() instead.")]
        public static void External(string caller, string message, Color color)
        {
            ff14bot.Helpers.Logging.Write(color, $"[{caller}]" + message);
        }

        [Obsolete("Use LlamaLibrary.Logging.LLogger.Error() instead.")]
        public static void LogCritical(string text)
        {
            ff14bot.Helpers.Logging.Write(Colors.OrangeRed, text);
        }

        [Obsolete("Use LlamaLibrary.Logging.LLogger.Information() instead.")]
        public static void Info(string text)
        {
            ff14bot.Helpers.Logging.Write(Colors.Aqua, text);
        }
    }
}