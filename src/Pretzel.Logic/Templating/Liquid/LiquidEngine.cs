using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Templating.Liquid
{
    public class SiteContext
    {
        public string Folder { get; set; }
        public string Title { get; set; }
    }

    public class PageContext
    {
        public string Title { get; set; }
        public string NewPath { get; set; }

        public string Content { get; set; }

        public static PageContext FromDictionary(IDictionary<string, object> metadata, string outputPath, string newPath)
        {
            var context = new PageContext();

            if (metadata.ContainsKey("permalink"))
            {
                context.NewPath = Path.Combine(outputPath, metadata["permalink"].ToString().ToRelativeFile());
            }
            else
            {
                context.NewPath = newPath;
            }

            if (metadata.ContainsKey("title"))
            {
                context.Title = metadata["title"].ToString();
            }

            return context;
        }


    }

    public class LiquidEngine : ITemplateEngine
    {
        private static readonly Markdown markdown = new Markdown();

        public void Process(IFileSystem fileSystem, SiteContext context)
        {
            var outputPath = Path.Combine(context.Folder, "_site");
            fileSystem.Directory.CreateDirectory(outputPath);

            foreach (var file in fileSystem.Directory.GetFiles(context.Folder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(context.Folder, "");
                if (relativePath.StartsWith("_")) continue;

                var extension = Path.GetExtension(file);
                var newPath = Path.Combine(outputPath, relativePath);

                var inputFile = fileSystem.File.ReadAllText(file);
                var output = "";
                if (extension.IsMarkdownFile())
                {
                    newPath = newPath.Replace(extension, ".html");

                    var metadata = inputFile.YamlHeader();
                    var pageContext = PageContext.FromDictionary(metadata, outputPath, newPath);

                    if (metadata.ContainsKey("layout"))
                    {
                        var path = Path.Combine(context.Folder, "_layouts", metadata["layout"] + ".html");

                        if (fileSystem.File.Exists(path))
                        {
                            var templateFile = fileSystem.File.ReadAllText(path);
                            pageContext.Content = markdown.Transform(inputFile.ExcludeHeader());
                            var templateContent = templateFile.ExcludeHeader();
                            
                            var data = FromAnonymousObject(context, pageContext);

                            output = RenderTemplate(templateContent, data);
                            var metaData = templateFile.YamlHeader();
                            if (metaData.ContainsKey("layout"))
                            {
                                var innerPath = Path.Combine(context.Folder, "_layouts", metaData["layout"] + ".html");
                                if (fileSystem.File.Exists(innerPath))
                                {
                                    var templateFileContents = fileSystem.File.ReadAllText(innerPath);
                                    pageContext.Content = output;
                                    data = FromAnonymousObject(context, pageContext);
                                    output = RenderTemplate(templateFileContents, data);
                                }
                            }
                        }
                    }

                    fileSystem.File.WriteAllText(pageContext.NewPath, output);
                }
                else
                {
                    Hash data = FromAnonymousObject(context);
                    var template = Template.Parse(inputFile);
                    output = template.Render(data);
                    fileSystem.File.WriteAllText(newPath, output);
                }
            }
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
    }
}
