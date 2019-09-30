using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    [CommandArguments(CommandName = BuiltInCommands.Import)]
    public class ImportCommandArguments : PretzelBaseCommandArguments
    {
        [ImportingConstructor]
        public ImportCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }

        protected override IEnumerable<Option> CreateOptions() => base.CreateOptions().Concat(new[]
        {
            new Option(new [] {"--importtype", "-i"}, "The import type")
            {
                Argument = new Argument<string>()
            },
            new Option(new [] {"--importfile", "-f"}, "Path to import file")
            {
                Argument = new Argument<string>()
            }
        });

        public string ImportType { get; set; }

        public string ImportFile { get; set; }
    }

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Import, CommandDescription = "import posts from external source")]
    class ImportCommand : IPretzelCommand
    {
        readonly static List<string> Importers = new List<string>(new[] { "wordpress", "blogger" });

        [Import]
        public IFileSystem FileSystem { get; set; }
        [Import]
        public ImportCommandArguments Parameters { get; set; }

        public Task<int> Execute()
        {
            Tracing.Info("import - import posts from external source");

            if (!Importers.Any(e => String.Equals(e, Parameters.ImportType, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested import type not found: {0}", Parameters.ImportType);

                return Task.FromResult(1);
            }

            if (string.Equals("wordpress", Parameters.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var wordpressImporter = new WordpressImport(FileSystem, Parameters.Source, Parameters.ImportFile);
                wordpressImporter.Import();
            }
            else if (string.Equals("blogger", Parameters.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var bloggerImporter = new BloggerImport(FileSystem, Parameters.Source, Parameters.ImportFile);
                bloggerImporter.Import();
            }

            Tracing.Info("Import complete");

            return Task.FromResult(0);
        }
    }
}
