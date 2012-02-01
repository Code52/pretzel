using System.Linq;

namespace Pretzel.Logic.Templating.Liquid
{
    public static class LiquidExtensions
    {
        private static readonly string[] markdownFiles = new[] { ".md", ".mdown", ".markdown" };

        public static bool IsMarkdownFile(this string extension)
        {
            return markdownFiles.Contains(extension.ToLower());
        }
    }
}
