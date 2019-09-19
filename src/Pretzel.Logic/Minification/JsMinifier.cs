using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using NUglify;
using Pretzel.Logic.Extensions;

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
            var content = fileSystem.BundleFiles(files);
            var minified = Uglify.Js(content);
            if(minified.HasErrors)
            {
                foreach(var error in minified.Errors)
                {
                    Tracing.Error(error.ToString());
                }
            }
            fileSystem.File.WriteAllText(outputPath, minified.Code);
        }
    }
}
