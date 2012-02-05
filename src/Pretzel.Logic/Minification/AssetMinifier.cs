using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions;
using System.IO;

namespace Pretzel.Logic.Minification
{
    public class AssetMinifier
    {
        private IFileSystem _fileSystem;
        private string _siteDirectory;

        public AssetMinifier(IFileSystem fileSystem, string siteDirectory)
        {
            _fileSystem = fileSystem;
            _siteDirectory = siteDirectory;
        }

        public string MinifyTemplateAssets(string templateContent)
        {
            var minifiedTemplate = minifyCss(templateContent);
            minifiedTemplate = minifyJs(minifiedTemplate);

            throw new NotImplementedException();
        }

        private string minifyCss(string templateContent)
        {
            var outputPath = Path.Combine(_siteDirectory, @"_site\css\minified.css"); 

            //todo extract all internal css/less links from template
            var cssFiles = new List<FileInfo>();

            var cssMinifier = new CssMinifier(_fileSystem, cssFiles, outputPath);
            cssMinifier.Minify();

            //todo replace extracted links with single link to minified css (possibly with a hash appended)

            throw new NotImplementedException();
        }

        private string minifyJs(string templateContent)
        {
            throw new NotImplementedException();
        }
    }
}
