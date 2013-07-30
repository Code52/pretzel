using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Pretzel.Commands;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;

namespace Pretzel
{
    public class Site
    {
        public RelayCommand TogglePause { get; set; }
        public int Port { get; set; }
        public string Directory { get; set; }
        public bool IsRunning { get; set; }

        private WebHost w;
        private ISiteEngine engine;
        [Import]
        private TemplateEngineCollection templateEngines;
        [Import]
        private SiteContextGenerator Generator { get; set; }
        [Import]
        private CommandParameters parameters;
        [ImportMany]
        private IEnumerable<ITransform> transforms;

        public Site()
        {
            TogglePause = new RelayCommand(p => true, p => Pause());
        }

        public void Pause()
        {
            if (w.IsRunning)
            {
                w.Stop();
            }
            else
            {
                Execute();
            }

        }

        public void Execute()
        {
            var context = Generator.BuildContext(Directory);

            if (string.IsNullOrWhiteSpace(parameters.Template))
                parameters.DetectFromDirectory(templateEngines.Engines, context);

            engine = templateEngines[parameters.Template];

            if (engine == null)
            {
                Tracing.Info(string.Format("template engine {0} not found - (engines: {1})", parameters.Template, string.Join(", ", templateEngines.Engines.Keys)));
                return;
            }

            engine.Initialize();
            engine.Process(context, true);

            foreach (var t in transforms)
                t.Transform(context);

            var watcher = new SimpleFileSystemWatcher();
            watcher.OnChange(Directory, WatcherOnChanged);
            w = new WebHost(engine.GetOutputDirectory(Directory), new FileContentProvider(), Convert.ToInt32(Port));
            w.Start();
        }

        private void WatcherOnChanged(string file)
        {
            Tracing.Info(string.Format("File change: {0}", file));

            var context = Generator.BuildContext(Directory);
            engine.Process(context, true);
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-t", "-d", "-p", "--nobrowser");
        }
    }
}