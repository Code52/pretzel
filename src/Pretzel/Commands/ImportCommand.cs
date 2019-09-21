using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Abstractions;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;
using System.Composition;

namespace Pretzel.Commands
{
    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Import, CommandDescription = "import posts from external source")]
    class ImportCommand : ICommand
    {
        readonly static List<string> Importers = new List<string>(new[] { "wordpress", "blogger" });

#pragma warning disable 649
        [Import] public IFileSystem FileSystem { get; set; }
        [Import] public CommandParameters Parameters { get; set; }
#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("import - import posts from external source");

            Parameters.Parse(arguments);

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
