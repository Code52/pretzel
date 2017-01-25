using System;

namespace Pretzel.Logic.Extensions
{
    public static class Tracing
    {
        [Obsolete("Use Tracing directly instead")]
        public enum Category {
            Debug,
            Info,
            Error
        }

        static Tracing()
        {
            Logger = new Logger();
        }

        [Obsolete("Use Tracing directly instead")]
        public static Logger Logger { get; set; }

        public static void Debug(string message)
        {
            Logger.Write(message, Category.Debug);
        }

        public static void Info(string message)
        {
            Logger.Write(message, Category.Info);
        }

        public static void Error(string message)
        {
            Logger.Write(message, Category.Error);
        }

        public static void DebugFormat(string message, params object[] messageParameters)
        {
            Logger.WriteFormat(message, Category.Debug, messageParameters);
        }

        public static void InfoFormat(string message, params object[] messageParameters)
        {
            Logger.WriteFormat(message, Category.Info, messageParameters);
        }

        public static void ErrorFormat(string message, params object[] messageParameters)
        {
            Logger.WriteFormat(message, Category.Error, messageParameters);
        }
    }
}
