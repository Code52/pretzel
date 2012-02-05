using System.Collections.Generic;
using System.Linq;
using System.IO;
using dotless.Core;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Minification
{
    public class CssMinifier
    {

        private readonly IFileSystem fileSystem;
        private readonly IEnumerable<FileInfo> files;
        private readonly string outputPath;

        public CssMinifier(IFileSystem fileSystem, IEnumerable<FileInfo> files, string outputPath)
        {
            this.files = files;
            this.outputPath = outputPath;
            this.fileSystem = fileSystem;
        }

        public void Minify()
        {
            var bundled = fileSystem.BundleFiles(files);

            //todo resolve imports
            //_fileSystem.Directory.SetCurrentDirectory("");
            
            //minify
            var engineFactory = new EngineFactory();
            engineFactory.Configuration.MinifyOutput = true;

            var engine = engineFactory.GetEngine();
            var minified = engine.TransformToCss(bundled, files.First().FullName);

            //build file
            fileSystem.File.WriteAllText(outputPath, minified);
        }

        public string ProcessCss(FileInfo file)
        {
            var content = fileSystem.File.ReadAllText(file.FullName);

            //todo resolve imports
            //_fileSystem.Directory.SetCurrentDirectory("");
            var engineFactory = new EngineFactory();
            engineFactory.Configuration.MinifyOutput = true;
            var engine = engineFactory.GetEngine();
            
            return engine.TransformToCss(content, file.FullName);
        }
    }
}
