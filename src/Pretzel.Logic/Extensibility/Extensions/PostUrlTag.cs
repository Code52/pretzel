using DotLiquid;
using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class PostUrlTag : Tag, ITag
    {
        private string postFileName;

        public new string Name { get { return "PostUrl"; } }

        public static string PostUrl(string input)
        {
            var permalink = input.Replace(".md", "");
            permalink = permalink.Replace(".mdown", "");
            permalink = permalink.Replace("-", "/");
            permalink += ".html";

            return permalink;
        }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);
            postFileName = markup.Trim();
        }

        public override void Render(Context context, TextWriter result)
        {
            result.Write(PostUrl(postFileName));
        }
    }
}
