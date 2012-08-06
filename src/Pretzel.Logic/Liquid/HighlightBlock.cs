using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotLiquid;

namespace Pretzel.Logic.Liquid
{
    public class HighlightBlock : DotLiquid.Block
    {
        public override void Render(Context context, TextWriter result)
        {
            result.Write("<pre>");
            base.Render(context, result);
            result.Write("</pre>");
        }
    }
}
