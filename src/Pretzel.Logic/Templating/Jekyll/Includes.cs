using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic.Templating.Jekyll
{
    public class Includes : DotLiquid.FileSystems.IFileSystem
    {
        private IFileSystem _fileSystem;

        public string Root { get; set; }

        public Includes(string root, IFileSystem fileSystem)
        {
            Root = root;
            _fileSystem = fileSystem;
        }

        public string ReadTemplateFile(DotLiquid.Context context, string templateName)
        {
            var include = Path.Combine(Root, "_includes", templateName);
            if (_fileSystem.File.Exists(include))
                return _fileSystem.File.ReadAllText(include);
            return string.Empty;
        }
    }
}