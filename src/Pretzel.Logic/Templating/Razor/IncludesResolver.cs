using System;
using System.IO;
using System.IO.Abstractions;
using RazorEngine.Templating;

namespace Pretzel.Logic.Templating.Razor
{
    internal class IncludesResolver : ITemplateResolver
    {
        private readonly IFileSystem fileSystem;
        private readonly string includesPath;

        public IncludesResolver(IFileSystem fileSystem, string includesPath)
        {
            this.fileSystem = fileSystem;
            this.includesPath = includesPath;
        }

        public string Resolve(string name)
        {
            var templatePath = Path.Combine(includesPath, name);
            var templateExists = fileSystem.File.Exists(templatePath);
            if (!templateExists)
            {
                foreach (var ext in new[] { ".cshtml", ".html", ".html" })
                {
                    var testPath = String.Concat(templatePath, ext);
                    templateExists = fileSystem.File.Exists(testPath);
                    if (templateExists)
                    {
                        templatePath = testPath;
                        break;
                    }
                }
            }

            return templateExists ? fileSystem.File.ReadAllText(templatePath) : String.Empty;
        }
    }
}