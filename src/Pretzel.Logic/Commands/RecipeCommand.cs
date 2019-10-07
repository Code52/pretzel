using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipes;

namespace Pretzel.Logic.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class RecipeCommandArguments : PretzelBaseCommandArguments
    {
        [ImportingConstructor]
        public RecipeCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }

        protected override IEnumerable<Option> CreateOptions() => base.CreateOptions().Concat(new[]
        {
            new Option(new [] { "-p", "--withproject" }, "Includes a layout VS Solution, to give intellisense when editing razor layout files")
            {
                Argument = new Argument<bool>()
            },
            new Option(new [] { "-w", "--wiki"}, "Creates a wiki instead of a blog (razor template only)")
            {
                Argument = new Argument<bool>()
            },
        });

        public bool WithProject { get; set; }
        public bool Wiki { get; set; }
    }

    [Shared]
    [CommandInfo(
        Name = BuiltInCommands.Create,
        Description = "configure a new site",
        ArgumentsType = typeof(RecipeCommandArguments)
        )]
    public sealed class RecipeCommand : Command<RecipeCommandArguments>
    {
        private static readonly List<string> TemplateEngines = new List<string>(new[] { "Liquid", "Razor" });

        [Import]
        public IFileSystem FileSystem { get; set; }

        [ImportMany]
        public IEnumerable<IAdditionalIngredient> AdditionalIngredients { get; set; }

        protected override Task<int> Execute(RecipeCommandArguments arguments)
        {
            Tracing.Info("create - configure a new site");

            var engine = string.IsNullOrWhiteSpace(arguments.Template)
                             ? TemplateEngines.First()
                             : arguments.Template;

            if (!TemplateEngines.Any(e => string.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested templating engine not found: {0}", engine);

                return Task.FromResult(1);
            }

            Tracing.Info("Using {0} Engine", engine);

            var recipe = new Recipe(FileSystem, engine, arguments.Source, AdditionalIngredients, arguments.WithProject, arguments.Wiki, arguments.Drafts);
            recipe.Create();

            return Task.FromResult(0);
        }
    }
}
