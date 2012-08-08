using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Import
{
    public class TumblrImport
    {
        private readonly IFileSystem fileSystem;
        private readonly string pathToSite;
        private readonly string url;
        private readonly Func<string, string> webClient; 


        public TumblrImport(IFileSystem fileSystem, Func<string, string> webClient, string pathToSite, string url)
        {
            this.fileSystem = fileSystem;
            this.webClient = webClient;
            this.pathToSite = pathToSite;
            this.url = url;
        }

        public void Import()
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("url parameter is required.");
            }

            const int numPostsToLoad = 10;
            int total = numPostsToLoad;

            for (int i = 0; i < total; i += numPostsToLoad)
            {
                var uri = new UriBuilder(url)
                {
                    Path = "/api/read",
                    Query = string.Format("start={0}&num={1}", i, numPostsToLoad)
                };

                Console.WriteLine("Importing tumblr site from host: {0}", uri);
                var xml = webClient(uri.ToString());
                var root = XElement.Parse(xml);

                var posts = root.Descendants("posts").First();

                total = (int) posts.Attribute("total");

                foreach (var post in posts.Elements())
                {
                    var urlWithSlug = (string) post.Attribute("url-with-slug");
                    var permalink = new Uri(urlWithSlug).PathAndQuery + "/index.html";
                    

                    var title = (string) post.Element("regular-title");
                    var regularBody = (string) post.Element("regular-body");
                    var date = (DateTime) post.Attribute("date");

                    var outputPath = Path.Combine(pathToSite, "_posts", 
                            string.Format("{0}-{1}.md", date.ToString("yyyy-MM-dd"), title.Replace(" ", "-")));

                    var header = new
                    {
                        title,
                        date = date.ToString("yyyy-MM-dd"),
                        layout = "post",
                        permalink
                    };

                    var yamlHeader = string.Format("---\r\n{0}---\r\n\r\n", header.ToYaml());
                    var postContent = yamlHeader + regularBody;

                    fileSystem.File.WriteAllText(outputPath, postContent);
                }
            }
        }
    }
}
