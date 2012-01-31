using System;
using System.Linq;
using Pretzel.Logic.Minification;
using Pretzel.Logic.Extensions;
using System.IO;
using System.Collections.Generic;

namespace Pretzel
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Usage();
                return;
            }

            var command = args[0];
            var commandArgs = args.Skip(1).ToArray();

            if (string.Equals("usage", command, StringComparison.InvariantCultureIgnoreCase))
                Usage();
            else if (string.Equals("bake", command, StringComparison.InvariantCultureIgnoreCase))
                Bake(commandArgs);
            else if (string.Equals("taste", command, StringComparison.InvariantCultureIgnoreCase))
                Taste(commandArgs);
            else
            {
                Console.WriteLine("Unknown Command: " + command);
                Usage();
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  pretzel bake [OPTIONS] [PATH]");
            Console.WriteLine("  pretzel taste [OPTIONS]");
            Console.WriteLine();

            Console.WriteLine("Bake Options");
            BakeOptions.WriteHelp(Console.Out);

            Console.WriteLine();

            Console.WriteLine("Taste Options");
            TasteOptions.WriteHelp(Console.Out);
        }

        public static void Bake(string[] args)
        {
            var options = BakeOptions.Parse(args);

            // TODO Implement Bake

            Console.WriteLine("Path: " + options.Path);
            Console.WriteLine("Engine: " + options.Engine);
        }

        public static void Taste(string[] args)
        {
            var options = TasteOptions.Parse(args);

            // TODO Implement Taste
            Console.WriteLine("Port: " + options.Port);
        }
    }
}
