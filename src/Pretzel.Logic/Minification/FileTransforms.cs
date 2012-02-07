using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Parser;
using dotless.Core.Stylizers;

namespace Pretzel.Logic.Minification
{
    [Export]
    public class FileTransforms : IPartImportsSatisfiedNotification
    {
        [Import] IFileSystem fileSystem;
        CssMinifier minifier;
        string filePath;

        public bool CanProcess(string extension)
        {
            return extension.ToLower() == ".less"; // simple case
        }

        public string MapFile(string file)
        {
            return file.Replace(".less", ".css");
        }

        public string Process(string filePath)
        {
            this.filePath = filePath;
            return minifier.ProcessCss(filePath);
        }

        public void OnImportsSatisfied()
        {
            minifier = new CssMinifier(fileSystem, GetEngine);
        }

        private ILessEngine GetEngine()
        {
            var importer = new Importer(new FileReader(new CustomPathResolver(filePath)));
            var parser = new Parser(new PlainStylizer(), importer);
            var engine = new LessEngine(parser);
            return engine;
        }

        class CustomPathResolver : IPathResolver
        {
            readonly string filePath;

            public CustomPathResolver(string filePath)
            {
                this.filePath = filePath;
            }

            public string GetFullPath(string path)
            {
                var currentDirectory = Path.GetDirectoryName(filePath);
                return Path.Combine(currentDirectory, path);
            }
        }
    }
}
