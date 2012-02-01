using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions;
using System.IO;
using Microsoft.Ajax.Utilities;

namespace Pretzel.Logic.Minification
{
    public class JsMinifier
    {
        private readonly IFileSystem _fileSystem;
        private IEnumerable<FileInfo> _files;
        private string _outputPath;

        public JsMinifier(IFileSystem fileSystem, IEnumerable<FileInfo> files, string outputPath)
        {
            _files = files;
            _outputPath = outputPath;
            _fileSystem = fileSystem;
        }

        public void Minify()
        {
            var minifer = new Minifier();
            var codeSettings = new CodeSettings();

            var content = _fileSystem.BundleFiles(_files);
            var minified =  minifer.MinifyJavaScript(content, codeSettings);

            _fileSystem.File.WriteAllText(_outputPath, minified);
        }
    }
}
