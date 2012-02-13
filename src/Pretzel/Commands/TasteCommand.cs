using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Minification;
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
#pragma warning disable 649
        [Import] TemplateEngineCollection templateEngines;
        [Import] SiteContextGenerator Generator { get; set; }
        [Import] CommandParameters parameters;
        [ImportMany] private IEnumerable<ITransform> transforms;
#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("taste - testing a site locally");

            parameters.Parse(arguments);

            if (string.IsNullOrWhiteSpace(parameters.Template))
            {
                parameters.DetectFromDirectory(templateEngines.Engines);
            }

            engine = templateEngines[parameters.Template];

            if (engine == null)
                return;

            var context = Generator.BuildContext(parameters.Path);
            engine.Initialize();
            engine.Process(context);
            foreach (var t in transforms)
                t.Transform(context);
            var watcher = new SimpleFileSystemWatcher();
            watcher.OnChange(parameters.Path, WatcherOnChanged);

            var w = new WebHost(engine.GetOutputDirectory(parameters.Path), new FileContentProvider(),Convert.ToInt32(parameters.Port));
            w.Start();

            Tracing.Info(string.Format("Browse to http://localhost:{0}/ to test the site.", parameters.Port));
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

            var context = Generator.BuildContext(parameters.Path);
            engine.Process(context);
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-t", "-d", "-p");
        }
    }
}
