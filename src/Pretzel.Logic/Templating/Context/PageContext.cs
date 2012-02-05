using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Templating.Context
{
    public class PageContext
    {
        public string Title { get; set; }
        public string OutputPath { get; set; }

        public string Content { get; set; }

        public static PageContext FromDictionary(IDictionary<string, object> metadata, string outputPath, string defaultOutputPath)
        {
            var context = new PageContext();

            if (metadata.ContainsKey("permalink"))
            {
                context.OutputPath = Path.Combine(outputPath, metadata["permalink"].ToString().ToRelativeFile());
            }
            else
            {
                context.OutputPath = defaultOutputPath;
            }

            if (metadata.ContainsKey("title"))
            {
                context.Title = metadata["title"].ToString();
            }

            return context;
        }
    }
}