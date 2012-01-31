using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Pretzel.Commands;

namespace Pretzel
{
    class Program
    {
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
    }
}
