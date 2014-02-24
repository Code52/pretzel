using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
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
            var help = false;
            var nopause = false;
            var defaultSet = new OptionSet
                {
                    {"help", "Display help mode", p => help = true},
                    {"debug", "Enable debugging", p => debug = true},
                    {"nopause", "Don't show \"Press any key to continue...\" message after execution", p => nopause = true}
                };
            defaultSet.Parse(args);

            if (debug)
                Tracing.Logger.AddCategory("debug");

            var program = new Program();
            Tracing.Info("starting pretzel...");
            program.Compose();

            if (help || !args.Any())
            {
                program.ShowHelp(defaultSet);
                return;
            }

            program.Run(args, defaultSet, nopause);
        }

        private void ShowHelp(OptionSet defaultSet)
        {
            Commands.WriteHelp(defaultSet);
        }

        private void Run(string[] args, OptionSet defaultSet, bool nopause)
        {
            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();

            if (Commands[commandName] == null)
            {
                Console.WriteLine(@"Can't find command ""{0}""", commandName);
                Commands.WriteHelp(defaultSet);
                return;
            }

            Commands[commandName].Execute(commandArgs);
            if (!nopause) WaitForClose();
        }

        [Conditional("DEBUG")]
        public void WaitForClose()
        {
            Console.WriteLine(@"Press any key to continue...");
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                //Output is redirected, we don't care to keep console open just let it close
            }
        }

        public void Compose()
        {
            try
            {
                var first = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                var container = new CompositionContainer(first);

                var batch = new CompositionBatch();
                batch.AddExportedValue<IFileSystem>(new FileSystem());
                batch.AddPart(this);
                container.Compose(batch);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(@"Unable to load: \r\n{0}", 
                    string.Join("\r\n", ex.LoaderExceptions.Select(e=>e.Message)));

                throw;
            }
        }
    }
}
