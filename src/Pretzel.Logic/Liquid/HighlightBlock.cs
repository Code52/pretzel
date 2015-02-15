using DotLiquid;
using System.IO;

namespace Pretzel.Logic.Liquid
{
    public class HighlightBlock : Block
    {
        public override void Render(Context context, TextWriter result)
        {
            var markup = Markup.Trim();
            var addCode = !string.IsNullOrEmpty(markup);

            result.Write("<pre>");
            if (addCode)
            {
                result.Write("<code class=\"language-{0}\">", Markup.Trim());
            }
            base.Render(context, result);
            if (addCode)
            {
                result.Write("</code>", Markup);
            }
            result.Write("</pre>");
        }
    }
}
