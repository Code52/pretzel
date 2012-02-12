using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using HtmlAgilityPack;
using Pretzel.Logic.Templating.Context;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Parser;
using dotless.Core.Stylizers;

namespace Pretzel.Logic.Minification
{
    public class LessTransform : ITransform
    {
        #pragma warning disable 0649
        private readonly IFileSystem fileSystem;
        #pragma warning restore 0649

        [ImportingConstructor]
        public LessTransform(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        private string filePath = string.Empty;

        private ILessEngine GetEngine()
        {
            var importer = new Importer(new CustomFileReader(fileSystem, filePath));
            var parser = new Parser(new PlainStylizer(), importer);
            var engine = new LessEngine(parser);
            engine.Compress = true;
            return engine;
        }

        public void Transform(SiteContext siteContext)
        {
            var shouldCompile = new List<Page>();
            //Process to see if the site has a CSS file that doesn't exist, and should be created from LESS files.
            //This is "smarter" than just compiling all Less files, which will crash if they're part of a larger Less project 
            //ie, one file pulls in imports, individually they don't know about each other but use variables
            foreach (var file in siteContext.Pages.Where(p => p.OutputFile.EndsWith(".html")))
            {
                var doc = new HtmlDocument();
                var fileContents = fileSystem.File.ReadAllText(file.OutputFile);
                doc.LoadHtml(fileContents);

                var nodes = doc.DocumentNode.SelectNodes("/html/head/link[@rel='stylesheet']");
                if (nodes != null)
                    foreach(HtmlNode link in nodes)
                    {
                        var cssfile = link.Attributes["href"].Value;
                        if (cssfile.StartsWith("http"))
                            continue;

                        //If the file exists, ignore it
                        if (File.Exists(Path.Combine(siteContext.OutputFolder, cssfile)))
                            continue;

                        //If there is a CSS file that matches the name, ignore, could be another issue
                        if (siteContext.Pages.Any(p => p.OutputFile.Contains(cssfile)))
                            continue;


                        var n = cssfile.Replace(".css", ".less");
                        n = n.Replace('/', '\\');
                        shouldCompile.Add(siteContext.Pages.FirstOrDefault(f => f.OutputFile.Contains(n)));
                    }
            }

            foreach (var less in shouldCompile.Where(i => i != null))
            {
                filePath = less.OutputFile;
                fileSystem.File.WriteAllText(less.OutputFile.Replace(".less", ".css"), ProcessCss(less.Filepath));
            }
        }

        public string ProcessCss(string file)
        {
            var content = fileSystem.File.ReadAllText(file);
            var engine = GetEngine();
            return engine.TransformToCss(content, file);
        }

        class CustomFileReader : IFileReader
        {
            private readonly IFileSystem fileSystem;
            private readonly string currentDirectory;

            public CustomFileReader(IFileSystem fileSystem, string currentDirectory )
            {
                this.fileSystem = fileSystem;
                this.currentDirectory = Path.GetDirectoryName(currentDirectory);
            }

            public string GetFileContents(string fileName)
            {
                return fileSystem.File.ReadAllText(MapToFullPath(fileName));
            }

            public bool DoesFileExist(string fileName)
            {
                return fileSystem.File.Exists(MapToFullPath(fileName));
            }

            private string MapToFullPath(string fileName)
            {
                var fullPath = fileName;

                if (!Path.IsPathRooted(fileName))
                {
                    fullPath = Path.Combine(currentDirectory, fileName);
                }

                return fullPath;
            }
        }
    }
}
