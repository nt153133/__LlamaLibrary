using System.Windows.Media;

namespace LlamaLibrary.Helpers
{
    public static class Logger
    {
        public static void External(string caller, string message, Color color)
        {
            ff14bot.Helpers.Logging.Write(color, $"[{caller}]" + message);
        }

        public static void LogCritical(string text)
        {
            ff14bot.Helpers.Logging.Write(Colors.OrangeRed, text);
        }

        public static void Info(string text)
        {
            ff14bot.Helpers.Logging.Write(Colors.Aqua, text);
        }
    }
}