using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using NDesk.Options;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Jekyll;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand, IPartImportsSatisfiedNotification
    {
        private Dictionary<string, ISiteEngine> engineMap;

        [ImportMany]
        private Lazy<ISiteEngine, ISiteEngineInfo>[] Engines { get; set; }

        public string Path { get; private set; }
        public string Engine { get; private set; }
        public bool Debug { get; private set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               { "e|engine=", "The render engine", v => Engine = v },
                               { "p|path=", "The path to site directory", p => Path = p },
                               { "debug", "Enable debugging", p => Debug = true}
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Settings.Parse(arguments);
            
            var fileSystem = new FileSystem();

            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrWhiteSpace(Engine))
            {
                Engine = InferEngineFromDirectory(Path, fileSystem);
            }

            ISiteEngine engine;

            if (engineMap.TryGetValue(Engine, out engine))
            {
                var context = new SiteContext { Folder = Path };
                engine.Initialize(fileSystem, context);
                engine.Process();
            }
            else
            {
                Console.WriteLine("Cannot find engine for input: '{0}'", Engine);
                System.Diagnostics.Debug.WriteLine("Cannot find engine for input: '{0}'", Engine);
            }
        }

        private string InferEngineFromDirectory(string path, IFileSystem fileSystem)
        {
            foreach(var engine in engineMap)
            {
                if (!engine.Value.CanProcess(fileSystem, path)) continue;
                System.Diagnostics.Debug.WriteLine("Recommended engine for directory: '{0}'", engine.Key);
                return engine.Key;
            }

            return string.Empty;
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }

        public void OnImportsSatisfied()
        {
            engineMap = new Dictionary<string, ISiteEngine>(Engines.Length, StringComparer.OrdinalIgnoreCase);

            foreach (var command in Engines)
            {
                if (!engineMap.ContainsKey(command.Metadata.Engine))
                    engineMap.Add(command.Metadata.Engine, command.Value);
            }
        }
    }
}
