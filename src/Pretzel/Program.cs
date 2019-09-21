using Pretzel.Commands;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pretzel
{
    [Export]
    internal class Program
    {
        [Import]
        public CommandCollection CommandCollection { get; set; }

        [Export]
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        [Export("SourcePath")]
        public string SourcePath { get; }

        public Program()
        {
            SourcePath = sourcePath;
        }

        private static string sourcePath;
        private static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                TreatUnmatchedTokensAsErrors = false
            };

            var globalOptions = new[]
            {
                new Option("--debug", "Enable debugging")
                {
                    Argument = new Argument<bool>()
                },
                new Option("--safe", "Disable custom plugins")
                {
                    Argument = new Argument<bool>()
                },
                new Option(new[] { "-s", "--source" }, "The path to the source site (default current directory)")
                {
                    Argument = new Argument<string>(() => Environment.CurrentDirectory)
                }
            };

            foreach (var option in globalOptions)
            {
                rootCommand.AddOption(option);
            }

            rootCommand.Handler = CommandHandler.Create(async (bool debug, bool safe, string source) =>
            {
                sourcePath = source;
                try
                {
                    InitializeTrace(debug);
                    Tracing.Info("starting pretzel...");
                    Tracing.Debug("V{0}", Assembly.GetExecutingAssembly().GetName().Version);

                    using (var host = Compose(debug, safe, source))
                    {
                        var program = host.GetExport<Program>();
                        var result = await program.Run(globalOptions, args);
                        WaitForClose();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Tracing.Error(ex.Message);
                    WaitForClose();
                    return -1;
                }
            });
            try
            {
                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                Tracing.Error(ex.Message);
                WaitForClose();
                return -1;
            }
        }

        private static void InitializeTrace(bool debug)
        {
            Tracing.SetTrace(ConsoleTrace.Write);

            if (debug)
            {
                Tracing.SetMinimalLevel(TraceLevel.Debug);
            }
        }

        private async Task<int> Run(IEnumerable<Option> globalOptions, string[] args)
        {
            try
            {
                foreach (var option in globalOptions)
                {
                    CommandCollection.RootCommand.AddOption(option);

                    foreach(var command in CommandCollection.RootCommand.OfType<Command>())
                    {
                        command.AddOption(option);
                    }
                }

                return await CommandCollection.RootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                Tracing.Error(ex.Message);
                WaitForClose();
                return -1;
            }
            //if (Commands[parameters.CommandName] == null)
            //{
            //    Console.WriteLine(@"Can't find command ""{0}""", parameters.CommandName);
            //    //Commands.WriteHelp(parameters.Options);
            //    return -1;
            //}

            //Commands[parameters.CommandName].Execute(parameters.CommandArgs);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WaitForClose()
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

        public static CompositionHost Compose(bool debug, bool safe, string path)
        {
            try
            {
                var configuration = new ContainerConfiguration();

                configuration
                    .WithAssembly(Assembly.GetExecutingAssembly())
                    .WithAssembly(typeof(Logic.SanityCheck).Assembly)
                    ;

                LoadPlugins(configuration, safe, path);

                var container = configuration.CreateContainer();

                return container;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(@"Unable to load: \r\n{0}",
                    string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));

                throw;
            }
        }

        private static void LoadPlugins(ContainerConfiguration configuration, bool safe, string path)
        {
            if (!safe)
            {
                var pluginsPath = Path.Combine(path, "_plugins");

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

        private static void AddScriptCs(ContainerConfiguration configuration, string pluginsPath)
        {
            var pretzelScriptCsPath = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "Pretzel.ScriptCs.dll");
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
                            throw new NotSupportedException($"Currently there is no support for ScriptCS cause the lack of the new 'System.Composition' model.{Environment.NewLine}Please stay tuned.");
                        }
                        //TODO: ScriptCS Support
                        //if (scriptCsCatalogMethod != null)
                        //{
                        //    var catalog = (ComposablePartCatalog)scriptCsCatalogMethod.Invoke(null, new object[]
                        //        {
                        //            pluginsPath,
                        //            new[]
                        //            {
                        //                typeof(DotLiquid.Tag),
                        //                typeof(Logic.Extensibility.ITag),
                        //                typeof(Logic.Templating.Context.SiteContext),
                        //                typeof(IFileSystem),
                        //                typeof(IConfiguration),
                        //            }
                        //        });
                        //    mainCatalog.Catalogs.Add(catalog);
                        //}
                        //else
                        //{
                        //    Tracing.Debug("Assembly 'Pretzel.ScriptCs.dll' detected and loaded, type 'Pretzel.ScriptCs.ScriptCsCatalogFactory' found but method 'CreateScriptCsCatalog' not found.");
                        //}
                    }
                    else
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
