using System;
using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Context
{
    public class SiteContext
    {
        public IDictionary<string, object> Config;
        private string engine;

        public SiteContext()
        {
            Tags = new List<Tag>();
            Posts = new List<Page>();
            Pages = new List<Page>();
        }

        public string SourceFolder { get; set; }
        public string OutputFolder { get; set; }
        public string Title { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public IList<Page> Posts { get; set; }
        public DateTime Time { get; set; }

        public List<Page> Pages { get; set; }

        public string Engine
        {
            get
            { 
                if (engine == null)
                {
                    if (!Config.ContainsKey("pretzel"))
                    {
                        engine = string.Empty;
                        return engine;
                    }

                    var pretzelSettings = Config["pretzel"] as Dictionary<string, object>;
                    if (pretzelSettings != null && pretzelSettings.ContainsKey("engine"))
                        engine = (string) pretzelSettings["engine"];
                    else
                        engine = string.Empty;
                }

                return engine;
            }
        }
    }
}