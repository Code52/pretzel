using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Import
{
    /// <summary>
    /// This one uses the HTML agility pack
    /// </summary>
    public class HtmlToMarkdownConverter
    {
        public string Convert(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            StringBuilder markdown = new StringBuilder();
            doc.LoadHtml(html);
            ProcessNodes(markdown, doc.DocumentNode.ChildNodes);
            return markdown.ToString();
        }

        private static Regex regexBr = new Regex(@"\<br\s*/?\>",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled);


        private static void ProcessNodes(StringBuilder markdown, IEnumerable<HtmlNode> htmlNodes)
        {
            foreach (var htmlNode in htmlNodes)
            {
                switch (htmlNode.Name)
                {
                    case "#text":
                        markdown.Append(htmlNode.InnerText);
                        break;
                    case "h1":
                        markdown.AppendFormat("# {0}", htmlNode.InnerText);
                        markdown.AppendLine();
                        break;
                    case "h2":
                        markdown.AppendFormat("## {0}", htmlNode.InnerText);
                        markdown.AppendLine();
                        break;
                    case "h3":
                        markdown.AppendFormat("### {0}", htmlNode.InnerText);
                        markdown.AppendLine();
                        break;
                    case "ul":
                        markdown.AppendLine();
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        markdown.AppendLine();
                        break;
                    case "li":
                        markdown.AppendLine();
                        markdown.Append("* ");
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        break;
                    case "p":
                        markdown.AppendLine();
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        markdown.AppendLine();
                        break;
                    case "b":
                    case "strong":
                        markdown.AppendFormat("**{0}**", htmlNode.InnerText);
                        break;
                    case "i":
                    case "em":
                        markdown.AppendFormat("*{0}*", htmlNode.InnerText);
                        break;
                    case "br":
                        markdown.AppendLine();
                        break;
                    case "a":
                        markdown.AppendFormat("[{0}]({1})", htmlNode.InnerText, htmlNode.Attributes["href"].Value);
                        break;
                    case "img":
                        // leave unchanged
                        markdown.Append(htmlNode.OuterHtml);
                        break;
                    case "pre":
                    case "code":
                        var code = htmlNode.InnerText;
                        // a bit hacky, but we need to sort out where lines of code end
                        code = code.Replace("\r\n", "\n");
                        code = code.Replace("\r", "\n");
                        code = regexBr.Replace(code, "\n");
                        var lines = code.Split('\n');
                        markdown.Append(Environment.NewLine + "    ");
                        markdown.Append(string.Join(Environment.NewLine + "    ", lines));
                        break;
                    default:
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        Console.WriteLine("{0}, {1} child nodes", htmlNode.Name, htmlNode.ChildNodes.Count);
                        break;
                }
            }
        }
    }
}
