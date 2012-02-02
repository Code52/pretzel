using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Templating.Jekyll
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "jekyll")]
    public class JekyllEngine : ISiteEngine
    {
        private static readonly Markdown Markdown = new Markdown();
        private SiteContext context;

        [Import]
        public IFileSystem FileSystem { get; set; }

        public void Process(SiteContext siteContext)
        {
            context = siteContext;

            var outputDirectory = Path.Combine(context.Folder, "_site");
            FileSystem.Directory.CreateDirectory(outputDirectory);

            foreach (var file in FileSystem.Directory.GetFiles(context.Folder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(context.Folder, "").TrimStart('\\');
                if (relativePath.StartsWith("_")) continue;
                if (relativePath.StartsWith(".")) continue;

                var outputFile = Path.Combine(outputDirectory, relativePath);

                var directory = Path.GetDirectoryName(outputFile);
                if (!FileSystem.Directory.Exists(directory))
                    FileSystem.Directory.CreateDirectory(directory);

                var extension = Path.GetExtension(file);
                if (extension.IsImageFormat())
                {
                    FileSystem.File.Copy(file, outputFile, true);
                    continue;
                }

                var inputFile = FileSystem.File.ReadAllText(file);
                if (!inputFile.StartsWith("---"))
                {
                    FileSystem.File.WriteAllText(outputFile, inputFile);
                    continue;
                }

                // TODO: refine this step
                // markdown file should not be treated differently
                // output from markdown file should be sent to template pipeline
                
                if (extension.IsMarkdownFile())
                {
                    outputFile = outputFile.Replace(extension, ".html");

                    var pageContext = ProcessMarkdownPage(inputFile, outputFile, outputDirectory);

                    FileSystem.File.WriteAllText(pageContext.OutputPath, pageContext.Content);
                }
                else
                {
                    RenderTemplate(inputFile.ExcludeHeader(), outputFile);
                }
            }
        }

        private PageContext ProcessMarkdownPage(string fileContents, string outputPath, string outputDirectory)
        {
            var metadata = fileContents.YamlHeader();
            var pageContext = PageContext.FromDictionary(metadata, outputDirectory, outputPath);
            pageContext.Content = Markdown.Transform(fileContents.ExcludeHeader());

            while (metadata.ContainsKey("layout"))
            {
                var path = Path.Combine(context.Folder, "_layouts", metadata["layout"] + ".html");

                if (!FileSystem.File.Exists(path)) 
                    continue;

                metadata = ProcessTemplate(pageContext, path);
            }
            return pageContext;
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = FileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            var data = CreatePageData(context, pageContext);
            pageContext.Content = RenderTemplate(templateContent, data);
            return metadata;
        }

        private void RenderTemplate(string inputFile, string outputPath)
        {
            var data = CreatePageData(context);
            var template = Template.Parse(inputFile);
            var output = template.Render(data);
            FileSystem.File.WriteAllText(outputPath, output);
        }

        private static Hash CreatePageData(SiteContext context, PageContext pageContext)
        {
            var title = string.IsNullOrWhiteSpace(pageContext.Title) ? context.Title : pageContext.Title;

            return Hash.FromAnonymousObject(new { page = new { title }, content = pageContext.Content });
        }

        private static Hash CreatePageData(SiteContext context)
        {
            return Hash.FromAnonymousObject(new { page = new { title = context.Title } });
        }

        private static string RenderTemplate(string templateContents, Hash data)
        {
            var template = Template.Parse(templateContents);
            return template.Render(data);
        }

        public bool CanProcess(string directory)
        {
            var configPath = Path.Combine(directory, "_config.yml");
            return FileSystem.File.Exists(configPath);
        }

        public void Initialize()
        {
            // Template.RegisterTag<RenderTime>("render_time");
        }
    }
}
