using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Templating.Context
{
    public class PageContext
    {
        public PageContext(SiteContext context, Page page)
        {
            Site = context;
            Page = page;
        }

        public string Title { get; set; }
        public string OutputPath { get; set; }
        public IDictionary<string, object> Bag { get; set; }
        public string Content { get; set; }
        public SiteContext Site { get; private set; }
        public Page Page { get; set; }
        public Page Previous { get; set; }
        public Page Next { get; set; }

        public bool Comments
        {
            get { return Bag.ContainsKey("comments") ? bool.Parse(Bag["comments"].ToString()) : false; }
        }

        //public static PageContext FromDictionary(SiteContext siteContext, IDictionary<string, object> metadata, string outputPath, string defaultOutputPath)
        //{
        //    var context = new PageContext(siteContext, TODO)
        //                      {
        //                          OutputPath =
        //                              metadata.ContainsKey("permalink")
        //                                  ? Path.Combine(outputPath, metadata["permalink"].ToString().ToRelativeFile())
        //                                  : defaultOutputPath
        //                      };


        //    if (metadata.ContainsKey("title"))
        //    {
        //        context.Title = metadata["title"].ToString();
        //    }

        //    context.Bag = metadata;

        //    return context;
        //}

        public static PageContext FromPage(SiteContext siteContext, Page page, string outputPath, string defaultOutputPath)
        {
            var context = new PageContext(siteContext, page);

            if (page.Bag.ContainsKey("permalink"))
            {
                context.OutputPath = Path.Combine(outputPath, page.Url.ToRelativeFile());
            }
            else
            {
                context.OutputPath = defaultOutputPath;
                page.Bag.Add("permalink", page.File);
            }
            if (page.Bag.ContainsKey("title"))
            {
                context.Title = page.Bag["title"].ToString();
            }

            context.Content = page.Content;
            context.Bag = page.Bag;
            context.Bag.Add("id", page.Id);
            context.Bag.Add("url", page.Url);
            return context;
        }
    }
}