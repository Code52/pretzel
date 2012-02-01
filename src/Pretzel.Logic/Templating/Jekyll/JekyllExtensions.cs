using System.Linq;

namespace Pretzel.Logic.Templating.Jekyll
{
    public static class JekyllExtensions
    {
        private static readonly string[] MarkdownFiles = new[] { ".md", ".mdown", ".markdown" };

        public static bool IsMarkdownFile(this string extension)
        {
            return MarkdownFiles.Contains(extension.ToLower());
        }

        public static string ToRelativeFile(this string path)
        {
            return path.Replace(@"/", @"\").TrimStart('\\');
        }
    }
}
