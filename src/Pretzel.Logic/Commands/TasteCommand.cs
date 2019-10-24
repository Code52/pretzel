using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Pretzel.Logic;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Hosting;
using Pretzel.Logic.Modules;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class TasteCommandArguments : BakeBaseCommandArguments
    {
        [ImportingConstructor]
        public TasteCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }

        protected override IEnumerable<Option> CreateOptions() => base.CreateOptions().Concat(new[]
        {
            new Option(new[] { "--port", "-p" }, "The port to test the site locally")
            {
                Argument = new Argument<int>(() => 8080)
            },
            new Option("--nobrowser", "Do not launch a browser (false by default)")
            {
                Argument = new Argument<bool>()
            },
        });

        public int Port { get; set; }
        public bool NoBrowser { get; set; }
        public bool LaunchBrowser => !NoBrowser;
    }

    [Shared]
    [CommandInfo(
        Name = "taste",
        Description = "testing a site locally",
        ArgumentsType = typeof(TasteCommandArguments),
        CommandType = typeof(TasteCommand)
        )]
    public sealed class TasteCommand : Command<TasteCommandArguments>
    {
        private ISiteEngine engine;

        [Import]
        public TemplateEngineCollection TemplateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

        protected async override Task<int> Execute(TasteCommandArguments arguments)
        {
            Tracing.Info("taste - testing a site locally");

            var context = Generator.BuildContext(arguments.Source, arguments.Destination, arguments.Drafts);

            if (arguments.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(arguments.Template))
            {
                arguments.DetectFromDirectory(TemplateEngines.Engines, context);
            }

            engine = TemplateEngines[arguments.Template];

            if (engine == null)
            {
                Tracing.Info("template engine {0} not found - (engines: {1})", arguments.Template,
                                           string.Join(", ", TemplateEngines.Engines.Keys));

                return 1;
            }

            engine.Initialize();
            engine.Process(context, skipFileOnError: true);
            foreach (var t in Transforms)
                t.Transform(context);

            using (var watcher = new SimpleFileSystemWatcher(arguments.Destination))
            {
                watcher.OnChange(arguments.Source, file => WatcherOnChanged(file, arguments));

                using (var w = new AspNetCoreWebHost(arguments.Destination, Convert.ToInt32(arguments.Port), arguments.Debug))
                {
                    try
                    {
                        await w.Start();
                    }
                    catch (IOException ex) when (ex.InnerException is AddressInUseException)
                    {
                        Tracing.Info("Port {0} is already in use", arguments.Port);

                        return 1;
                    }

                    var url = string.Format("http://localhost:{0}/", arguments.Port);
                    if (arguments.LaunchBrowser)
                    {
                        Tracing.Info("Opening {0} in default browser...", url);
                        try
                        {
                            System.Diagnostics.Process.Start(url);
                        }
                        catch (Exception)
                        {
                            Tracing.Info("Failed to launch {0}.", url);
                        }
                    }
                    else
                    {
                        Tracing.Info("Browse to {0} to view the site.", url);
                    }

                    Tracing.Info("Press 'Q' to stop the web host...");
                    ConsoleKeyInfo key;
                    do
                    {
                        key = Console.ReadKey();
                    }
                    while (key.Key != ConsoleKey.Q);
                    Console.WriteLine();
                }
            }

            return 0;
        }

        private void WatcherOnChanged(string file, TasteCommandArguments arguments)
        {
            if (file.StartsWith(arguments.Source))
            {
                var relativeFile = file.Substring(arguments.Source.Length).ToRelativeFile();
                if (Generator.IsExcludedPath(relativeFile))
                {
                    return;
                }
            }

            Tracing.Info("File change: {0}", file);

            Configuration.ReadFromFile(arguments.Source);

            var context = Generator.BuildContext(arguments.Source, arguments.Destination, arguments.Drafts);
            if (arguments.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }
            engine.Process(context, true);
            foreach (var t in Transforms)
                t.Transform(context);
        }
    }
}
