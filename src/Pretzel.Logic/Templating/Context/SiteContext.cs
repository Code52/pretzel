using System;
using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Context
{
    public class SiteContext
    {
        public string SourceFolder { get; set; }
        public string OutputFolder { get; set; }
        public string Title { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public IList<Page> Posts { get; set; }
        public DateTime Time { get; set; }
    }
}