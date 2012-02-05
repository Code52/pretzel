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

        public DateTime Time
        {
            get { return context.Time; }
        }

        public IList<PostDrop> Posts
        {
            get { return context.Posts.Select(p => new PostDrop(p)).ToList(); }
        }

        public string Title
        {
            get { return context.Title; }
        }

        public SiteContextDrop(SiteContext context)
        {
            this.context = context;
        }
    }
}