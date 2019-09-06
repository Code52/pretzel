using DotLiquid;
using System.Collections.Generic;
using System.IO;
using Pretzel.Logic.Templating.Context;
using System.Linq;
using Pretzel.Logic.Exceptions;
using System.Composition;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class PostUrlTag : DotLiquid.Tag, ITag
    {
        private string _postFileName;
        private readonly SiteContext _siteContext;

        public new string Name { get { return "PostUrl"; } }
        
        public PostUrlTag(SiteContext siteContext)
        {
            _siteContext = siteContext;
        }

        public string PostUrl(string postFileName)
        {
            // get Page
            var page = _siteContext.Posts.FirstOrDefault(p => p.File.Contains(postFileName));
            if (page == null)
            {
                page = _siteContext.Pages.FirstOrDefault(p => p.File.Contains(postFileName));
            }

            if (page == null)
            {
                throw new PageProcessingException(string.Format("PostUrl: no post/page found for '{0}'.", postFileName), null);
            }

            var url = page.Url;

            if (url.EndsWith("/"))
            {
                url += "index.html";
            }

            return url;
        }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);
            _postFileName = markup.Trim();
        }

        public override void Render(Context context, TextWriter result)
        {   
            result.Write(PostUrl(_postFileName));
        }
    }

    [Export(typeof(PostUrlTagFactory))]
    public class PostUrlTagFactory : TagFactoryBase
    {
        public PostUrlTagFactory()
            : base("PostUrl")
        { }

        public override ITag CreateTag()
        {
            return new PostUrlTag(SiteContext);
        }
    }
}
