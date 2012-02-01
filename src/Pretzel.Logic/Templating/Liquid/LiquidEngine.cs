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

            foreach (var file in fileSystem.Directory.GetFiles(folder))
            {
                var fileName = Path.GetFileName(file);
                var newPath = Path.Combine(outputPath, fileName);
                fileSystem.File.WriteAllText(newPath, fileSystem.File.ReadAllText(file));
            }
        }
    }
}
