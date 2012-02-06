﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using NDesk.Options;
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
        private string Engine { get; set; }
        public int Port { get; private set; }
        public string Path { get; private set; }

        private ISiteEngine engine;

        [Import] TemplateEngineCollection templateEngines;
        [Import] SiteContextGenerator Generator { get; set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               {"p|port=", "The server port number.", v => Port = int.Parse(v)},
                               {"d|path=", "The path to site directory", p => Path = p },
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Tracing.Info("taste - testing a site locally");
            Settings.Parse(arguments);
            if (Port == 0)
            {
                Port = 8080;
            }

            var f = new FileContentProvider();
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrWhiteSpace(Engine))
            {
                Engine = InferEngineFromDirectory(Path);
            }

            engine = templateEngines[Engine];

            if (engine == null)
                return;

            var context = Generator.BuildContext(Path);
            engine.Initialize();
            engine.Process(context);

            var watcher = new SimpleFileSystemWatcher();
            watcher.OnChange(Path, WatcherOnChanged);

            var w = new WebHost(engine.GetOutputDirectory(Path), f);
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

            var context = Generator.BuildContext(Path);
            engine.Process(context);
        }

        private string InferEngineFromDirectory(string path)
        {
            foreach (var engine in templateEngines.Engines)
            {
                if (!engine.Value.CanProcess(path)) continue;
                Tracing.Info(String.Format("Recommended engine for directory: '{0}'", engine.Key));
                return engine.Key;
            }

            return string.Empty;
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
