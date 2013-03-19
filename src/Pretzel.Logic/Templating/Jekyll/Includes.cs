using System.IO;

namespace Pretzel.Logic.Templating.Jekyll
{
    public class Includes : DotLiquid.FileSystems.IFileSystem
    {
        public string Root { get; set; }

        public Includes(string root)
        {
            Root = root;
        }

        public string ReadTemplateFile(DotLiquid.Context context, string templateName)
        {
            var include = Path.Combine(Root, "_includes", templateName);
            if (File.Exists(include))
                return File.ReadAllText(include);
            return string.Empty;
        }
    }
}