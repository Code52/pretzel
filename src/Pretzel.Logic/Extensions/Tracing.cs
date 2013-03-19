using System.Diagnostics;

namespace Pretzel.Logic.Extensions
{
    public static class Tracing
    {
        static Tracing()
        {
            Logger = new Logger();
        }

        public static Logger Logger { get; set; }

        public static void Debug(string message)
        {
            Logger.Write(message, "debug");
        }

        public static void Info(string message)
        {
            Logger.Write(message, "info");
        }

        public static void Error(string message)
        {
            Logger.Write(message, "error");
        }
    }
}
