using System.Collections.Generic;
using System.IO;

// ReSharper disable CheckNamespace
namespace Pretzel.Logic.Templating.Context
{
    public class PageContext
    {
        public PageContext(SiteContext context, Page page)
        {
            Site = context;
            Page = page;
        }

        public PageContext(PageContext context)
        {
            Title = context.Title;
            OutputPath = context.OutputPath;
            Bag = context.Bag;
            Content = context.Content;
            Site = context.Site;
            Page = context.Page;
            Previous = context.Previous;
            Next = context.Next;
            Paginator = context.Paginator;
        }

        public string Title { get; set; }
        public string OutputPath { get; set; }
        public IDictionary<string, object> Bag { get; set; }
        public string Content { get; set; }
        public SiteContext Site { get; private set; }
        public Page Page { get; set; }
        public Page Previous { get; set; }
        public Page Next { get; set; }
        public Paginator Paginator { get; set; }

        public bool Comments
        {
            get { return Bag.ContainsKey("comments") && bool.Parse(Bag["comments"].ToString()); }
        }
    }
}