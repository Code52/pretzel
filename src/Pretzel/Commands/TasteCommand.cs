using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Commands
{
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
        public CommandParameters Parameters { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("taste - testing a site locally");

            Parameters.Parse(arguments);

            var context = Generator.BuildContext(Parameters.Path, Parameters.DestinationPath, Parameters.IncludeDrafts);

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

            using (var watcher = new SimpleFileSystemWatcher(Parameters.DestinationPath))
            {
                watcher.OnChange(Parameters.Path, WatcherOnChanged);

                using (var w = new WebHost(Parameters.DestinationPath, new FileContentProvider(), Convert.ToInt32(Parameters.Port)))
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
                            Process.Start(url);
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

            var context = Generator.BuildContext(Parameters.Path, Parameters.DestinationPath, Parameters.IncludeDrafts);
            if (Parameters.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }
            engine.Process(context, true);
            foreach (var t in Transforms)
                t.Transform(context);
        }

        public void WriteHelp(TextWriter writer)
        {
            Parameters.WriteOptions(writer, "-t", "-d", "-p", "--nobrowser", "-cleantarget", "-s", "-destination");
        }
    }
}
