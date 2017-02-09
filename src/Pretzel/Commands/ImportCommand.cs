using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "import")]
    class ImportCommand : ICommand
    {
        readonly static List<string> Importers = new List<string>(new[] { "wordpress", "blogger" });

#pragma warning disable 649
        [Import] IFileSystem fileSystem;
        [Import] CommandParameters parameters;
#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("import - import posts from external source");

            parameters.Parse(arguments);

            if (!Importers.Any(e => String.Equals(e, parameters.ImportType, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested import type not found: {0}", parameters.ImportType);
                return;
            }

            if (string.Equals("wordpress", parameters.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var wordpressImporter = new WordpressImport(fileSystem, parameters.Path, parameters.ImportPath);
                wordpressImporter.Import();
            }
            else if (string.Equals("blogger", parameters.ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var bloggerImporter = new BloggerImport(fileSystem, parameters.Path, parameters.ImportPath);
                bloggerImporter.Import();
            }

            Tracing.Info("Import complete");
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-i", "-f");
        }
    }
}
