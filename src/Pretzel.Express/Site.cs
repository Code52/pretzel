using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using Pretzel.Commands;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Pretzel.Modules;

namespace Pretzel
{
    public class Site : INotifyPropertyChanged
    {
        public RelayCommand PauseCommand { get; set; }
        public int Port { get; set; }
        public string Directory { get; set; }

        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                isRunning = value;
                OnPropertyChanged();
            }
        }

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

        private bool isRunning;

        public Site()
        {
            PauseCommand = new RelayCommand(p => true, p => PlayPause(false));
            PlayCommand = new RelayCommand(p => true, p => PlayPause());
        }

        public RelayCommand PlayCommand { get; set; }

        public void PlayPause(bool play=true)
        {
            if (play)
            {
                Execute();
            }
            else
            {
                w.Stop();
            }

            IsRunning = w.IsRunning;
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

            IsRunning = true;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}