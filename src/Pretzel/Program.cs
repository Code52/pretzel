using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Pretzel.Commands;
using Pretzel.Logic.Templating;

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
            var command = Commands[commandName];
            if (command == null)
            {
                Commands.WriteInvalidCommand(commandName);
                return;
            }
            command.Execute(commandArgs);
            WaitForClose();
        }

        [Conditional("DEBUG")]
        public void WaitForClose()
        {
            Console.ReadLine();
        }

        public void Compose()
        {
            var first = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(first);
            container.ComposeParts(this);
        }
    }
}
