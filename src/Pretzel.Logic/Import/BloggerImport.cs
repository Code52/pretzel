using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions;
using System.Xml.Linq;
using Pretzel.Logic.Extensions;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Import
{
    public class BloggerImport
    {
        private readonly IFileSystem fileSystem;
        private readonly string pathToSite;
        private readonly string pathToImportFile;

        public BloggerImport(IFileSystem fileSystem, string pathToSite, string pathToImportFile)
        {
            this.fileSystem = fileSystem;
            this.pathToSite = pathToSite;
            this.pathToImportFile = pathToImportFile;
        }

        public void Import()
        {
            var xml = fileSystem.File.ReadAllText(pathToImportFile);
            var root = XElement.Parse(xml);

            // key bits of the atom xml format:
            // <feed>
            //    <category /> - can be many
            //    <title /> = title of blog
            //    <author><name /></author>
            //    <entry>
            //       <id/>
            //       <category scheme='http://schemas.google.com/g/2005#kind' term='http://schemas.google.com/blogger/2008/kind#post'/>
            //       <category scheme='http://www.blogger.com/atom/ns#' term='A Category'/>
            //       <published/> formatted like this: 2007-02-01T14:01:23.326Z
            //       <updated/>
            //       <title/>
            //       <content type='html'/>
            //       <author><name /></author>
            //    <entry>

            XNamespace atom = "http://www.w3.org/2005/Atom";

            var posts = from e in root.Descendants(atom + "entry")
                        where e.Elements(atom + "category").Any(x => x.Attribute("term").Value == "http://schemas.google.com/blogger/2008/kind#post")
                        select new BloggerPost
                        {
                            Title = e.Element(atom + "title").Value,
                            //PostName = e.Element(wp + "post_name").Value,
                            Published = Convert.ToDateTime(e.Element(atom + "published").Value).ToUniversalTime(),
                            Updated = Convert.ToDateTime(e.Element(atom + "updated").Value),
                            Content = ConvertToMarkdown(e.Element(atom + "content").Value),
                            /*Tags = from t in e.Elements(atom + "category")
                                   where t.Attribute("domain").Value == "post_tag"
                                   select t.Value,*/
                            // blogger categories are more like tags
                            Tags = from t in e.Elements(atom + "category")
                                         where t.Attribute("scheme").Value == "http://www.blogger.com/atom/ns#"
                                         select t.Attribute("term").Value
                        };

            foreach (var p in posts)
            {
                ImportPost(p);
            }
        }

        private string ConvertToMarkdown(string content)
        {
            var converter = new HtmlToMarkdownConverter();
            return converter.Convert(content);
        }

        private void ImportPost(BloggerPost post)
        {
            var header = new
            {
                title = post.Title,
                date = post.Published,
                layout = "post",
                categories = post.Categories,
                tags = post.Tags
            };

            var yamlHeader = string.Format("---\r\n{0}---\r\n\r\n", header.ToYaml());
            var postContent = yamlHeader + post.Content;

            string fileName = string.Format(@"{0}-{1}.md", post.Published.ToString("yyyy-MM-dd"), post.Title); //not sure about post name
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            // replace some valid ones too
            fileName = fileName.Replace(' ', '-'); 
            fileName = fileName.Replace('\u00A0', '-');

            try
            {
                var postsPath = Path.Combine(pathToSite, "_posts");
                var path = Path.Combine(postsPath, fileName);

                if (!fileSystem.Directory.Exists(postsPath))
                {
                    fileSystem.Directory.CreateDirectory(postsPath);
                }
                fileSystem.File.WriteAllText(path, postContent);
            }
            catch (Exception e)
            {
                Tracing.Info("Failed to write out {0}", fileName);
                Tracing.Debug(e.Message);
            }
        }

        protected class BloggerPost
        {
            public string Title { get; set; }
            public DateTime Updated { get; set; }
            public DateTime Published { get; set; }
            public string Content { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<string> Categories { get; set; }
        }
    }
}
