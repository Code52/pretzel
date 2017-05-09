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

        private static readonly HashSet<ConsoleColor> BrightConsoleColors = new HashSet<ConsoleColor>
        {
            ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Gray, ConsoleColor.Green,
            ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.White, ConsoleColor.Yellow
        };

        static ConsoleTrace()
        {
            // If console uses bright background, adjust text colors.
            if (BrightConsoleColors.Contains(Console.BackgroundColor))
            {
                Colors[TraceLevel.Info] = ConsoleColor.Black;
                Colors[TraceLevel.Debug] = ConsoleColor.DarkGray;
            }
        }

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
