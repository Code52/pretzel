using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pretzel.Commands;
using Pretzel.Logic.Extensions;

namespace Pretzel
{
    internal class Program
    {
#pragma warning disable S2223 // Non-constant static fields should not be visible
        internal static IFileSystem fileSystem = new FileSystem();

        internal static Option[] GlobalOptions = new[]
        {
            new Option("--debug", "Enable debugging")
            {
                Argument = new Argument<bool>()
            },
            new Option("--safe", "Disable custom plugins")
            {
                Argument = new Argument<bool>()
            },
            new Option(new[] { "--source", "-s" }, "The path to the source site (default current directory)")
            {
                Argument = new Argument<string>(() => fileSystem.Directory.GetCurrentDirectory())
            }
        };
#pragma warning restore S2223 // Non-constant static fields should not be visible

        [Import]
        public CommandCollection CommandCollection { get; set; }

        [Export]
        public IFileSystem FileSystem { get; set; } = fileSystem;

        private static async Task<int> Main(string[] args)
        {
            Tracing.SetTrace(ConsoleTrace.Write);

            args = PatchSourcePath(args);

            var parseResult = ParseArguments(args);
            if (parseResult.hasError)
                return -1;

            try
            {
                InitializeTrace(parseResult.debug);
                Tracing.Info("starting pretzel...");
                Tracing.Debug("V{0}", Assembly.GetExecutingAssembly().GetName().Version);

                using (var host = Compose(parseResult.debug, parseResult.safe, parseResult.source))
                {
                    var program = new Program();
                    host.SatisfyImports(program);
                    var result = await program.Run(GlobalOptions, args);
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
        }

        internal static (bool debug, bool safe, string source, bool hasError) ParseArguments(string[] args)
        {
            var command = new RootCommand()
            {
                TreatUnmatchedTokensAsErrors = false
            };

            foreach (var option in GlobalOptions)
                command.AddOption(option);

            var parseResult = new Parser(command).Parse(args);

            if (parseResult.Errors.Count > 0)
            {
                foreach (var error in parseResult.Errors)
                {
                    Tracing.Error(error.Message);
                }
                return (true, true, null, true);
            }
            var debug = parseResult.FindResultFor(GlobalOptions.First(m => m.Name == "debug"))?.GetValueOrDefault<bool>() ?? false;
            var safe = parseResult.FindResultFor(GlobalOptions.First(m => m.Name == "safe"))?.GetValueOrDefault<bool>() ?? false;
            var source = parseResult.FindResultFor(GlobalOptions.First(m => m.Name == "source"))?.GetValueOrDefault<string>();
            return (debug, safe, source, false);
        }


        internal static string[] PatchSourcePath(string[] args)
        {
            if (args.Length > 1 && !args.Contains("-s") && !args.Contains("--source"))
            {
                var firstArgument = args[1];

                // take the first argument after the command
                if (firstArgument != null && !firstArgument.StartsWith("-") && !firstArgument.StartsWith("/"))
                {
                    var arguments = args.ToList();
                    var path = fileSystem.Path.IsPathRooted(firstArgument)
                        ? firstArgument
                        : fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), firstArgument);

                    path = string.IsNullOrWhiteSpace(path)
                        ? fileSystem.Directory.GetCurrentDirectory()
                        : fileSystem.Path.GetFullPath(path);

                    arguments[1] = "-s";
                    arguments.Insert(2, path);
                    return arguments.ToArray();
                }
            }

            return args;
        }

        private static void InitializeTrace(bool debug)
        {
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
                    foreach (var command in CommandCollection.RootCommand.OfType<Command>())
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

        internal static CompositionHost Compose(bool debug, bool safe, string path)
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
                            //Cannot load the type
                        }
                        catch (BadImageFormatException)
                        {
                            //Cannot load the type. It's probably wrong bitness
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
