using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Minification;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Liquid;

namespace Pretzel.Logic.Templating.Jekyll
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "jekyll")]
    public class JekyllEngine : ISiteEngine
    {
        private SiteContext context;
        private SiteContextDrop contextDrop;

        [Import] 
        public IFileSystem FileSystem { get; set; }
        [Import] 
        FileTransforms transforms;

        public void Process(SiteContext siteContext)
        {
            context = siteContext;
            contextDrop = new SiteContextDrop(context);

            var outputDirectory = Path.Combine(context.SourceFolder, "_site");

            foreach (var p in siteContext.Posts)
            {
                ProcessFile(outputDirectory, p, p.Filepath);
            }

            foreach (var p in siteContext.Pages)
            {
                ProcessFile(outputDirectory, p);
            }
        }

        public string GetOutputDirectory(string path)
        {
            return Path.Combine(path, "_site");
        }

        private void ProcessFile(string outputDirectory, Page page, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            var outputFile = Path.Combine(outputDirectory, relativePath);

            var directory = Path.GetDirectoryName(outputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);

            var extension = Path.GetExtension(page.File);
            if (transforms.CanProcess(extension))
            {
                var contents = transforms.Process(page.File);
                outputFile = transforms.MapFile(page.File);
                FileSystem.File.WriteAllText(outputFile, contents);
                return;
            }
            
            if (extension.IsImageFormat())
            {
                FileSystem.File.Copy(page.File, outputFile, true);
                return;
            }

            if (page is NonProcessedPage)
            {
                FileSystem.File.Copy(page.File, outputFile, true);
                return;
            }

            if (extension.IsMarkdownFile())
                outputFile = outputFile.Replace(extension, ".html");

            var pageContext = PageContext.FromPage(page, outputDirectory, outputFile);
            var metadata = page.Bag;
            while (metadata.ContainsKey("layout"))
            {
                if (metadata["layout"] == "nil" || metadata["layout"] == null)
                    break;

                var path = Path.Combine(context.SourceFolder, "_layouts", metadata["layout"] + ".html");

                if (!FileSystem.File.Exists(path))
                    break;

                metadata = ProcessTemplate(pageContext, path);
            }

            pageContext.Content = RenderTemplate(pageContext.Content, CreatePageData(pageContext));

            FileSystem.File.WriteAllText(pageContext.OutputPath, pageContext.Content);
        }

        private string MapToOutputPath(string file)
        {
            return file.Replace(context.SourceFolder, "").TrimStart('\\');
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = FileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            var data = CreatePageData(pageContext);
            pageContext.Content = RenderTemplate(templateContent, data);
            return metadata;
        }
        
        private Hash CreatePageData(PageContext pageContext)
        {
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
                site = contextDrop,
                page = y,
                content = pageContext.Content
            });

            return x;
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
