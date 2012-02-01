using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pretzel.Commands;
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

        [Import]
        private CommandCollection Commands { get; set; }

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        public void Run(string[] args)
        {
            Compose();

            if (!args.Any())
            {
                Commands.WriteHelp();
                return;
            }

            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();
            Commands[commandName].Execute(commandArgs);
        }

        public void Compose()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
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
