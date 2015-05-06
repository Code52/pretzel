using NDesk.Options;
using Pretzel.Commands;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
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

        private static void Main(string[] args)
        {
            Tracing.Logger.SetWriter(Console.Out);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("error");

            var parameters = BaseParameters.Parse(args, new FileSystem());

            if (parameters.Debug)
            {
                Tracing.Logger.AddCategory("debug");
            }

            var program = new Program();
            Tracing.Info("starting pretzel...");
            Tracing.Debug(string.Format("V{0}", Assembly.GetExecutingAssembly().GetName().Version));

            program.Compose(parameters);

            if (parameters.Help || !args.Any())
            {
                program.ShowHelp(parameters.Options);
                return;
            }

            program.Run(args, parameters);
        }

        private void ShowHelp(OptionSet defaultSet)
        {
            Commands.WriteHelp(defaultSet);
            WaitForClose();
        }

        private void Run(string[] args, BaseParameters baseParameters)
        {
            if (Commands[baseParameters.CommandName] == null)
            {
                Console.WriteLine(@"Can't find command ""{0}""", baseParameters.CommandName);
                Commands.WriteHelp(baseParameters.Options);
                return;
            }

            Commands[baseParameters.CommandName].Execute(baseParameters.CommandArgs);
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

        public void Compose(BaseParameters parameters)
        {
            try
            {
                var catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

                LoadPlugins(catalog, parameters);

                var container = new CompositionContainer(catalog);

                var batch = new CompositionBatch();
                batch.AddPart(this);
                batch.AddPart(parameters);
                container.Compose(batch);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(@"Unable to load: \r\n{0}",
                    string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));

                throw;
            }
        }

        private void LoadPlugins(AggregateCatalog catalog, BaseParameters parameters)
        {
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

        private void AddScriptCs(AggregateCatalog mainCatalog, string pluginsPath)
        {
            var pretzelScriptCsPath = Assembly.GetEntryAssembly().Location.Replace("Pretzel.exe", "Pretzel.ScriptCs.dll");
            if (File.Exists(pretzelScriptCsPath))
            {
                var pretzelScriptcsAssembly = Assembly.LoadFile(pretzelScriptCsPath);
                if (pretzelScriptcsAssembly != null)
                {
                    var factoryType = pretzelScriptcsAssembly.GetType("Pretzel.ScriptCs.ScriptCsCatalogFactory");
                    if (factoryType != null)
                    {
                        var scriptCsCatalogMethod = factoryType.GetMethod("CreateScriptCsCatalog");
                        if (scriptCsCatalogMethod != null)
                        {
                            var catalog = (ComposablePartCatalog)scriptCsCatalogMethod.Invoke(null, new object[] { pluginsPath, new[] { typeof(DotLiquid.Tag), typeof(ITag) } });
                            mainCatalog.Catalogs.Add(catalog);
                        }
                        else
                        {
                            Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected and loaded, type 'Pretzel.ScriptCs.ScriptCsCatalogFactory' found but method 'CreateScriptCsCatalog' not found.");
                        }
                    }
                    {
                        Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected and loaded but type 'Pretzel.ScriptCs.ScriptCsCatalogFactory' not found.");
                    }
                }
                else
                {
                    Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected but not loaded.");
                }
            }
        }
    }
}
