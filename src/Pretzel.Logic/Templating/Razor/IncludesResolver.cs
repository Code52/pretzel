using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic.Templating.Razor
{
    internal class IncludesResolver : ITemplateManager
    {
        private readonly IFileSystem fileSystem;
        private readonly string includesPath;
        private readonly Dictionary<ITemplateKey, ITemplateSource> _templates = new Dictionary<ITemplateKey, ITemplateSource>();

        public IncludesResolver(IFileSystem fileSystem, string includesPath)
        {
            this.fileSystem = fileSystem;
            this.includesPath = includesPath;
        }

        public ITemplateSource Resolve(ITemplateKey key)
        {
            if (_templates.ContainsKey(key))
            {
                return _templates[key];
            }

            var templatePath = Path.Combine(includesPath, key.Name);
            var templateExists = fileSystem.File.Exists(templatePath);
            if (!templateExists)
            {
                foreach (var ext in new[] { ".cshtml", ".html", ".htm" })
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

            var template = templateExists ? fileSystem.File.ReadAllText(templatePath) : String.Empty;

            return new LoadedTemplateSource(template, null);
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            return new NameOnlyTemplateKey(name, resolveType, context);
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            if (!_templates.ContainsKey(key))
            {
                _templates.Add(key, source);
            }
        }
    }
}
