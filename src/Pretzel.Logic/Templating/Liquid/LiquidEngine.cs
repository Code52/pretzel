using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Templating.Liquid
{
    public class LiquidEngine : ITemplateEngine
    {
        private static readonly Markdown Markdown = new Markdown();
        private SiteContext _context;
        private IFileSystem _fileSystem;

        public void Process()
        {
            var outputDirectory = Path.Combine(_context.Folder, "_site");
            _fileSystem.Directory.CreateDirectory(outputDirectory);

            foreach (var file in _fileSystem.Directory.GetFiles(_context.Folder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(_context.Folder, "");
                if (relativePath.StartsWith("_")) continue;

                var extension = Path.GetExtension(file);
                var outputPath = Path.Combine(outputDirectory, relativePath);

                var inputPath = _fileSystem.File.ReadAllText(file);
                if (extension.IsMarkdownFile())
                {
                    outputPath = outputPath.Replace(extension, ".html");

                    var pageContext = ProcessMarkdownPage(inputPath, outputPath, outputDirectory);

                    _fileSystem.File.WriteAllText(pageContext.OutputPath, pageContext.Content);
                }
                else
                {
                    RenderTemplate(inputPath, outputPath);
                }
            }
        }

        private PageContext ProcessMarkdownPage(string inputPath, string outputPath, string outputDirectory)
        {
            var metadata = inputPath.YamlHeader();
            var pageContext = PageContext.FromDictionary(metadata, outputDirectory, outputPath);
            pageContext.Content = Markdown.Transform(inputPath.ExcludeHeader());

            while (metadata.ContainsKey("layout"))
            {
                var path = Path.Combine(_context.Folder, "_layouts", metadata["layout"] + ".html");

                if (!_fileSystem.File.Exists(path)) 
                    continue;

                metadata = ProcessTemplate(pageContext, path);
            }
            return pageContext;
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = _fileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            var data = FromAnonymousObject(_context, pageContext);
            pageContext.Content = RenderTemplate(templateContent, data);
            return metadata;
        }

        private void RenderTemplate(string inputPath, string outputPath)
        {
            var data = FromAnonymousObject(_context);
            var template = Template.Parse(inputPath);
            var output = template.Render(data);
            _fileSystem.File.WriteAllText(outputPath, output);
        }

        private static Hash FromAnonymousObject(SiteContext context, PageContext pageContext)
        {
            var title = string.IsNullOrWhiteSpace(pageContext.Title) ? context.Title : pageContext.Title;

            return Hash.FromAnonymousObject(new { page = new { title }, content = pageContext.Content });
        }

        private static Hash FromAnonymousObject(SiteContext context)
        {
            return Hash.FromAnonymousObject(new { page = new { title = context.Title } });
        }

        private static string RenderTemplate(string templateFile, Hash data)
        {
            var template = Template.Parse(templateFile);
            return template.Render(data);
        }

        public void Initialize(IFileSystem fileSystem, SiteContext context)
        {
            _fileSystem = fileSystem;
            _context = context;
        }
    }
}
