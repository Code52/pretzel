using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Extensions;

namespace Pretzel.Logic.Templating.Jekyll.Liquid
{
    public class SiteContextDrop : Drop
    {
        private readonly SiteContext context;
        //private IList<Hash> posts;
        public DateTime Time
        {
            get
            {
                return context.Time;
            }
        }

        public IList<Hash> Posts
        {
            get { return context.Posts.Select(p => p.ToHash()).ToList(); }
        }

        public string Title
        {
            get { return context.Title; }
        }

        public SiteContextDrop(SiteContext context)
        {
            this.context = context;
        }

        public Hash ToHash()
        {
            if (!context.Config.ContainsKey("date"))
                context.Config.Add("date", "2012-01-01");
            var x = Hash.FromDictionary(context.Config);
            x.Add("posts", Posts);
            x.Add("pages", context.Pages);
            x.Add("title", context.Title);
            x.Add("tags", context.Tags);
            x.Add("categories", context.Categories);
            x.Add("time", Time);

            return x;
        }
    }
}