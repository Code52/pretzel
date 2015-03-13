using System.Diagnostics;

namespace Pretzel.Logic.Extensions
{
    public static class Tracing
    {
        public enum Category {
            Debug,
            Info,
            Error
        }

        static Tracing()
        {
            Logger = new Logger();
        }

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
    }
}
