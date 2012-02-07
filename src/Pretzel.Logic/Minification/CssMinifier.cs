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
        private readonly Func<ILessEngine> getEngine;

        public CssMinifier(IFileSystem fileSystem, Func<ILessEngine> getEngine)
        {
            this.getEngine = getEngine;
            this.fileSystem = fileSystem;
        }

        public void Minify( IEnumerable<FileInfo> files, string outputPath)
        {
            var bundled = fileSystem.BundleFiles(files);
            var engine = getEngine();
            var minified = engine.TransformToCss(bundled, files.First().FullName);
            fileSystem.File.WriteAllText(outputPath, minified);
        }

        public string ProcessCss(string filePath)
        {
            var content = fileSystem.File.ReadAllText(filePath);
            var engine = getEngine();
            return engine.TransformToCss(content, filePath);
        }
    }
}
