using System;
using System.Diagnostics;
using System.Windows.Media;

namespace LlamaLibrary.Logging
{
    /// <summary>
    /// Custom logger that writes to bot logs + console and general terminal.
    /// </summary>
    public class LLogger
    {
        /// <summary>
        /// Gets or sets <see cref="System.Windows.Media.Color"/> of log lines displayed in bot console.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="Logging.LogLevel"/> for log filtering. Logs will include
        /// current level and above (e.g. Information -> Information through Critical).
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Display name for this log category.  Appears at the start of each log line.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="LLogger"/> class.
        /// </summary>
        /// <param name="color">Log line <see cref="System.Windows.Media.Color"/>.</param>
        /// <param name="name">Display name for this logging category.</param>
        /// <param name="logLevel"><see cref="LogLevel"/> for this logging category.</param>
        public LLogger(string name, Color color, LogLevel logLevel = LogLevel.Information)
        {
            if (logLevel == LogLevel.Information)
            {
                logLevel = RbCurrentLogLevel;
            }
            _name = name;
            Color = color;
            LogLevel = logLevel;
        }

        public LLogger(string name, LogLevel logLevel = LogLevel.Information)
        {
            if (logLevel == LogLevel.Information)
            {
                logLevel = RbCurrentLogLevel;
            }
            _name = name;
            Color = Colors.Gainsboro;
            LogLevel = logLevel;
        }

        /// <summary>
        /// Checks if <see cref="LogLevel"/> will print from this <see cref="LLogger"/>.
        /// </summary>
        /// <param name="logLevel"><see cref="LogLevel"/> to evaluate.</param>
        /// <returns><see langword="true"/> if enabled.</returns>
        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel;
        }

        /// <summary>
        /// Writes a message to log with the indicated color, regardless of <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="color">Log line <see cref="System.Windows.Media.Color"/>.</param>
        /// <param name="message">Text to write to log.</param>
        public virtual void WriteLog(Color color, string message, ff14bot.Helpers.LogLevel rbLogLevel = ff14bot.Helpers.LogLevel.Normal)
        {
            var logLine = $"[{_name}] {message}";
            ff14bot.Helpers.Logging.Write(rbLogLevel, color, logLine); //ff14bot.Helpers.LogLevel.Normal
            Console.WriteLine(logLine); // Needed to appear in debugger, tests, etc
        }

        /// <summary>
        /// Writes a message to log, filtered by <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logLevel">Severity of this message.</param>
        /// <param name="color">Log line <see cref="System.Windows.Media.Color"/>.</param>
        /// <param name="message">Text to write to log.</param>
        public virtual void WriteFilteredLog(LogLevel logLevel, Color color, string message)
        {
            if (IsEnabled(logLevel))
            {
                switch (logLevel)
                {
                    case LogLevel.Verbose:
                        WriteLog(color, message, ff14bot.Helpers.LogLevel.Verbose);
                        break;
                    case LogLevel.Debug:
                        WriteLog(color, message, ff14bot.Helpers.LogLevel.Diagnostic);
                        break;
                    case LogLevel.Information:
                        WriteLog(color, message, ff14bot.Helpers.LogLevel.Normal);
                        break;
                    case LogLevel.Warning:
                    case LogLevel.Error:
                        WriteLog(color, message, ff14bot.Helpers.LogLevel.Quiet);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Verbose"/> message to log.
        /// </summary>
        /// <param name="message">Text to write to log.</param>
        public virtual void Verbose(string message)
        {
            WriteFilteredLog(LogLevel.Verbose, Color, message);
        }

        /// <summary>
        /// Writes a <see cref="System.Diagnostics.Debug"/> message to log.
        /// </summary>
        /// <param name="message">Text to write to log.</param>
        public virtual void Debug(string message)
        {
            WriteFilteredLog(LogLevel.Debug, Color, message);
        }

        /// <summary>
        /// Writes an <see cref="LogLevel.Information"/> message to log.
        /// </summary>
        /// <param name="message">Text to write to log.</param>
        public virtual void Information(string message)
        {
            WriteFilteredLog(LogLevel.Information, Color, message);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Warning"/> message to log.
        /// </summary>
        /// <param name="message">Text to write to log.</param>
        public virtual void Warning(string message)
        {
            WriteFilteredLog(LogLevel.Warning, Colors.Goldenrod, message);
        }

        /// <summary>
        /// Writes an <see cref="LogLevel.Error"/> message to log.
        /// </summary>
        /// <param name="message">Text to write to log.</param>
        public virtual void Error(string message)
        {
            WriteFilteredLog(LogLevel.Error, Colors.OrangeRed, message);
        }

        public virtual void Exception(Exception exception)
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            Error($"{method?.GetType().Name} {method?.Name} \n{exception}");
        }

        public static LogLevel GetRbLogLevel(ff14bot.Helpers.LogLevel rbLogLevel)
        {
            return rbLogLevel switch
            {
                ff14bot.Helpers.LogLevel.Verbose    => LogLevel.Verbose,
                ff14bot.Helpers.LogLevel.Diagnostic => LogLevel.Debug,
                ff14bot.Helpers.LogLevel.Normal     => LogLevel.Information,
                ff14bot.Helpers.LogLevel.Quiet      => LogLevel.Warning,
                _                                   => throw new ArgumentOutOfRangeException(nameof(rbLogLevel), rbLogLevel, null)
            };
        }

        public static LogLevel RbCurrentLogLevel => GetRbLogLevel(ff14bot.Helpers.Logging.LoggingLevel);
    }
}