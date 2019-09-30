using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipe;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    [CommandArguments(CommandName = BuiltInCommands.Ingredient)]
    public class IngredientCommandArguments : PretzelBaseCommandArguments
    {
        [ImportingConstructor]
        public IngredientCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }

        protected override IEnumerable<Option> CreateOptions() => base.CreateOptions().Concat(new[]
        {
            new Option(new [] { "--newposttitle", "-n" }, "The title of the new post (\"New post\" by default")
            {
                Argument = new Argument<string>(() => "New post")
            }
        });

        public string NewPostTitle { get; set; }
    }

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Ingredient, CommandDescription = "create a new post")]
    public sealed class IngredientCommand : IPretzelCommand
    {
        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public IngredientCommandArguments Parameters { get; set; }

        public Task<int> Execute()
        {
            Tracing.Info("ingredient - create a new post");

            var ingredient = new Ingredient(FileSystem, Parameters.NewPostTitle, Parameters.Source, Parameters.Drafts);
            ingredient.Create();

            return Task.FromResult(0);
        }
    }
}
