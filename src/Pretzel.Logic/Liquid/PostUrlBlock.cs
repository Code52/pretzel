using System;
using System.Collections.Generic;
using System.IO;
using DotLiquid;

namespace Pretzel.Logic.Liquid
{
    public class PostUrlBlock : Block
    {
        private string postFileName;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);
            postFileName = markup;
        }

        public override void Render(Context context, TextWriter result)
        {
            var permalink = postFileName.Replace(".md", "");
            permalink = permalink.Replace(".mdown", "");
            permalink = permalink.Replace("-", "/");
            permalink += ".html";
            
            result.Write(permalink);
        }
    }
}
