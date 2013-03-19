using System.IO;
using DotLiquid;

namespace Pretzel.Logic.Liquid
{
    public class HighlightBlock : Block
    {
        public override void Render(Context context, TextWriter result)
        {
            result.Write("<pre>");
            base.Render(context, result);
            result.Write("</pre>");
        }
    }
}
