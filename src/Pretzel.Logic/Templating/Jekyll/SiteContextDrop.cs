using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Extensions;

namespace Pretzel.Logic.Templating.Jekyll.Liquid
{
    public class SiteContextDrop : Drop
    {
        private readonly SiteContext context;

        public DateTime Time
        {
            get
            {
                return context.Time;
            }
        }

        public string Title
        {
            get { return context.Title; }
        }

        public Dictionary<string, object> Data { get; set; }

        public SiteContextDrop(SiteContext context)
        {
            this.context = context;
            FillData(Path.Combine(context.SourceFolder, "_data"));
        }

        public Hash ToHash()
        {
            var x = Hash.FromDictionary(context.Config.ToDictionary());
            x["posts"] = context.Posts.Select(p => p.ToHash()).ToList();
            x["pages"] = context.Pages.Select(p => p.ToHash()).ToList();
            x["title"] = context.Title;
            x["tags"] = context.Tags;
            x["categories"] = context.Categories;
            x["time"] = Time;
            x["data"] = Data;

            return x;
        }

        private void FillData(string dataPath)
        {
            if (Directory.Exists(dataPath))
            {
                Data = Directory.EnumerateFiles(dataPath, "*.yml", SearchOption.AllDirectories)
                    .Select(dataFilePath =>
                    {
                        string dataFile = File.ReadAllText(dataFilePath);
                        var yaml = dataFile.ParseYaml();

                        return new KeyValuePair<string, object>(Path.GetFileNameWithoutExtension(dataFilePath), yaml);
                    })
                    .ToDictionary(source => source.Key, source => source.Value);
            }
            else
                Data = new Dictionary<string, object>();
        }
    }
}
