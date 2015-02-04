using System.Text;
using System.Text.RegularExpressions;
using MarkdownDeep;

namespace Pretzel.Logic.Helpers
{
    public static class MarkdownHelper
    {
        public static Markdown CreateMarkdown()
        {
            return new Markdown { FormatCodeBlock = FormatCodePrettyPrint };
        }

        // Original code: http://www.toptensoftware.com/Articles/83/MarkdownDeep-Syntax-Highlighting
        public static Regex rxExtractLanguage = new Regex("^({{(.+)}}[\r\n])", RegexOptions.Compiled);

        public static string FormatCodePrettyPrint(Markdown m, string code)
        {
            // Try to extract the language from the first line
            var match = rxExtractLanguage.Match(code);
            string language = null;

            if (match.Success)
            {
                // Save the language
                var g = (Group)match.Groups[2];
                language = g.ToString();

                // Remove the first line
                code = code.Substring(match.Groups[1].Length);
            }

            // If not specified, look for a link definition called "default_syntax" and
            // grab the language from its title
            if (language == null)
            {
                var d = m.GetLinkDefinition("default_syntax");
                if (d != null)
                    language = d.title;
            }

            // Common replacements
            if (language == "C#")
                language = "cs";
            if (language == "C++")
                language = "cpp";

            // Wrap code in pre/code tags and add PrettyPrint attributes if necessary
            if (string.IsNullOrEmpty(language))
                return string.Format("<pre><code>{0}</code></pre>\n", code);
            else
                return string.Format("<pre class=\"prettyprint lang-{0}\"><code>{1}</code></pre>\n",
                                    language.ToLowerInvariant(), code);
        }


    }
}