using System.Text.RegularExpressions;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class WebSequenceDiagrams : IContentTransform
    {
        static readonly Regex SequenceDiagramRegex = new Regex(@"(?s:<pre><code>@@sequence(?<style>.*?)\r?\n(?<sequenceContent>.*?)</code></pre>)");

        public string Transform(string content)
        {
            var contentIncludesASequenceDiagram = false;
            content = SequenceDiagramRegex.Replace(content, match =>
            {
                contentIncludesASequenceDiagram = true;
                var sequenceContent = match.Groups["sequenceContent"].Value;
                var styleGroup = match.Groups["style"];
                string style;
                if (styleGroup.Success && string.IsNullOrWhiteSpace(styleGroup.Value))
                    style = string.Format(" wsd_style=\"{0}\"", styleGroup.Value);
                else
                    style = string.Empty;
                return string.Format("<div class=\"wsd\"{1}><pre>{0}</pre></div>", sequenceContent, style);
            });

            if (contentIncludesASequenceDiagram)
                content += "\r\n<script type=\"text/javascript\" src=\"http://www.websequencediagrams.com/service.js\"></script>";

            return content;
        }
    }
}