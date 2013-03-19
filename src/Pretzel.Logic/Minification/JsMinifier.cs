using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using Microsoft.Ajax.Utilities;

namespace Pretzel.Logic.Minification
{
    public class JsMinifier
    {
        private readonly IFileSystem fileSystem;
        private readonly IEnumerable<FileInfo> files;
        private readonly string outputPath;

        public JsMinifier(IFileSystem fileSystem, IEnumerable<FileInfo> files, string outputPath)
        {
            this.files = files;
            this.outputPath = outputPath;
            this.fileSystem = fileSystem;
        }

        public void Minify()
        {
            var minifer = new Minifier();
            var codeSettings = new CodeSettings();

            var content = fileSystem.BundleFiles(files);
            var minified =  minifer.MinifyJavaScript(content, codeSettings);

            fileSystem.File.WriteAllText(outputPath, minified);
        }
    }
}
