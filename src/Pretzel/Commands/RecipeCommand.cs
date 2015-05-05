using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipe;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "create")]
    public sealed class RecipeCommand : ICommand
    {
        private static readonly List<string> TemplateEngines = new List<string>(new[] { "Liquid", "Razor" });

#pragma warning disable 649

        [Import]
        private IFileSystem fileSystem;

        [Import]
        private CommandParameters parameters;

        [ImportMany]
        private IEnumerable<IAdditionalIngredient> additionalIngredients;

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("create - configure a new site");

            parameters.Parse(arguments);

            var engine = String.IsNullOrWhiteSpace(parameters.Template)
                             ? TemplateEngines.First()
                             : parameters.Template;

            if (!TemplateEngines.Any(e => String.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info(String.Format("Requested templating engine not found: {0}", engine));
                return;
            }

            Tracing.Info(string.Format("Using {0} Engine", engine));

            var recipe = new Recipe(fileSystem, engine, parameters.Path, additionalIngredients, parameters.WithProject, parameters.Wiki, parameters.IncludeDrafts);
            recipe.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-t", "-d", "withproject", "wiki", "-s");
        }
    }
}
