using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using NDesk.Options;
using Pretzel.Logic;
using Pretzel.Logic.Recipe;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "create")]
    public sealed class CreateCommand : ICommand
    {
        public string Engine { get; set; }
        public bool Debug { get; set; }
        public string Path { get; set; }

        [Import] 
        private IFileSystem fileSystem;

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

            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrWhiteSpace(Engine))
            {
                Engine = "";
            }

            var recipe = new Recipe(fileSystem, Engine, Path);
            recipe.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
