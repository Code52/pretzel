using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using NDesk.Options;
using Pretzel.Logic.Templating.Liquid;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand
    {
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
            Console.WriteLine("Path: " + Path);
            Console.WriteLine("Engine: " + Engine);
            Console.WriteLine("Debug: " + Debug);

            if (Engine.ToLower() == "jekyll")
            {
                var engine = new LiquidEngine();
                var fileSystem = new FileSystem();
                var context = new SiteContext { Folder = Path }; // TODO: process _config.yml file
                engine.Initialize(fileSystem, context);
                engine.Process();
            }

        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
