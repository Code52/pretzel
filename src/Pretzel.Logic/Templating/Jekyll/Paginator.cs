using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Extensions;

namespace Pretzel.Logic.Templating.Jekyll
{
    public class Paginator : Drop
    {
        private readonly SiteContext site;

        public int total_pages { get; set; }
        public int total_posts { get; set; }
        public int per_page { get; set; }
        public int previous_page { get { return page - 1; } }
        public int next_page { get { return page + 1; } }
        public int page { get; set; }

        private IList<Hash> posts;
        public IList<Hash> Posts
        {
            get { return posts ?? (posts = site.Posts.Skip(page*per_page).Take(per_page).Select(p => p.ToHash()).ToList()); }
        }

        public Paginator(SiteContext site, int offset =0)
        {
            this.site = site;
            per_page = Convert.ToInt32(site.Config["paginate"]);
            total_pages = (int)Math.Ceiling(site.Posts.Count / Convert.ToDouble(site.Config["paginate"]));
            total_posts = site.Pages.Count;
            page = offset;
        }
    }
}