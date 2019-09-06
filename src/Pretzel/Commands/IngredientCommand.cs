using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipe;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Commands
{
    [Shared]
    [CommandInfo(CommandName = "ingredient")]
    public sealed class IngredientCommand : ICommand
    {
#pragma warning disable 649

        [Import]
        public IFileSystem fileSystem { get; set; }

        [Import]
        public CommandParameters parameters { get; set; }

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("ingredient - create a new post");

            parameters.Parse(arguments);

            var ingredient = new Ingredient(fileSystem, parameters.NewPostTitle, parameters.PathProvider.Path, parameters.IncludeDrafts);
            ingredient.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            writer.Write("   Create a new post\r\n");
            parameters.WriteOptions(writer, "newposttitle", "drafts", "-s");
        }
    }
}
