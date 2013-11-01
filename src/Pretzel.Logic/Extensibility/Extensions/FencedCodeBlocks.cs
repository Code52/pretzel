using System.Text.RegularExpressions;
using Pygments;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class FencedCodeBlocks : IContentTransform
    {
        private static readonly Regex FencedCodeBlokRegex = new Regex(@"(?:\n|\r|\r\n|\A)(`{3,})(?<lexer>[a-zA-Z0-9_-]+)\s*(\n|\r|\r\n)(?<code>(?>(?!\1\s*(\n|\r|\r\n)).*(\n|\r|\r\n)+)+)\1\s*(\n|\r|\r\n)*", 
            RegexOptions.IgnorePatternWhitespace);
        private static readonly Highlighter Highlighter = new Highlighter();

        public string Transform(string content)
        {
            return FencedCodeBlokRegex.Replace(content, match =>
            {
                var lexerName = match.Groups["lexer"].Value;
                var code = match.Groups["code"].Value;

                return Highlighter.HighlightToHtml(code, lexerName, "vs", fragment: true);
            });
        }
    }
}