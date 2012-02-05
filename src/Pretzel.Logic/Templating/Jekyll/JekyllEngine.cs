using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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
            context.Posts = new List<Page>();

            var outputDirectory = Path.Combine(context.Folder, "_site");
            Tracing.Debug(string.Format("generating the site contents at {0}", outputDirectory));
            FileSystem.Directory.CreateDirectory(outputDirectory);

            IDictionary<string, string> posts = new Dictionary<string, string>();

            var postsFolder = Path.Combine(context.Folder, "_posts");
            if (FileSystem.Directory.Exists(postsFolder))
            {
                foreach (var file in FileSystem.Directory.GetFiles(postsFolder, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = GetPathWithTimestamp(outputDirectory, file);
                    posts.Add(file, relativePath);

                    // TODO: more parsing of data
                    var contents = FileSystem.File.ReadAllText(file);
                    var header = contents.YamlHeader();
                    var post = new Page
                                   {
                                       Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post",
                                       Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                                       Content = Markdown.Transform(contents.ExcludeHeader())
                                   };
                    context.Posts.Add(post);
                }

                context.Posts = context.Posts.OrderByDescending(p => p.Date).ToList();
                foreach (var p in posts)
                {
                    ProcessFile(outputDirectory, p.Key, p.Value);
                }
            }

            foreach (var file in FileSystem.Directory.GetFiles(context.Folder, "*.*", SearchOption.AllDirectories))
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

        private string GetPathWithTimestamp(string outputDirectory, string file)
        {
            // TODO: detect mode from site config
            var fileName = file.Substring(file.LastIndexOf("\\"));

            var tokens = fileName.Split('-');
            var timestamp = string.Join("\\", tokens.Take(3)).Trim('\\');
            var title = string.Join("-", tokens.Skip(3));
            return Path.Combine(outputDirectory, timestamp, title);
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
            return file.Replace(context.Folder, "").TrimStart('\\');
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
            Template.FileSystem = new Includes(context.Folder);

            var output = template.Render(data);
            var x = template.Errors;
            FileSystem.File.WriteAllText(outputPath, output);
        }

        private static Hash CreatePageData(SiteContext context, PageContext pageContext)
        {
            var title = string.IsNullOrWhiteSpace(pageContext.Title) ? context.Title : pageContext.Title;

            var drop = new SiteContextDrop(context);

            return Hash.FromAnonymousObject(new
            {
                site = drop,
                page = new { title },
                content = pageContext.Content
            });
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
            Template.FileSystem = new Includes(context.Folder);

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
