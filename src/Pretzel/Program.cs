using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pretzel.Logic;
using Pretzel.Logic.Extensions;

namespace Pretzel
{
    class Program
    {
        private readonly static List<string> Engines = new List<string>(new[]
                                       {
                                           "Liquid",
                                           "Razor"
                                       });

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
            else if (string.Equals("recipe", command, StringComparison.InvariantCultureIgnoreCase))
                Recipe(commandArgs);
#if DEBUG
            else if (string.Equals("test", command, StringComparison.InvariantCultureIgnoreCase))
                Test(commandArgs);
#endif
            else
            {
                Console.WriteLine("Unknown Command: " + command);
                Usage();
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  pretzel recipe [OPTIONS] [PATH]");
            Console.WriteLine("  pretzel bake [OPTIONS] [PATH]");
            Console.WriteLine("  pretzel taste [OPTIONS]");
            Console.WriteLine();

            Console.WriteLine("Recipe Options");
            RecipeOptions.WriteHelp(Console.Out);

            Console.WriteLine();

            Console.WriteLine("Bake Options");
            BakeOptions.WriteHelp(Console.Out);

            Console.WriteLine();

            Console.WriteLine("Taste Options");
            TasteOptions.WriteHelp(Console.Out);
        }

        private static void Test(string[] args)
        {
            var text = @"---
                        layout: post
                        title: This is a test jekyll document
                        description: TEST ALL THE THINGS
                        date: 2012-01-30
                        tags : 
                        - test
                        - alsotest
                        - lasttest
                        ---
            
                        ##Test
            
                        This is a test of YAML parsing";

            var header = text.YamlHeader();
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

        public static void Recipe(string[] args)
        {
            var options = RecipeOptions.Parse(args);

            var engine = string.IsNullOrWhiteSpace(options.Engine)
                             ? "Liquid"
                             : options.Engine;

            if(!Engines.Any(e => string.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Console.WriteLine(string.Format("Requested Render Engine not found: {0}", engine));
                return;
            }

            var createResponse = new Recipe(engine, options.Path).Create();

            Console.WriteLine(createResponse);
        }
    }
}
