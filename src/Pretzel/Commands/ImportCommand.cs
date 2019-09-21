using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Abstractions;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;
using System.Composition;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    [CommandArguments(CommandName = BuiltInCommands.Import)]
    public class ImportParameters : BParameters
    {
        protected override void WithOptions(List<Option> options)
        {
            options.AddRange(new[]
            {
                new Option(new [] {"import", "i"}, "The import type")
                {
                    Argument = new Argument<string>()
                },
                new Option(new [] {"file", "f"}, "Path to import file")
                {
                    Argument = new Argument<string>()
                },
            });
        }
    }

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Import, CommandDescription = "import posts from external source")]
    class ImportCommand : ICommand
    {
        public ICommandHandler CreateCommandHandler()
        {
            return CommandHandler.Create<BakeCommandParameters>(x =>
            {

            });
        }

        readonly static List<string> Importers = new List<string>(new[] { "wordpress", "blogger" });

#pragma warning disable 649
        [Import] public IFileSystem FileSystem { get; set; }
        [Import] public CommandParameters Parameters { get; set; }
#pragma warning restore 649

        public async Task Execute()
        {
            Tracing.Info("import - import posts from external source");
            
            if (!Importers.Any(e => String.Equals(e, Parameters.ImportType, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested import type not found: {0}", Parameters.ImportType);
                return;
            }

            if (string.Equals("wordpress", Parameters.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var wordpressImporter = new WordpressImport(FileSystem, Parameters.Path, Parameters.ImportPath);
                wordpressImporter.Import();
            }
            else if (string.Equals("blogger", Parameters.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var bloggerImporter = new BloggerImport(FileSystem, Parameters.Path, Parameters.ImportPath);
                bloggerImporter.Import();
            }

            Tracing.Info("Import complete");
        }

        public void WriteHelp(TextWriter writer)
        {
            Parameters.WriteOptions(writer, "-i", "-f");
        }
    }
}
