using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Templating.Context
{
    public class PageContext
    {
        private string _content;

        public PageContext(SiteContext context, Page page)
        {
            Site = context;
            Page = page;
        }

        public PageContext(PageContext context)
        {
            Title = context.Title;
            OutputPath = context.OutputPath;
            Bag = new Dictionary<string, object>(context.Bag);
            _content = context.Content;
            FullContent = context.Content;
            Site = context.Site;
            Page = context.Page;
            Previous = context.Previous;
            Next = context.Next;
            Paginator = context.Paginator;
        }

        public string Title { get; set; }

        public string OutputPath { get; set; }

        public IDictionary<string, object> Bag { get; set; }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                Page.Content = value;
            }
        }

        public SiteContext Site { get; private set; }

        public Page Page { get; set; }

        public Page Previous { get; set; }

        public Page Next { get; set; }

        public Paginator Paginator { get; set; }

        public bool Comments
        {
            get { return Bag.ContainsKey("comments") && bool.Parse(Bag["comments"].ToString()); }
        }

        public string FullContent { get; set; }

        public static PageContext FromPage(SiteContext siteContext, Page page, string outputPath, string defaultOutputPath)
        {
            var context = new PageContext(siteContext, page);

            if (page.Bag.ContainsKey("permalink") || siteContext.Config.ContainsKey("permalink"))
            {
                context.OutputPath = Path.Combine(outputPath, page.Url.ToRelativeFile());
            }
            else
            {
                context.OutputPath = defaultOutputPath;
                page.Bag.Add("permalink", page.File);
            }

            if (context.OutputPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                context.OutputPath = Path.Combine(context.OutputPath, "index.html");
            }

            page.OutputFile = context.OutputPath;

            if (page.Bag.ContainsKey("title"))
            {
                context.Title = page.Bag["title"].ToString();
            }

            if (string.IsNullOrEmpty(context.Title))
            {
                context.Title = siteContext.Title;
            }

            context.Content = page.Content;
            context.FullContent = page.Content;
            context.Bag = page.Bag;
            context.Bag["id"] = page.Id;
            context.Bag["url"] = page.Url;
            return context;
        }
    }
}
