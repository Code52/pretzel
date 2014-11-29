using System.IO;
using DotLiquid;

namespace Pretzel.Logic.Liquid
{
    public class CommentBlock : Block
    {
        public override void Render(Context context, TextWriter result)
        {
            // Do nothing (i.e. swallow the comment...)
        }
    }
}
