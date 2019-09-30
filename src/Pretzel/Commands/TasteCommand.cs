using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    [CommandArguments(CommandName = BuiltInCommands.Taste)]
    public class TasteCommandParameters : BakeBaseCommandParameters
    {
        [ImportingConstructor]
        public TasteCommandParameters(IFileSystem fileSystem) : base(fileSystem) { }

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
    [CommandInfo(CommandName = BuiltInCommands.Taste, CommandDescription = "testing a site locally")]
    public sealed class TasteCommand : IPretzelCommand
    {
        private ISiteEngine engine;

        [Import]
        public TemplateEngineCollection TemplateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [Import]
        public TasteCommandParameters Parameters { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

        public Task Execute()
        {
            Tracing.Info("taste - testing a site locally");
            
            var context = Generator.BuildContext(Parameters.Source, Parameters.Destination, Parameters.Drafts);

            if (Parameters.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(Parameters.Template))
            {
                Parameters.DetectFromDirectory(TemplateEngines.Engines, context);
            }

            engine = TemplateEngines[Parameters.Template];

            if (engine == null)
            {
                Tracing.Info("template engine {0} not found - (engines: {1})", Parameters.Template,
                                           string.Join(", ", TemplateEngines.Engines.Keys));

                return Task.CompletedTask;
            }

            engine.Initialize();
            engine.Process(context, skipFileOnError: true);
            foreach (var t in Transforms)
                t.Transform(context);

            using (var watcher = new SimpleFileSystemWatcher(Parameters.Destination))
            {
                watcher.OnChange(Parameters.Source, WatcherOnChanged);

                using (var w = new WebHost(Parameters.Destination, new FileContentProvider(), Convert.ToInt32(Parameters.Port)))
                {
                    try
                    {
                        w.Start();
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        Tracing.Info("Port {0} is already in use", Parameters.Port);

                        return Task.CompletedTask;
                    }

                    var url = string.Format("http://localhost:{0}/", Parameters.Port);
                    if (Parameters.LaunchBrowser)
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

            return Task.CompletedTask;
        }

        private void WatcherOnChanged(string file)
        {
            if (file.StartsWith(Parameters.Source))
            {
                var relativeFile = file.Substring(Parameters.Source.Length).ToRelativeFile();
                if (Generator.IsExcludedPath(relativeFile))
                {
                    return;
                }
            }

            Tracing.Info("File change: {0}", file);

            Configuration.ReadFromFile(Parameters.Source);

            var context = Generator.BuildContext(Parameters.Source, Parameters.Destination, Parameters.Drafts);
            if (Parameters.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }
            engine.Process(context, true);
            foreach (var t in Transforms)
                t.Transform(context);
        }
    }
}
