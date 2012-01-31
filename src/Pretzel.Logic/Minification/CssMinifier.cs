using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using dotless.Core;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Minification
{
    public class CssMinifier
    {
        private readonly static Regex IMPORT_PATTERN = new Regex(@"@import +url\(([""']){0,1}(.*?)\1{0,1}\);", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IFileSystem _fileSystem;
        private IEnumerable<FileInfo> _files;
        private string _outputPath;

        public CssMinifier(IFileSystem fileSystem, IEnumerable<FileInfo> files, string outputPath)
        {
            _files = files;
            _outputPath = outputPath;
            _fileSystem = fileSystem;
        }

        public void Minify()
        {
            var bundled = bundleFiles(_files);

            //todo resolve imports
            //_fileSystem.Directory.SetCurrentDirectory("");
            
            //minify
            var engineFactory = new EngineFactory();
            engineFactory.Configuration.MinifyOutput = true;

            var engine = engineFactory.GetEngine();
            var minified = engine.TransformToCss(bundled, _files.First().FullName);

            //build file
            _fileSystem.File.WriteAllText(_outputPath, minified);
        }

        public string ProcessCss(FileInfo file)
        {
            var content = ReadFile(file.FullName);

            //todo resolve imports
            //_fileSystem.Directory.SetCurrentDirectory("");

            var engineFactory = new EngineFactory();
            engineFactory.Configuration.MinifyOutput = true;

            var engine = engineFactory.GetEngine();
            
            return engine.TransformToCss(content, file.FullName);
        }

        private string bundleFiles(IEnumerable<FileInfo> filePaths)
        {
            var outputCss = new StringBuilder();

            filePaths.Select(file => ReadFile(file.FullName))
                .Aggregate(outputCss, (builder, val) => builder.Append(val + "\n"));

            return outputCss.ToString();
        }

        private string ReadFile(string filepath)
        {
            return _fileSystem.File.ReadAllText(filepath);
        }
    }
}
