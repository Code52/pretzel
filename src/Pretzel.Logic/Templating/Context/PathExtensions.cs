using System;
using System.Linq;

namespace Pretzel.Logic.Templating.Context
{
    public static class PathExtensions
    {
        private static readonly string[] MarkdownFiles = new[] { ".md", ".mkd", ".mkdn", ".mdown", ".markdown" };
        private static readonly string[] ImageFiles = new[] { ".png", ".gif", ".jpg" };

        public static bool IsMarkdownFile(this string extension)
        {
            return MarkdownFiles.Contains(extension.ToLower());
        }

        public static bool IsRazorFile(this string extension)
        {
            return string.Equals(extension, ".cshtml", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string ToRelativeFile(this string path)
        {
            return path.Replace(@"/", @"\").TrimStart('\\');
        }

        public static bool IsImageFormat(this string extension)
        {
            return ImageFiles.Contains(extension.ToLower());
        }
    }
}
