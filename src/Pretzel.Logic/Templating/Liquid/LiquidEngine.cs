using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic.Templating.Liquid
{
    public class Site
    {
        public string Title { get; set; }
    }

    public class LiquidEngine : ITemplateEngine
    {
        public void Process(IFileSystem fileSystem, string folder, Site site)
        {
            var outputPath = Path.Combine(folder, "_site");
            fileSystem.Directory.CreateDirectory(outputPath);

            foreach (var file in fileSystem.Directory.GetFiles(folder, "*.*",SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(folder, "");
                var newPath = Path.Combine(outputPath, relativePath);

                var template = DotLiquid.Template.Parse(fileSystem.File.ReadAllText(file));

                var output = template.Render();

                fileSystem.File.WriteAllText(newPath, output);
            }
        }
    }
}
