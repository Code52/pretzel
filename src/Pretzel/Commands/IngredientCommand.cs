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
    [CommandInfo(CommandName = "ingredient", CommandDescription = "create a new post")]
    public sealed class IngredientCommand : ICommand
    {
#pragma warning disable 649

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public CommandParameters Parameters { get; set; }

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("ingredient - create a new post");

            Parameters.Parse(arguments);

            var ingredient = new Ingredient(FileSystem, Parameters.NewPostTitle, Parameters.Path, Parameters.IncludeDrafts);
            ingredient.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            writer.Write("   Create a new post\r\n");
            Parameters.WriteOptions(writer, "newposttitle", "drafts", "-s");
        }
    }
}
