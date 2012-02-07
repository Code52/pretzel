using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Templating.Context
{
    public class PageContext
    {
        public string Title { get; set; }
        public string OutputPath { get; set; }
        public IDictionary<string, object> Bag { get; set; }
        public string Content { get; set; }

        public static PageContext FromDictionary(IDictionary<string, object> metadata, string outputPath, string defaultOutputPath)
        {
            var context = new PageContext
                              {
                                  OutputPath =
                                      metadata.ContainsKey("permalink")
                                          ? Path.Combine(outputPath, metadata["permalink"].ToString().ToRelativeFile())
                                          : defaultOutputPath
                              };


            if (metadata.ContainsKey("title"))
            {
                context.Title = metadata["title"].ToString();
            }

            context.Bag = metadata;

            return context;
        }

        public static PageContext FromPage(Page page, string outputPath, string defaultOutputPath)
        {
            var context = new PageContext();

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