using System;
using System.IO;
using System.Linq;
using System.Net;
using DotLiquid;
using Pygments;

namespace Pretzel.Logic.Liquid
{
    public class PygmentsHighlightBlock : Block
    {
        private const string LinenosToken = "linenos";

        public LineNumberStyle LineNumberStyle { get; private set; }
        public string LexerName { get; private set; }

        public override void Initialize(string tagName, string markup, System.Collections.Generic.List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);

            var arguments = markup.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            LexerName = arguments.FirstOrDefault();
            LineNumberStyle = arguments.Any(t => t == LinenosToken) ? LineNumberStyle.inline : LineNumberStyle.none;
        }

        public override void Render(Context context, TextWriter result)
        {   
            if (LexerName == null)
            {
                result.Write("<pre><code>");
                base.Render(context, result);
                result.Write("</code></pre>");
            }
            else
            {
                var decodedText = WebUtility.HtmlDecode(NodeList[0].ToString());

                var highlightedText = PygmentsHighlighter.Current.HighlightToHtml(decodedText, LexerName, "vs", lineNumberStyle: LineNumberStyle, fragment: true);
                result.Write(highlightedText);
            }
        }
    }
}