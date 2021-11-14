using System;
using System.Windows.Media;

namespace LlamaLibrary.Helpers
{
    public class LLogger
    {
        private readonly string name;
        private readonly Color color;
        private readonly LogLevel logLevel;

        public LLogger(string name, Color color, LogLevel logLevel = LogLevel.Information)
        {
            this.name = name;
            this.color = color;
            this.logLevel = logLevel;
        }

        public void Log(IntPtr pointer)
        {
            Information(pointer.ToString("X"));
        }

        public void Verbose(string text)
        {
            if (logLevel >= LogLevel.Verbose)
            {
                Log(text, color);
            }
        }

        public void Information(string text)
        {
            if (logLevel >= LogLevel.Information)
            {
                Log(text, color);
            }
        }

        public void Error(string text)
        {
            Log(text, Colors.OrangeRed);
        }

        public void Log(string text, Color logColor)
        {
            ff14bot.Helpers.Logging.Write(logColor, $"[{name}] {text}");
        }
    }

    public enum LogLevel
    {
        Verbose,
        Information,
        Error
    }
}