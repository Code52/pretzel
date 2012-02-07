using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
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
        private Dictionary<string, object> config = new Dictionary<string, object>();
#pragma warning disable 649
        [Import] TemplateEngineCollection templateEngines;
        [Import] SiteContextGenerator Generator { get; set; }
        [Import] CommandParameters parameters;
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


            if (File.Exists(Path.Combine(parameters.Path, "_config.yml")))
                config = (Dictionary<string, object>)File.ReadAllText(Path.Combine(parameters.Path, "_config.yml")).YamlHeader(true);

            var context = Generator.BuildContext(parameters.Path, config);
            engine.Initialize();
            engine.Process(context);

            var watcher = new SimpleFileSystemWatcher();
            watcher.OnChange(parameters.Path, WatcherOnChanged);

            var w = new WebHost(engine.GetOutputDirectory(parameters.Path), new FileContentProvider());
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

            var context = Generator.BuildContext(parameters.Path, config);
            engine.Process(context);
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer);  // TODO: output relevant messages (not all of them)
        }
    }
}
