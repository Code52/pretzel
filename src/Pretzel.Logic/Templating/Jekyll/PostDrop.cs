using DotLiquid;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Jekyll.Liquid
{
    // Seems unnecessary, see Given_Page_And_Posts_Have_Custom_Metadatas test
    public class PostDrop : Drop
    {
        private readonly Page page;

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