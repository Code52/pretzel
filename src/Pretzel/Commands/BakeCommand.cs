using System;
using System.ComponentModel.Composition;
using System.IO;
using NDesk.Options;

namespace Pretzel.Commands
{
    [Export(typeof(ICommand))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand
    {
        public string Path { get; private set; }
        public string Engine { get; private set; }

        private OptionSet GetSettings()
        {
            var settings = new OptionSet
            {
                { "e|engine=", "The render engine",
                    v => Engine = v},
                    {"p|path=", "The path to site directory",
                    p=> Path = p}
            };
            return settings;
        }

        public void Execute(string[] arguments)
        {
            GetSettings().Parse(arguments);
            Console.WriteLine("Path: " + Path);
            Console.WriteLine("Engine: " + Engine);
        }

        public void WriteHelp(TextWriter writer)
        {
            var settings = GetSettings();
            settings.WriteOptionDescriptions(writer);
        }
    }
}
