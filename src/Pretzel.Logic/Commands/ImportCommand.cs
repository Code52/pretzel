using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;

namespace Pretzel.Logic.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
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
    [CommandInfo(
        Name = BuiltInCommands.Import,
        Description = "import posts from external source",
        ArgumentsType = typeof(ImportCommandArguments)
        )]
    class ImportCommand : Command<ImportCommandArguments>
    {
        readonly static List<string> Importers = new List<string>(new[] { "wordpress", "blogger" });

        [Import]
        public IFileSystem FileSystem { get; set; }

        protected override Task<int> Execute(ImportCommandArguments arguments)
        {
            Tracing.Info("import - import posts from external source");

            if (!Importers.Any(e => string.Equals(e, arguments.ImportType, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested import type not found: {0}", arguments.ImportType);

                return Task.FromResult(1);
            }

            if (string.Equals("wordpress", arguments.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var wordpressImporter = new WordpressImport(FileSystem, arguments.Source, arguments.ImportFile);
                wordpressImporter.Import();
            }
            else if (string.Equals("blogger", arguments.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var bloggerImporter = new BloggerImport(FileSystem, arguments.Source, arguments.ImportFile);
                bloggerImporter.Import();
            }

            Tracing.Info("Import complete");

            return Task.FromResult(0);
        }
    }
}
