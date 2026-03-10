using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Media;

namespace LlamaLibrary.Logging
{
    /// <summary>
    /// A TraceListener that forwards trace calls into an instance of LLogger.
    /// Use by registering: Trace.Listeners.Add(new LLoggerTraceListener(myLLogger));
    /// </summary>
    public sealed class LLoggerTraceListener : TraceListener
    {
        private readonly LLogger _logger;
        private readonly object _sync = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LLoggerTraceListener"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="LLogger"/> instance to forward trace messages to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
        public LLoggerTraceListener(LLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Name = "LLoggerTraceListener";
        }

        /// <summary>
        /// Writes a message to the underlying <see cref="LLogger"/> instance at Information level.
        /// </summary>
        /// <param name="message">The message to write. If null or empty, the message is ignored.</param>
        public override void Write(string? message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            // keep minimal locking around LLogger usage to avoid interleaved writes
            lock (_sync)
            {
                _logger.Information(message);
            }
        }

        /// <summary>
        /// Writes a line with a message to the underlying <see cref="LLogger"/> instance at Information level.
        /// </summary>
        /// <param name="message">The message to write. If null or empty, the message is ignored.</param>
        public override void WriteLine(string? message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            lock (_sync)
            {
                _logger.Information(message);
            }
        }

        /// <summary>
        /// Traces an event to the underlying <see cref="LLogger"/> instance with the appropriate log level based on the event type.
        /// </summary>
        /// <param name="eventCache">The trace event cache (not used).</param>
        /// <param name="source">The source of the trace event.</param>
        /// <param name="eventType">The type of the trace event, which determines the log level used.</param>
        /// <param name="id">The trace event identifier (not used).</param>
        /// <param name="message">The trace message. If null, the message is ignored.</param>
        /// <remarks>
        /// The <paramref name="eventType"/> is mapped to log levels as follows:
        /// <list type="table">
        /// <listheader><term>Event Type</term><description>Log Level</description></listheader>
        /// <item><term>Critical, Error</term><description>Error</description></item>
        /// <item><term>Warning</term><description>Warning</description></item>
        /// <item><term>Information</term><description>Information</description></item>
        /// <item><term>Verbose, Start, Stop, Suspend, Resume, Transfer</term><description>Verbose</description></item>
        /// <item><term>Other</term><description>Debug</description></item>
        /// </list>
        /// </remarks>
        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
        {
            if (message == null)
            {
                return;
            }

            lock (_sync)
            {
                switch (eventType)
                {
                    case TraceEventType.Critical:
                    case TraceEventType.Error:
                        _logger.Error(Format(source, message));
                        break;
                    case TraceEventType.Warning:
                        _logger.Warning(Format(source, message));
                        break;
                    case TraceEventType.Information:
                        _logger.Information(Format(source, message));
                        break;
                    case TraceEventType.Verbose:
                    case TraceEventType.Start:
                    case TraceEventType.Stop:
                    case TraceEventType.Suspend:
                    case TraceEventType.Resume:
                    case TraceEventType.Transfer:
                        _logger.Verbose(Format(source, message));
                        break;
                    default:
                        _logger.Debug(Format(source, message));
                        break;
                }
            }
        }

        /// <summary>
        /// Traces a formatted event to the underlying <see cref="LLogger"/> instance with the appropriate log level based on the event type.
        /// </summary>
        /// <param name="eventCache">The trace event cache (not used).</param>
        /// <param name="source">The source of the trace event.</param>
        /// <param name="eventType">The type of the trace event, which determines the log level used.</param>
        /// <param name="id">The trace event identifier (not used).</param>
        /// <param name="format">The format string for the message. If null, the message is ignored.</param>
        /// <param name="args">Optional arguments to format into the format string.</param>
        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
        {
            if (format == null)
            {
                return;
            }

            var msg = args == null || args.Length == 0 ? format : string.Format(format, args);
            TraceEvent(eventCache, source, eventType, id, msg);
        }

        private static string Format(string? source, string message)
        {
            if (string.IsNullOrEmpty(source))
            {
                return message;
            }

            var sb = new StringBuilder();
            sb.Append(message);
            return sb.ToString();
        }
    }
}