using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "taste")]
    public sealed class TasteCommand : ICommand
    {
        private ISiteEngine engine;
#pragma warning disable 649

        [Import]
        private TemplateEngineCollection templateEngines;

        [Import]
        private SiteContextGenerator Generator { get; set; }

        [Import]
        private CommandParameters parameters;

        [ImportMany]
        private IEnumerable<ITransform> transforms;

        [Import]
        private IFileSystem FileSystem;

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("taste - testing a site locally");

            parameters.Parse(arguments);

            var context = Generator.BuildContext(parameters.Path, parameters.DestinationPath, parameters.IncludeDrafts);

            if (parameters.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(parameters.Template))
            {
                parameters.DetectFromDirectory(templateEngines.Engines, context);
            }

            engine = templateEngines[parameters.Template];

            if (engine == null)
            {
                Tracing.Info(string.Format("template engine {0} not found - (engines: {1})", parameters.Template,
                                           string.Join(", ", templateEngines.Engines.Keys)));

                return;
            }

            engine.Initialize();
            engine.Process(context, skipFileOnError: true);
            foreach (var t in transforms)
                t.Transform(context);

            using (var watcher = new SimpleFileSystemWatcher(parameters.DestinationPath))
            {
                watcher.OnChange(parameters.Path, WatcherOnChanged);

                using (var w = new WebHost(parameters.DestinationPath, new FileContentProvider(), Convert.ToInt32(parameters.Port)))
                {
                    try
                    {
                        w.Start();
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        Tracing.Info(string.Format("Port {0} is already in use", parameters.Port));
                        return;
                    }

                    var url = string.Format("http://localhost:{0}/", parameters.Port);
                    if (parameters.LaunchBrowser)
                    {
                        Tracing.Info(string.Format("Opening {0} in default browser...", url));
                        try
                        {
                            Process.Start(url);
                        }
                        catch (Exception)
                        {
                            Tracing.Info(string.Format("Failed to launch {0}.", url));
                        }
                    }
                    else
                    {
                        Tracing.Info(string.Format("Browse to {0} to view the site.", url));
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
            Tracing.Info(string.Format("File change: {0}", file));

            var context = Generator.BuildContext(parameters.Path, parameters.DestinationPath, parameters.IncludeDrafts);
            if (parameters.CleanTarget && FileSystem.Directory.Exists(context.OutputFolder))
            {
                FileSystem.Directory.Delete(context.OutputFolder, true);
            }
            engine.Process(context, true);
            foreach (var t in transforms)
                t.Transform(context);
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-t", "-d", "-p", "--nobrowser", "-cleantarget", "-s", "-destination");
        }
    }
}
