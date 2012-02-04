using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NDesk.Options;
using Pretzel.Logic;
using Pretzel.Logic.Extensions;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "create")]
    public sealed class RecipeCommand : ICommand
    {
        readonly static List<string> TemplateEngines = new List<string>(new[] { "Liquid", "Razor" });

        public string Path { get; private set; }
        public string Engine { get; private set; }

        [Import] 
        private IFileSystem fileSystem;

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               { "t|templates=", "The templates to use for the site", v => Engine = v },
                               { "p|path=", "The path to site directory", p => Path = p },
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Tracing.Info("create - configure a new site");

            Settings.Parse(arguments);

            var path = String.IsNullOrWhiteSpace(Path)
                             ? Directory.GetCurrentDirectory()
                             : Path;

            var engine = String.IsNullOrWhiteSpace(Engine)
                             ? TemplateEngines.First()
                             : Engine;

            if (!TemplateEngines.Any(e => String.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info(String.Format("Requested templating engine not found: {0}", engine));
                return;
            }

            var recipe = new Recipe(fileSystem, engine, path);
            recipe.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}