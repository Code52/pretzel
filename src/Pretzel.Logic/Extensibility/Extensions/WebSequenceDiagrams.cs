using System.Text.RegularExpressions;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class WebSequenceDiagrams : IContentTransform
    {
        static readonly Regex SequenceDiagramRegex = new Regex(@"(?s:<pre><code>@@sequence(?<style>.*?)\r?\n(?<sequenceContent>.*?)</code></pre>)");
        const string Style_Template = " wsd_style=\"{0}\"";
        const string Div_Template = "<div class=\"wsd\"{1}><pre>{0}</pre></div>";
        const string JS_Script = "\r\n<script type=\"text/javascript\" src=\"http://www.websequencediagrams.com/service.js\"></script>";

        public string Transform(string content)
        {
            var contentIncludesASequenceDiagram = false;
            content = SequenceDiagramRegex.Replace(content, match =>
            {
                contentIncludesASequenceDiagram = true;
                var sequenceContent = match.Groups["sequenceContent"].Value;
                var styleGroup = match.Groups["style"];
                string style = "default";
                if (styleGroup.Success && !string.IsNullOrWhiteSpace(styleGroup.Value))
                {
                    style = styleGroup.Value.Trim();
                }

                return string.Format(Div_Template, sequenceContent, string.Format(Style_Template, style));
            });

            if (contentIncludesASequenceDiagram)
            { 
                content += JS_Script;
            }

            return content;
        }
    }
}