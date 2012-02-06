using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Jekyll.Liquid;

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
            var outputDirectory = Path.Combine(context.SourceFolder, "_site");

            foreach (var p in siteContext.Posts)
            {
                ProcessFile(outputDirectory, p.File, p.Filepath);
            }

            foreach (var file in FileSystem.Directory.GetFiles(context.SourceFolder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = MapToOutputPath(file);
                if (relativePath.StartsWith("_")) continue;
                if (relativePath.StartsWith(".")) continue;

                ProcessFile(outputDirectory, file, relativePath);
            }
        }

        public string GetOutputDirectory(string path)
        {
            return Path.Combine(path, "_site");
        }

        private void ProcessFile(string outputDirectory, string file, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(file);

            var outputFile = Path.Combine(outputDirectory, relativePath);

            var directory = Path.GetDirectoryName(outputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);

            var extension = Path.GetExtension(file);
            if (extension.IsImageFormat())
            {
                FileSystem.File.Copy(file, outputFile, true);
                return;
            }

            var inputFile = FileSystem.File.ReadAllText(file);
            if (!inputFile.StartsWith("---"))
            {
                FileSystem.File.WriteAllText(outputFile, inputFile);
                return;
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

        private string MapToOutputPath(string file)
        {
            return file.Replace(context.SourceFolder, "").TrimStart('\\');
        }

        private PageContext ProcessMarkdownPage(string fileContents, string outputPath, string outputDirectory)
        {
            var metadata = fileContents.YamlHeader();
            var pageContext = PageContext.FromDictionary(metadata, outputDirectory, outputPath);
            pageContext.Content = Markdown.Transform(fileContents.ExcludeHeader());

            var data = CreatePageData(context, pageContext);
            pageContext.Content = RenderTemplate(pageContext.Content, data);

            while (metadata.ContainsKey("layout"))
            {
                var path = Path.Combine(context.SourceFolder, "_layouts", metadata["layout"] + ".html");

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
            Template.FileSystem = new Includes(context.SourceFolder);

            var output = template.Render(data);
            var x = template.Errors;
            FileSystem.File.WriteAllText(outputPath, output);
        }

        private static Hash CreatePageData(SiteContext context, PageContext pageContext)
        {
            var drop = new SiteContextDrop(context);
            var y = Hash.FromDictionary(pageContext.Bag);

            if (y.ContainsKey("title"))
            {
                if (string.IsNullOrWhiteSpace(y["title"].ToString()))
                {
                    y["title"] = context.Title;
                }
            }
            else
            {
                y.Add("title", context.Title);
            }
            
            var x = Hash.FromAnonymousObject(new
            {
                site = drop,
                page = y,
                content = pageContext.Content
            });

            return x;
        }

        private static Hash CreatePageData(SiteContext context)
        {
            var drop = new SiteContextDrop(context);

            return Hash.FromAnonymousObject(new
            {
                site = drop,
                page = new { title = context.Title }
            });
        }

        private string RenderTemplate(string templateContents, Hash data)
        {
            var template = Template.Parse(templateContents);
            Template.FileSystem = new Includes(context.SourceFolder);

            return template.Render(data);
        }

        public bool CanProcess(string directory)
        {
            var configPath = Path.Combine(directory, "_config.yml");
            return FileSystem.File.Exists(configPath);
        }

        public void Initialize()
        {
            //Template.RegisterTag<RenderTime>("render_time");
            //Template.RegisterTag<TagCloud>("tag_cloud");
        }
    }
}
