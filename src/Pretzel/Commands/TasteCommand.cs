using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    [CommandArguments(CommandName = BuiltInCommands.Taste)]
    public class TasteParameters : BakeBaseCommandParameters
    {
        [ImportingConstructor]
        public TasteParameters(IFileSystem fileSystem) : base(fileSystem) { }

        protected override void WithOptions(List<Option> options)
        {
            base.WithOptions(options);
            options.AddRange(new[]
            {
                new Option(new[] { "--port", "-p" }, "The port to test the site locally")
                {
                    Argument = new Argument<int>(() => 8080)
                },
                new Option("--nobrowser", "Do not launch a browser (false by default)")
                {
                    Argument = new Argument<bool>(() => false)
                },
            });
        }

        public int Port { get; set; }

        public bool NoBrowser { get; set; }

        public bool LaunchBrowser => !NoBrowser;
    }

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Taste, CommandDescription = "testing a site locally")]
    public sealed class TasteCommand : ICommand
    {
        private ISiteEngine engine;
#pragma warning disable 649

        [Import]
        public TemplateEngineCollection TemplateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [Import]
        public TasteParameters Parameters { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

#pragma warning restore 649

        public async Task Execute()
        {
            Tracing.Info("taste - testing a site locally");
            
            var context = Generator.BuildContext(Parameters.Path, Parameters.Destination, Parameters.Drafts);

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

                return;
            }

            engine.Initialize();
            engine.Process(context, skipFileOnError: true);
            foreach (var t in Transforms)
                t.Transform(context);

            using (var watcher = new SimpleFileSystemWatcher(Parameters.Destination))
            {
                watcher.OnChange(Parameters.Path, WatcherOnChanged);

                using (var w = new WebHost(Parameters.Destination, new FileContentProvider(), Convert.ToInt32(Parameters.Port)))
                {
                    try
                    {
                        w.Start();
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        Tracing.Info("Port {0} is already in use", Parameters.Port);
                        return;
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
        }

        private void WatcherOnChanged(string file)
        {
            if (file.StartsWith(Parameters.Path))
            {
                var relativeFile = file.Substring(Parameters.Path.Length).ToRelativeFile();
                if (Generator.IsExcludedPath(relativeFile))
                {
                    return;
                }
            }

            Tracing.Info("File change: {0}", file);

            ((Configuration)Configuration).ReadFromFile();

            var context = Generator.BuildContext(Parameters.Path, Parameters.Destination, Parameters.Drafts);
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
