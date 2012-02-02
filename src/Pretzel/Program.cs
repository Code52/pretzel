using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using NDesk.Options;
using Pretzel.Commands;
using Pretzel.Logic.Extensions;

namespace Pretzel
{
    class Program
    {
        [Import]
        private CommandCollection Commands { get; set; }

        static void Main(string[] args)
        {
            Tracing.Logger.SetWriter(Console.Out);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("error");

            var debug = false;
            var defaultSet = new OptionSet { { "debug", "Enable debugging", p => debug = true } };
            defaultSet.Parse(args);

            if (debug)
                Tracing.Logger.AddCategory("debug");

            new Program().Run(args);
        }


        public void Run(string[] args)
        {
            Tracing.Info("starting pretzel...");

            Compose();

            if (!args.Any())
            {
                Commands.WriteHelp();
                return;
            }

            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();
            Commands[commandName].Execute(commandArgs);
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

            var batch = new CompositionBatch();
            batch.AddExportedValue<IFileSystem>(new FileSystem());
            batch.AddPart(this);
            container.Compose(batch);
        }
    }
}
