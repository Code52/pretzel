using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipe;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    [CommandArguments(CommandName = BuiltInCommands.Create)]
    public class RecipeCommandParameters : PretzelBaseCommandParameters
    {
        public RecipeCommandParameters(IFileSystem fileSystem) : base(fileSystem) { }

        protected override void WithOptions(List<Option> options)
        {
            options.AddRange(new[]
            {
                new Option("withproject", "Includes a layout VS Solution, to give intellisense when editing razor layout files")
                {
                    Argument = new Argument<bool>()
                },
                new Option("wiki", "Creates a wiki instead of a blog (razor template only)")
                {
                    Argument = new Argument<bool>()
                },
            });
        }

        public bool WithProject { get; set; }
        public bool Wiki { get; set; }
    }

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Create, CommandDescription = "configure a new site")]
    public sealed class RecipeCommand : ICommand
    {
        private static readonly List<string> TemplateEngines = new List<string>(new[] { "Liquid", "Razor" });

#pragma warning disable 649

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public RecipeCommandParameters Parameters { get; set; }

        [ImportMany]
        public IEnumerable<IAdditionalIngredient> AdditionalIngredients { get; set; }

#pragma warning restore 649

        public async Task Execute()
        {
            Tracing.Info("create - configure a new site");

            var engine = String.IsNullOrWhiteSpace(Parameters.Template)
                             ? TemplateEngines.First()
                             : Parameters.Template;

            if (!TemplateEngines.Any(e => String.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested templating engine not found: {0}", engine);
                return;
            }

            Tracing.Info("Using {0} Engine", engine);

            var recipe = new Recipe(FileSystem, engine, Parameters.Path, AdditionalIngredients, Parameters.WithProject, Parameters.Wiki, Parameters.Drafts);
            recipe.Create();
        }
    }
}
