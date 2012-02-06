using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "taste")]
    public sealed class TasteCommand : ICommand
    {
        private ISiteEngine engine;

        [Import] TemplateEngineCollection templateEngines;

        [Import] CommandParameters parameters;

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("taste - testing a site locally");

            parameters.Parse(arguments);

            var f = new FileContentProvider();

            if (string.IsNullOrWhiteSpace(parameters.Template))
            {
                parameters.InferEngineFromDirectory(templateEngines.Engines);
            }

            engine = templateEngines[parameters.Template];

            if (engine == null)
                return;

            var context = new SiteContext { Folder = parameters.Path };
            engine.Initialize();
            engine.Process(context);

            var watcher = new SimpleFileSystemWatcher();
            watcher.OnChange(parameters.Path, WatcherOnChanged);

            var w = new WebHost(engine.GetOutputDirectory(parameters.Path), f);
            w.Start();

            Tracing.Info("Press 'Q' to stop the web host...");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            }
            while (key.Key != ConsoleKey.Q);
        }

        private void WatcherOnChanged(string file)
        {
            Tracing.Info(string.Format("File change: {0}", file));

            var context = new SiteContext { Folder = parameters.Path };
            engine.Process(context);
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer);
        }
    }
}
