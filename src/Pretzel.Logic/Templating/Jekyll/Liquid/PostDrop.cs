using DotLiquid;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Jekyll.Liquid
{
    public class PostDrop : Drop
    {
        private readonly Page page;
        private readonly string content;

        public PostDrop(Page page)
        {
            this.page = page;
        }

        public string Title
        {
            get { return page.Title; }
        }

        public string Content
        {
            get { return page.Content; }
        }
    }
}