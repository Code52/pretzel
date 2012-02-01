using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Templating.Jekyll
{
    public class JekyllEngine : ISiteEngine
    {
        private static readonly Markdown Markdown = new Markdown();
        private SiteContext context;
        private IFileSystem fileSystem;

        public void Process()
        {
            var outputDirectory = Path.Combine(context.Folder, "_site");
            fileSystem.Directory.CreateDirectory(outputDirectory);

            foreach (var file in fileSystem.Directory.GetFiles(context.Folder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(context.Folder, "");
                if (relativePath.StartsWith("_")) continue;
                if (relativePath.StartsWith(".")) continue;

                var extension = Path.GetExtension(file);
                var outputFile = Path.Combine(outputDirectory, relativePath);

                var inputFile = fileSystem.File.ReadAllText(file);
                if (extension.IsMarkdownFile())
                {
                    outputFile = outputFile.Replace(extension, ".html");

                    var pageContext = ProcessMarkdownPage(inputFile, outputFile, outputDirectory);

                    fileSystem.File.WriteAllText(pageContext.OutputPath, pageContext.Content);
                }
                else
                {
                    RenderTemplate(inputFile, outputFile);
                }
            }
        }

        private PageContext ProcessMarkdownPage(string inputFile, string outputPath, string outputDirectory)
        {
            var metadata = inputFile.YamlHeader();
            var pageContext = PageContext.FromDictionary(metadata, outputDirectory, outputPath);
            pageContext.Content = Markdown.Transform(inputFile.ExcludeHeader());

            while (metadata.ContainsKey("layout"))
            {
                var path = Path.Combine(context.Folder, "_layouts", metadata["layout"] + ".html");

                if (!fileSystem.File.Exists(path)) 
                    continue;

                metadata = ProcessTemplate(pageContext, path);
            }
            return pageContext;
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = fileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            var data = FromAnonymousObject(context, pageContext);
            pageContext.Content = RenderTemplate(templateContent, data);
            return metadata;
        }

        private void RenderTemplate(string inputFile, string outputPath)
        {
            var data = FromAnonymousObject(context);
            var template = Template.Parse(inputFile);
            var output = template.Render(data);
            fileSystem.File.WriteAllText(outputPath, output);
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
            this.fileSystem = fileSystem;
            this.context = context;
        }
    }
}
