using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using dotless.Core;
using System.IO.Abstractions;

namespace Pretzel.Logic.Minification
{
    public class CssMinifier
    {
        private readonly IFileSystem fileSystem;
        private readonly IEnumerable<FileInfo> files;
        private readonly string outputPath;
        private readonly Func<ILessEngine> getEngine;

        public CssMinifier(IFileSystem fileSystem, IEnumerable<FileInfo> files, string outputPath, Func<ILessEngine> getEngine)
        {
            this.files = files;
            this.outputPath = outputPath;
            this.getEngine = getEngine;
            this.fileSystem = fileSystem;
        }

        public void Minify()
        {
            var bundled = fileSystem.BundleFiles(files);
            var engine = getEngine();
            var minified = engine.TransformToCss(bundled, files.First().FullName);
            fileSystem.File.WriteAllText(outputPath, minified);
        }

        public string ProcessCss(FileInfo file)
        {
            var content = fileSystem.File.ReadAllText(file.FullName);
            var engine = getEngine();
            return engine.TransformToCss(content, file.FullName);
        }
    }
}
