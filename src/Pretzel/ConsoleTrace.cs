using Pretzel.Logic.Extensions;
using System;
using System.Collections.Generic;

namespace Pretzel
{
    internal static class ConsoleTrace
    {
        private static readonly Dictionary<TraceLevel, ConsoleColor> Colors = new Dictionary<TraceLevel, ConsoleColor>
             {
                 { TraceLevel.Error, ConsoleColor.DarkRed },
                 { TraceLevel.Warning, ConsoleColor.DarkYellow },
                 { TraceLevel.Info, ConsoleColor.White },
                 { TraceLevel.Debug, ConsoleColor.Gray }
             };

        internal static void Write(string message, TraceLevel traceLevel)
        {
            ConsoleColor consoleColor;

            if (Colors.TryGetValue(traceLevel, out consoleColor))
            {
                var originalForground = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = consoleColor;
                    Console.WriteLine(message);
                }
                finally
                {
                    Console.ForegroundColor = originalForground;
                }
            }
            else
            {
                Console.WriteLine(message);
            }

        }
    }
}
