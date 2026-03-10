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

        public LLoggerTraceListener(LLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Name = "LLoggerTraceListener";
        }

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