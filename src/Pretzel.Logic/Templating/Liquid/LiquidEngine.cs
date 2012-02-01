using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Templating.Liquid
{
    public class Site
    {
        public string Title { get; set; }
    }

    public class LiquidEngine : ITemplateEngine
    {
        private static readonly Markdown markdown = new Markdown();

        public void Process(IFileSystem fileSystem, string folder, Site site)
        {
            var outputPath = Path.Combine(folder, "_site");
            fileSystem.Directory.CreateDirectory(outputPath);

            foreach (var file in fileSystem.Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(folder, "");
                if (relativePath.StartsWith("_")) continue;

                Hash data = null;
                if (site != null)
                {
                    data = Hash.FromAnonymousObject(new { page = new { title = site.Title } });
                }

                var extension = Path.GetExtension(file);
                var newPath = Path.Combine(outputPath, relativePath);

                var inputFile = fileSystem.File.ReadAllText(file);
                var output = "";
                if (extension.IsMarkdownFile())
                {
                    newPath = newPath.Replace(extension, ".html");

                    var metadata = inputFile.YamlHeader();
                    if (metadata.ContainsKey("permalink"))
                    {
                        newPath = Path.Combine(outputPath, metadata["permalink"].ToString().ToRelativeFile());
                    }

                    if (metadata.ContainsKey("layout"))
                    {
                        var path = Path.Combine(folder, "_layouts", metadata["layout"] + ".html");

                        if (fileSystem.File.Exists(path))
                        {
                            var templateFile = fileSystem.File.ReadAllText(path);
                            var fileContents = markdown.Transform(inputFile.ExcludeHeader());

                            data = Hash.FromAnonymousObject(new {page = new {title = site.Title}, content = fileContents});

                            var template = Template.Parse(templateFile);
                            output = template.Render(data);
                        }
                    }
                    fileSystem.File.WriteAllText(newPath, output);
                }
                else
                {
                    var template = Template.Parse(inputFile);
                    output = template.Render(data);
                    fileSystem.File.WriteAllText(newPath, output);
                }
            }
        }
    }
}
