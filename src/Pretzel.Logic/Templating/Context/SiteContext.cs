using System;
using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Context
{
    public class SiteContext
    {
        private const string ExcerptSeparatorDefault = "<!--more-->";

        private string engine;
        private string title;
        private string excerptSeparator = ExcerptSeparatorDefault;

        public IDictionary<string, object> Config { get; set; }
        public string SourceFolder { get; set; }
        public string OutputFolder { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IList<Page> Posts { get; set; }
        public DateTime Time { get; set; }
        public Boolean UseDrafts { get; set; }

        public List<Page> Pages { get; set; }

        public string Title
        {
            get
            {
                if(Config.Keys.Contains("title"))
                {
                    title = Config["title"].ToString();
                }
                return title;
            }
            set { title = value; }
        }

        public string ExcerptSeparator
        {
            get
            {
                if (Config.Keys.Contains("excerpt_separator"))
                {
                    excerptSeparator = Config["excerpt_separator"].ToString();
                }
                return excerptSeparator;
            }
        }

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
                        engine = (string)pretzelSettings["engine"];
                    else
                        engine = string.Empty;
                }

                return engine;
            }
        }

        public SiteContext()
        {
            Tags = new List<Tag>();
            Categories = new List<Category>();
            Posts = new List<Page>();
            Pages = new List<Page>();
            Config = new Dictionary<string, object>();
        }

    }
}