using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Jekyll.Liquid
{
    public class SiteContextDrop : Drop
    {
        private readonly SiteContext context;
        private IList<Hash> posts;
        public DateTime Time
        {
            get { return context.Time; }
        }

        public IList<Hash> Posts
        {
            get { return posts ?? (posts = context.Posts.Select(ToHash).ToList()); }
        }

        public string Title
        {
            get { return context.Title; }
        }

        public SiteContextDrop(SiteContext context)
        {
            this.context = context;
        }

        private Hash ToHash(Page page)
        {
            var p = Hash.FromDictionary(page.Bag);
            p.Add("Content", page.Content);
            return p;
        }
    }
}