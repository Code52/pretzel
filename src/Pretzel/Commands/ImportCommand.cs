using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using NDesk.Options;
using System.IO;
using System.IO.Abstractions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "import")]
    class ImportCommand : ICommand
    {
        readonly static List<string> Importers = new List<string>(new[] { "wordpress" });

        [Import]
        private IFileSystem fileSystem;

        public string SitePath { get; private set; }
        public string ImportPath { get; private set; }
        public string ImportType { get; set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               { "t|type=", "The import type", v => ImportType = v },
                               { "f|file=", "The path to import file", v => ImportPath = v },
                               { "p|path=", "The path to site directory", p => SitePath = p },
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Tracing.Info("import - import posts from external source");

            Settings.Parse(arguments);

            var path = String.IsNullOrWhiteSpace(SitePath)
                             ? Directory.GetCurrentDirectory()
                             : SitePath;

            if (!Importers.Any(e => String.Equals(e, ImportType, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info(String.Format("Requested import type not found: {0}", ImportType));
                return;
            }

            if (string.Equals("wordpress", ImportType, StringComparison.InvariantCultureIgnoreCase))
            {
                var wordpressImporter = new WordpressImport(fileSystem, SitePath, ImportPath);
                wordpressImporter.Import();
            }

            Tracing.Info("Import complete");
        }

        public void WriteHelp(System.IO.TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
