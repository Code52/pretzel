using NDesk.Options;
using Pretzel.Commands;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using System;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Pretzel
{
    internal class Program
    {
        [Import]
        public CommandCollection Commands { get; set; }

        [Export]
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        [Export("SourcePath")]
        public string SourcePath { get; }

        private BaseParameters parameters { get; }

        public Program()
        {
            parameters = BaseParameters.Parse(Args, FileSystem);
            SourcePath = parameters.Path;
        }

        private static string[] Args;
        private static void Main(string[] args)
        {
            Args = args;
            var program = new Program();

            program.InitializeTrace();
            Tracing.Info("starting pretzel...");
            Tracing.Debug("V{0}", Assembly.GetExecutingAssembly().GetName().Version);

            using (program.Compose())
            {

                if (program.parameters.Help || !args.Any())
                {
                    program.ShowHelp();
                    return;
                }

                program.Run();
            }
        }

        private void InitializeTrace()
        {
            Tracing.SetTrace(ConsoleTrace.Write);

            if (parameters.Debug)
            {
                Tracing.SetMinimalLevel(TraceLevel.Debug);
            }
        }

        private void ShowHelp()
        {
            Commands.WriteHelp(parameters.Options);
            WaitForClose();
        }

        private void Run()
        {
            if (Commands[parameters.CommandName] == null)
            {
                Console.WriteLine(@"Can't find command ""{0}""", parameters.CommandName);
                Commands.WriteHelp(parameters.Options);
                return;
            }

            Commands[parameters.CommandName].Execute(parameters.CommandArgs);
            WaitForClose();
        }

        [System.Diagnostics.Conditional("DEBUG")]
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

        public IDisposable Compose()
        {
            try
            {
                var configuration = new ContainerConfiguration();

                configuration
                    .WithAssembly(Assembly.GetExecutingAssembly())
                    .WithAssembly(typeof(Logic.SanityCheck).Assembly)
                    ;

                LoadPlugins(configuration);

                var container = configuration.CreateContainer();

                container.SatisfyImports(this);

                return container;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(@"Unable to load: \r\n{0}",
                    string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));

                throw;
            }
        }

        private void LoadPlugins(ContainerConfiguration configuration)
        {
            if (!parameters.Safe)
            {
                var pluginsPath = Path.Combine(parameters.Path, "_plugins");

                if (Directory.Exists(pluginsPath))
                {
                    var files = Directory.EnumerateFiles(pluginsPath, "*.dll", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var asm = Assembly.LoadFrom(file);
                            configuration.WithAssembly(asm);
                        }
                        catch (ReflectionTypeLoadException)
                        {
                        }
                        catch (BadImageFormatException)
                        {
                        }
                    }

                    AddScriptCs(configuration, pluginsPath);
                }
            }
        }

        private void AddScriptCs(ContainerConfiguration configuration, string pluginsPath)
        {
            //TODO: Make ScriptCS Working
            return;

            //var pretzelScriptCsPath = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "Pretzel.ScriptCs.dll");
            //if (File.Exists(pretzelScriptCsPath))
            //{
            //    var pretzelScriptcsAssembly = Assembly.LoadFile(pretzelScriptCsPath);
            //    if (pretzelScriptcsAssembly != null)
            //    {
            //        var factoryType = pretzelScriptcsAssembly.GetType("Pretzel.ScriptCs.ScriptCsCatalogFactory");
            //        if (factoryType != null)
            //        {
            //            var scriptCsCatalogMethod = factoryType.GetMethod("CreateScriptCsCatalog");
            //            if (scriptCsCatalogMethod != null)
            //            {
            //                var catalog = (ComposablePartCatalog)scriptCsCatalogMethod.Invoke(null, new object[]
            //                    {
            //                        pluginsPath,
            //                        new[]
            //                        {
            //                            typeof(DotLiquid.Tag),
            //                            typeof(Logic.Extensibility.ITag),
            //                            typeof(Logic.Templating.Context.SiteContext),
            //                            typeof(IFileSystem),
            //                            typeof(IConfiguration),
            //                        }
            //                    });
            //                mainCatalog.Catalogs.Add(catalog);
            //            }
            //            else
            //            {
            //                Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected and loaded, type 'Pretzel.ScriptCs.ScriptCsCatalogFactory' found but method 'CreateScriptCsCatalog' not found.");
            //            }
            //        }
            //        else
            //        {
            //            Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected and loaded but type 'Pretzel.ScriptCs.ScriptCsCatalogFactory' not found.");
            //        }
            //    }
            //    else
            //    {
            //        Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected but not loaded.");
            //    }
            //}
        }
    }
}
