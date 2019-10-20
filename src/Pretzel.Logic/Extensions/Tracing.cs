using System;

namespace Pretzel.Logic.Extensions
{
    /// <summary>
    /// Trace for Pretzel
    /// </summary>
    public static class Tracing
    {
        // Do nothing by default
        private static Action<string, TraceLevel> _trace = (message, TraceLevel) => { };
        private static TraceLevel _minLevel = TraceLevel.Info;

        internal static void SetTrace(Action<string, TraceLevel> trace)
        {
            _trace = trace;
        }

        internal static void SetMinimalLevel(TraceLevel level)
        {
            _minLevel = level;
        }

        /// <summary>
        /// Trace a debug message
        /// </summary>
        /// <param name="message">Message to trace</param>
        /// <param name="messageParameters">Format parameters for the message</param>
        public static void Debug(string message, params object[] messageParameters)
        {
            TraceMessage(message, messageParameters, TraceLevel.Debug);
        }

        /// <summary>
        /// Trace an info message
        /// </summary>
        /// <param name="message">Message to trace</param>
        /// <param name="messageParameters">Format parameters for the message</param>
        public static void Info(string message, params object[] messageParameters)
        {
            TraceMessage(message, messageParameters, TraceLevel.Info);
        }

        /// <summary>
        /// Trace a warning message
        /// </summary>
        /// <param name="message">Message to trace</param>
        /// <param name="messageParameters">Format parameters for the message</param>
        public static void Warning(string message, params object[] messageParameters)
        {
            TraceMessage(message, messageParameters, TraceLevel.Warning);
        }

        /// <summary>
        /// Trace an error message
        /// </summary>
        /// <param name="message">Message to trace</param>
        /// <param name="messageParameters">Format parameters for the message</param>
        public static void Error(string message, params object[] messageParameters)
        {
            TraceMessage(message, messageParameters, TraceLevel.Error);
        }

        private static void TraceMessage(string message, object[] messageParameters, TraceLevel messageLevel)
        {
            if (messageLevel >= _minLevel)
            {
                _trace(string.Format(message, messageParameters), messageLevel);
            }
        }
    }

    internal enum TraceLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
