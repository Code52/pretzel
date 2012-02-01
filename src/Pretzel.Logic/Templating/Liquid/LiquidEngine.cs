using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic.Templating.Liquid
{
    public class LiquidEngine : ITemplateEngine
    {
        public void Process(IFileSystem fileSystem, string folder)
        {
            var outputPath = Path.Combine(folder, "_site");
            fileSystem.Directory.CreateDirectory(outputPath);

            foreach (var file in fileSystem.Directory.GetFiles(folder, "*.*",SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(folder, "");
                var newPath = Path.Combine(outputPath, relativePath);
                fileSystem.File.WriteAllText(newPath, fileSystem.File.ReadAllText(file));
            }
        }
    }
}
