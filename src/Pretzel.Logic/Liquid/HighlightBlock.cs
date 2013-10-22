using System.CodeDom.Compiler;
using System.IO;
using DotLiquid;

namespace Pretzel.Logic.Liquid
{
    public class HighlightBlock : Block
    {
        public override void Render(Context context, TextWriter result)
        {
            var colorizer = new ColorCode.CodeColorizer();
            var language = ParseLanguage(this.Markup.Trim());

            if (language == null)
            {
                result.Write("<pre><code>");
                base.Render(context, result);
                result.Write("</code></pre>");
            }
            else
            {
                colorizer.Colorize(this.NodeList[0].ToString(), language, result);
            }
        }

        public ColorCode.ILanguage ParseLanguage(string language)
        {
            switch (language.ToLower())
            {
                //Languages not in pygments
                case "asax":
                    return ColorCode.Languages.Asax;
                case "ashx":
                    return ColorCode.Languages.Ashx;
                case "aspx":
                    return ColorCode.Languages.Aspx;

                //Default pygments shortnames: http://pygments.org/docs/lexers/
                case "aspx-cs":
                    return ColorCode.Languages.AspxCs;
                case "aspx-vb":
                    return ColorCode.Languages.AspxVb;
                case "c#":
                case "csharp":
                    return ColorCode.Languages.CSharp;
                case "cpp":
                case "c++":
                    return ColorCode.Languages.Cpp;
                case "css":
                    return ColorCode.Languages.Css;
                case "html":
                    return ColorCode.Languages.Html;
                case "java":
                    return ColorCode.Languages.Java;
                case "js":
                case "javascript":
                    return ColorCode.Languages.JavaScript;
                case "php":
                case "php3":
                case "php4":
                case "php5":
                    return ColorCode.Languages.Php;
                case "powershell":
                case "posh":
                case "ps1":
                case "psm1":
                    return ColorCode.Languages.PowerShell;
                case "sql":
                    return ColorCode.Languages.Sql;
                case "vb.net":
                case "vbnet":
                    return ColorCode.Languages.VbDotNet;
                case "xml":
                    return ColorCode.Languages.Xml;

                //Will cause the codeblock to be wrapped in <pre><code>
                default:
                    return null;
            }
        }
    }
}
