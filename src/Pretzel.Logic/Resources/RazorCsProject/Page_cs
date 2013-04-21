using System;
using System.Collections.Generic;
using DotLiquid;

// ReSharper disable CheckNamespace
namespace Pretzel.Logic.Templating.Context
{
    public class Page : Drop
    {
        public Page()
        {
            Bag = new Dictionary<string, object>();
            Categories = new List<string>();
            Tags = new List<string>();
        }

        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Content { get; set; }
        public string Filepath { get; set; }
        public IDictionary<string, object> Bag { get; set; }
        public IEnumerable<Page> DirectoryPages { get; set; }
        public string File { get; set; }
        public string OutputFile { get; set; }

        public string Layout
        {
            get { return (string)Bag["layout"]; }
        }
    }
}