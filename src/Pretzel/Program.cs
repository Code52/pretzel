using NDesk.Options;
using Pretzel.Commands;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.ScriptCs.Contracts;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Pretzel
{
    internal class Program
    {
        [Import]
        private CommandCollection Commands { get; set; }

        private AggregateCatalog catalog;

        private CompositionContainer container;

        private static void Main(string[] args)
        {
            Tracing.Logger.SetWriter(Console.Out);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("error");

            var debug = false;
            var help = false;
            var defaultSet = new OptionSet
                {
                    {"help", "Display help mode", p => help = true},
                    {"debug", "Enable debugging", p => debug = true}
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

            program.Run(args, defaultSet);
        }

        private void ShowHelp(OptionSet defaultSet)
        {
            Commands.WriteHelp(defaultSet);
            WaitForClose();
        }

        private void Run(string[] args, OptionSet defaultSet)
        {
            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();

            if (Commands[commandName] == null)
            {
                Console.WriteLine(@"Can't find command ""{0}""", commandName);
                Commands.WriteHelp(defaultSet);
                return;
            }

            LoadPlugins(commandArgs);
            Commands[commandName].Execute(commandArgs);
            WaitForClose();
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
                // Output is redirected, we don't care to keep console open just let it close
            }
        }

        private void LoadPlugins(string[] commandArgs)
        {
            var parameters = container.GetExport<CommandParameters>().Value;
            parameters.Parse(commandArgs);

            if (!parameters.Safe)
            {
                var pluginsPath = Path.Combine(parameters.Path, "_plugins");

                if (Directory.Exists(pluginsPath))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(pluginsPath));
                    AddScriptCs(catalog, pluginsPath);
                }
            }
        }

        public void Compose()
        {
            try
            {
                var pretzelCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                //var logicCatalog = new AssemblyCatalog(typeof(CommandParameters).Assembly);
                catalog = new AggregateCatalog(pretzelCatalog/*, logicCatalog*/);
                container = new CompositionContainer(catalog);

                var batch = new CompositionBatch();
                batch.AddExportedValue<IFileSystem>(new FileSystem());
                batch.AddPart(this);
                container.Compose(batch);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(@"Unable to load: \r\n{0}",
                    string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));

                throw;
            }
        }

        private void AddScriptCs(AggregateCatalog mainCatalog, string pluginsPath)
        {
            if (File.Exists("Pretzel.ScriptCs.dll"))
            {
                var catalog = (ComposablePartCatalog)Assembly.LoadFile(Path.GetFullPath("Pretzel.ScriptCs.dll")).GetType("Pretzel.ScriptCs.ScriptCsPluginsEngine").GetMethod("GetScriptCsCatalog").Invoke(null, new[] { new ScriptCsPluginsEngineOptions { PluginsFolderPath = pluginsPath, References = new[] { typeof(DotLiquid.Tag), typeof(Pretzel.Logic.Extensibility.ITag) } } });
                mainCatalog.Catalogs.Add(catalog);
            }
        }
    }
}
