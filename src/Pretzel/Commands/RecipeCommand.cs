using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipe;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Commands
{
    [Shared]
    [CommandInfo(CommandName = "create")]
    public sealed class RecipeCommand : ICommand
    {
        private static readonly List<string> TemplateEngines = new List<string>(new[] { "Liquid", "Razor" });

#pragma warning disable 649

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public CommandParameters Parameters { get; set; }

        [ImportMany]
        public IEnumerable<IAdditionalIngredient> AdditionalIngredients { get; set; }

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("create - configure a new site");

            Parameters.Parse(arguments);

            var engine = String.IsNullOrWhiteSpace(Parameters.Template)
                             ? TemplateEngines.First()
                             : Parameters.Template;

            if (!TemplateEngines.Any(e => String.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested templating engine not found: {0}", engine);
                return;
            }

            Tracing.Info("Using {0} Engine", engine);

            var recipe = new Recipe(FileSystem, engine, Parameters.Path, AdditionalIngredients, Parameters.WithProject, Parameters.Wiki, Parameters.IncludeDrafts);
            recipe.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            Parameters.WriteOptions(writer, "-t", "-d", "withproject", "wiki", "-s");
        }
    }
}
